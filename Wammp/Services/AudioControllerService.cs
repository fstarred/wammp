using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;

namespace Wammp.Services
{
    public enum CHANNEL_TYPE
    {
        STREAM,
        MUSIC,
        REMOTE_URL
    }

    public enum LOOP_MODE
    {
        NONE,
        TRACK,
        PLAYLIST
    }

    public class AudioControllerService
    {
        public AudioControllerService()
        {
            timer = new BASSTimer(TIMER_MS_INTERVAL);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        #region Events

        public delegate void StatusChangedEventHandler(BASSActive status);

        public event StatusChangedEventHandler StatusChanged;

        public void OnStatusChanged(BASSActive status)
        {
            if (StatusChanged != null)
                StatusChanged(status);
        }

        public delegate void StreamCreatedEventHandler(int channel);

        public event StreamCreatedEventHandler StreamCreated;

        public void OnStreamCreated(int channel)
        {
            if (StreamCreated != null)
                StreamCreated(channel);
        }

        public delegate void MetaUpdatedEventHandler(TAG_INFO tagInfo);

        public event MetaUpdatedEventHandler MetaUpdated;

        public void OnMetaUpdated(TAG_INFO tagInfo)
        {
            if (MetaUpdated != null)
                MetaUpdated(tagInfo);
        }

        public delegate void StreamEventHandler();

        public event StreamEventHandler StreamEnded;

        public void OnStreamEnded()
        {
            if (StreamEnded != null)
                StreamEnded();            
        }

        #endregion

        #region Sync

        SYNCPROC syncProcEnd;

        SYNCPROC syncMetaUpdated;

        int mySyncHandleEnd;

        int mySyncHandleMetaUpdate;

        #endregion

        #region DownloadProd

        DOWNLOADPROC downloadProc;

        #endregion


        private void Timer_Tick(object sender, EventArgs e)
        {
            BASSActive status = GetStreamStatus();

            if (status != CurrentStatus)
            {
                CurrentStatus = status;
                OnStatusChanged(status);
            }
        }

        public int Stream { get; private set; }
        
        public int StreamMixer { get; private set; }

        public int StreamPlugged { get; private set; }

        public long LengthInBytes { get; private set; }

        public double LengthInSeconds { get; private set; }

        public string FileSupportedExtFilter { get; private set; }

        public BASS_CHANNELINFO ChannelInfo { get; private set; }

        public TAG_INFO TagInfo { get; private set; }

        public CHANNEL_TYPE ChannelType { get; private set; }

        public LOOP_MODE LoopMode { get; private set; }

        public string LibPath { get; private set; }

        BASSActive CurrentStatus { get; set; }

        BASSTimer timer;

        int[] fxEQ = { };

        const int TIMER_MS_INTERVAL = 100;

        static AudioControllerService current;

        /// <summary>
        /// Lazy created Singleton instance of the container for simple scenarios
        /// </summary>
        public static AudioControllerService Current
        {
            get
            {
                return current ?? (current = new AudioControllerService());
            }
        }

        public Dictionary<int, string> PluginsLoaded { get; private set; }

        public void RegisterBass(string mail, string code)
        {
            if (!String.IsNullOrEmpty(mail) && !String.IsNullOrEmpty(code))
            {
                BassNet.Registration(mail, code);
            }

        }

        public bool Init()
        {
            IntPtr handle = IntPtr.Zero;

            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string targetPath;

            if (Un4seen.Bass.Utils.Is64Bit)
                targetPath = Path.Combine(appPath, "lib/x64");
            else
                targetPath = Path.Combine(appPath, "lib/x86");

            // EXTERNAL PATH SEEMS TO NOT LONGER WORK
            targetPath = appPath;

            this.LibPath = targetPath;
            
            bool isBassLoadOk = Bass.LoadMe(targetPath);
            bool isBassMixLoadOk = BassMix.LoadMe(targetPath);

            if (!(isBassLoadOk && isBassMixLoadOk))
                throw new Exception(String.Format("Init error: {0}", Bass.BASS_ErrorGetCode().ToString()));

            this.PluginsLoaded = Bass.BASS_PluginLoadDirectory(targetPath);
            
            const string allSupportedAudioFilesWord = "All supported Audio Files";
            const string playlistExtFilter = "*.pls;*.m3u;*.impls";

            string fileSupportedExtFilter = Un4seen.Bass.Utils.BASSAddOnGetPluginFileFilter(PluginsLoaded, allSupportedAudioFilesWord);

            Regex allSupportedFilesPattern = new Regex(allSupportedAudioFilesWord + @"\|(.*?)\|");
            Match match = allSupportedFilesPattern.Match(fileSupportedExtFilter);

            string allSupportedFiles = match.Value;

            allSupportedFiles = String.Format("{0};{1}|", allSupportedFiles.Substring(0, allSupportedFiles.Length - 1), playlistExtFilter);

            fileSupportedExtFilter = fileSupportedExtFilter.Replace(match.Value, allSupportedFiles);

            fileSupportedExtFilter += String.Format("|Playlist|{0}", playlistExtFilter);

            this.FileSupportedExtFilter = fileSupportedExtFilter;

            bool config = Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 0);

            downloadProc = new DOWNLOADPROC(DownloadProcCallback);

            BASS_DEVICEINFO[] devices = Bass.BASS_GetDeviceInfos();

            int devnum = 0;

            foreach (BASS_DEVICEINFO device in devices)
            {
                bool init = Bass.BASS_Init(devnum++, 44100, BASSInit.BASS_DEVICE_DEFAULT, handle);
                if (!init)
                    throw new Exception(String.Format("Init error: {0}", Bass.BASS_ErrorGetCode().ToString()));
            }
                        
            return true;
        }

        public void SetProxy(string server, int port, string user = null, string password = null)
        {
            IntPtr _proxyHandle = IntPtr.Zero;
            string _netProxy = String.IsNullOrEmpty(user) ?
                String.Format("{0}:{1}", server, port) :
                String.Format("{0}:{1}@{2}:{3}", user, password, server, port);

            // set it
            _proxyHandle = Marshal.StringToHGlobalAnsi(_netProxy);
            bool output = Bass.BASS_SetConfigPtr(BASSConfig.BASS_CONFIG_NET_PROXY, _proxyHandle);

            System.Diagnostics.Debug.WriteLine("proxy set: " + output);

            //// on dispose call this to free it
            //if (_proxyHandle != IntPtr.Zero)
            //    Marshal.FreeHGlobal(_proxyHandle);
        }

        public void CreateMixerChannel()
        {
            int mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_MIXER_END);

            this.StreamMixer = mixer;

            if (ChannelType != CHANNEL_TYPE.REMOTE_URL)
            {
                syncProcEnd = new SYNCPROC(SyncProcEndCallback);

                mySyncHandleEnd = Bass.BASS_ChannelSetSync(mixer, BASSSync.BASS_SYNC_END, 0, syncProcEnd, IntPtr.Zero);
            }
        }

        public void SetLoopMode(LOOP_MODE mode)
        {
            int handle = this.Stream;

            switch (mode)
            {
                case LOOP_MODE.TRACK:
                    Bass.BASS_ChannelFlags(handle, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                    break;
                case LOOP_MODE.NONE:
                case LOOP_MODE.PLAYLIST:
                    if ((Bass.BASS_ChannelFlags(handle, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_DEFAULT) & BASSFlag.BASS_SAMPLE_LOOP) == BASSFlag.BASS_SAMPLE_LOOP)
                        Bass.BASS_ChannelFlags(handle, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_SAMPLE_LOOP);
                    break;
            }

            this.LoopMode = mode;
        }

        public void Stop()
        {
            if (Stream != Bass.FALSE)
            {
                Bass.BASS_ChannelStop(this.StreamMixer);
                Bass.BASS_ChannelSetPosition(this.StreamMixer, 0);
            }

            FreeResources();
        }

        void SyncProcEndCallback(int handle, int channel, int data, IntPtr user)
        {
            OnStreamEnded();
        }

        void DownloadProcCallback(IntPtr buffer, int length, IntPtr user)
        {
            if (buffer != IntPtr.Zero && length == 0)
            {
                // the buffer contains HTTP or ICY tags.
                string text = Marshal.PtrToStringAnsi(buffer);
                System.Diagnostics.Debug.WriteLine(text);
            }
        }

        void SyncMetaCallback(int handle, int channel, int data, IntPtr user)
        {
            if (TagInfo.tagType == BASSTag.BASS_TAG_WMA)
            {
                string[] tags = Un4seen.Bass.Utils.IntPtrToArrayNullTermUtf8(Bass.BASS_ChannelGetTags(channel, BASSTag.BASS_TAG_WMA_META));
                foreach (string tag in tags)
                {
                    if (tag.StartsWith("Title="))
                    {
                        TagInfo.title = tag.Replace("Title=", string.Empty);
                        break;
                    }
                }
            }
            else
            {
                // BASS_SYNC_META is triggered on meta changes of SHOUTcast streams
                bool isUpdated = TagInfo.UpdateFromMETA(Bass.BASS_ChannelGetTags(channel, BASSTag.BASS_TAG_META), true, TagInfo.tagType == BASSTag.BASS_TAG_META);
            }

            if (TagInfo.title.Equals(";StreamUrl='';"))
                TagInfo.title = String.Empty;

            //this._currentTrack.UpdateTrackFromTagInfo(tagInfo);

            OnMetaUpdated(TagInfo);
        }

        public bool IsMusicModule()
        {
            if (this.ChannelInfo == null) return false;

            return (ChannelInfo.ctype & BASSChannelType.BASS_CTYPE_MUSIC_MOD) == BASSChannelType.BASS_CTYPE_MUSIC_MOD;
        }

        public void SetVolume(float volume)
        {
            bool isMusicModule = IsMusicModule();

            //forced to false
            isMusicModule = false;

            BASSAttribute attrib;
            int stream;

            if (isMusicModule)
            {
                attrib = BASSAttribute.BASS_ATTRIB_MUSIC_VOL_GLOBAL;
                stream = this.Stream;
            }
            else
            {
                attrib = BASSAttribute.BASS_ATTRIB_VOL;
                stream = this.StreamMixer;
            }

            Bass.BASS_ChannelSetAttribute(stream, attrib, volume);
        }

        public void SetPan(float value)
        {
            BASSAttribute attrib;
            int stream;

            bool isModule = IsMusicModule();

            isModule = false;

            if (isModule)
            {
                attrib = BASSAttribute.BASS_ATTRIB_MUSIC_PANSEP;
                stream = this.Stream;
            }
            else
            {
                attrib = BASSAttribute.BASS_ATTRIB_PAN;
                stream = this.StreamMixer;
            }

            Bass.BASS_ChannelSetAttribute(stream, attrib, value);
        }

        public long GetPosition()
        {
            long pos = 0;

            if (this.StreamMixer != Bass.FALSE)
                pos = Bass.BASS_ChannelGetPosition(this.StreamPlugged);

            return pos;
        }

        public void SetPosition(long value)
        {
            Bass.BASS_ChannelSetPosition(this.StreamPlugged, value);
        }

        public BASSActive GetStreamStatus()
        {
            return Bass.BASS_ChannelIsActive(this.StreamMixer);
        }

        //public long 

        public void FreeResources()
        {
            if (StreamMixer != Bass.FALSE
                && Stream != Bass.FALSE
                && Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_STOPPED)
            {
                BassMix.BASS_Mixer_ChannelRemove(StreamPlugged);
            }
            
            Bass.BASS_StreamFree(Stream);
            //Bass.BASS_StreamFree(StreamPlugged);

            Stream = Bass.FALSE;
            StreamPlugged = Bass.FALSE;
        }

        public bool AddMixerChannel(int source)
        {
            int mixer = this.StreamMixer;

            this.StreamMixer = mixer;

            this.StreamPlugged = source;
            
            return BassMix.BASS_Mixer_StreamAddChannel(mixer, source, BASSFlag.BASS_MIXER_NORAMPIN);            
        }

        public bool Play(bool repeat)
        {
            return Bass.BASS_ChannelPlay(this.StreamMixer, repeat);
        }

        public bool Pause()
        {
            bool status = Bass.BASS_ChannelPause(this.StreamMixer);

            return status;
        }

        public void LoadFile(string file)
        {
            ChannelType = CHANNEL_TYPE.STREAM;

            this.Stop();

            int stream = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);

            if (stream == Bass.FALSE)
            {
                stream = Bass.BASS_MusicLoad(file, 0L, 0, BASSFlag.BASS_MUSIC_DECODE | BASSFlag.BASS_MUSIC_FLOAT | BASSFlag.BASS_MUSIC_PRESCAN | BASSFlag.BASS_MUSIC_POSRESETEX | BASSFlag.BASS_MUSIC_RAMP, 0);

                if (stream != Bass.FALSE)
                {
                    ChannelType = CHANNEL_TYPE.MUSIC;
                }
            }

            if (stream == Bass.FALSE)
            {
                stream = Bass.BASS_StreamCreateURL(file, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_STATUS, downloadProc, IntPtr.Zero);                
                if (stream != Bass.FALSE)
                {
                    ChannelType = CHANNEL_TYPE.REMOTE_URL;                    
                }
            }

            TAG_INFO tagInfo = new TAG_INFO(file);
            BASS_CHANNELINFO channelInfo = stream != Bass.FALSE ? 
                Bass.BASS_ChannelGetInfo(stream) :
                new BASS_CHANNELINFO();

            this.Stream = stream;

            if (stream != Bass.FALSE)
            {
                bool isTagAvailable = ChannelType == CHANNEL_TYPE.REMOTE_URL ? BassTags.BASS_TAG_GetFromURL(stream, tagInfo) : BassTags.BASS_TAG_GetFromFile(stream, tagInfo);

                this.LengthInBytes = Bass.BASS_ChannelGetLength(stream);
                this.LengthInSeconds = Bass.BASS_ChannelBytes2Seconds(stream, this.LengthInBytes);
            }

            this.TagInfo = tagInfo;
            
            this.ChannelInfo = channelInfo;

            if (ChannelType == CHANNEL_TYPE.REMOTE_URL)
            {
                BASSTag tagType = TagInfo.tagType;

                BASSSync syncFlag = syncFlag = BASSSync.BASS_SYNC_META;

                if (tagType == BASSTag.BASS_TAG_WMA)
                    syncFlag = BASSSync.BASS_SYNC_WMA_META;

                bool isWMA = false;

                if (channelInfo.ctype == BASSChannelType.BASS_CTYPE_STREAM_WMA)
                    isWMA = true;
                // ok, do some pre-buffering...
                System.Diagnostics.Debug.WriteLine("Buffering...");
                if (!isWMA)
                {
                    // display buffering for MP3, OGG...
                    while (true)
                    {
                        long len = Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_END);
                        if (len == -1)
                            break; // typical for WMA streams
                                   // percentage of buffer filled
                        float progress = (
                            Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_DOWNLOAD) -
                            Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_CURRENT)
                            ) * 100f / len;

                        if (progress > 75f)
                        {
                            break; // over 75% full, enough
                        }

                        System.Diagnostics.Debug.WriteLine(String.Format("Buffering... {0}%", progress));
                    }
                }
                else
                {
                    // display buffering for WMA...
                    while (true)
                    {
                        long len = Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_WMA_BUFFER);
                        if (len == -1L)
                            break;
                        // percentage of buffer filled
                        if (len > 75L)
                        {
                            break; // over 75% full, enough
                        }

                        System.Diagnostics.Debug.WriteLine(String.Format("Buffering... {0}%", len));
                    }
                }

                // get the meta tags (manually - will not work for WMA streams here)
                string[] icy = Bass.BASS_ChannelGetTagsICY(stream);
                if (icy == null)
                {
                    // try http...
                    icy = Bass.BASS_ChannelGetTagsHTTP(stream);
                }
                if (icy != null)
                {
                    foreach (string tag in icy)
                    {
                        System.Diagnostics.Debug.WriteLine("ICY: " + tag);
                    }
                }
                // get the initial meta data (streamed title...)
                icy = Bass.BASS_ChannelGetTagsMETA(stream);
                if (icy != null)
                {
                    foreach (string tag in icy)
                    {
                        System.Diagnostics.Debug.WriteLine("Meta: " + tag);
                    }
                }
                else
                {
                    // an ogg stream meta can be obtained here
                    icy = Bass.BASS_ChannelGetTagsOGG(stream);
                    if (icy != null)
                    {
                        foreach (string tag in icy)
                        {
                            System.Diagnostics.Debug.WriteLine("Meta: " + tag);
                        }
                    }
                }

                syncMetaUpdated = new SYNCPROC(SyncMetaCallback);

                mySyncHandleMetaUpdate = Bass.BASS_ChannelSetSync(stream, syncFlag, 0, syncMetaUpdated, IntPtr.Zero);

            }                        
            
            OnStreamCreated(stream);
        }

        public void InitEQ(float[] eqItems)
        {
            fxEQ = new int[eqItems.Length];

            for (int i = 0; i < eqItems.Length; i++)
            {
                fxEQ[i] = Bass.BASS_ChannelSetFX(this.Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0);

                BASS_DX8_PARAMEQ eq = new BASS_DX8_PARAMEQ();
                eq.fCenter = eqItems[i];

                Bass.BASS_FXSetParameters(fxEQ[i], eq);
            }
        }


        public void SetEQ(int index, float gain)
        {
            BASS_DX8_PARAMEQ eq = new BASS_DX8_PARAMEQ();
            if (fxEQ.Length > 0)
                if (Bass.BASS_FXGetParameters(fxEQ[index], eq))
                {
                    eq.fGain = gain;
                    Bass.BASS_FXSetParameters(fxEQ[index], eq);
                }
        }

        public double GetElapsedTime()
        {
            return Bass.BASS_ChannelBytes2Seconds(this.Stream, GetPosition());
        }

        public string GetElapsedTimeString()
        {
            return Un4seen.Bass.Utils.FixTimespan(GetElapsedTime(), "MMSS");
        }

        public string GetTotalTimeString()
        {
            return Un4seen.Bass.Utils.FixTimespan(this.LengthInSeconds, "MMSS");
        }

        public string GetRemainingTimeString()
        {
            return String.Format("-{0}", Un4seen.Bass.Utils.FixTimespan((this.LengthInSeconds - GetElapsedTime()), "MMSS"));
        }
    }
}

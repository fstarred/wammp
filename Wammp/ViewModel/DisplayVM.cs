using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TinyIoC;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using Wammp.Naming;
using Wammp.Services;
using WammpCommons.Utils;
using WammpCommons.ViewModel;

namespace Wammp.ViewModel
{
    public class DisplayVM : BaseViewModel
    {
        public DisplayVM()
        {
            if (!Utility.IsDesignMode())
            {

                AudioControllerService.Instance.StatusChanged += Instance_StatusChanged;
                AudioControllerService.Instance.StreamCreated += Instance_StreamCreated;
                AudioControllerService.Instance.MetaUpdated += Instance_MetaUpdated;
                TracklistProvider.Instance.IndexChanged += Instance_IndexChanged;
                TracklistProvider.Instance.Tracks.CollectionChanged += Tracks_CollectionChanged;

                Reload();

                BASSTimer timer = new BASSTimer();
                timer.Tick += _timer_Tick;
                timer.Interval = TIMER_MS_INTERVAL;
                //timer.Interval = new TimeSpan(0, 0, 0, 0, TIMER_MS_INTERVAL);
                timer.Start();

                CurrentSpectrumDisplay = WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.SPECTRUM_LINE;      
            }

            
        }

        public event EventHandler MetaUpdatedEvent;

        private void Instance_MetaUpdated(Un4seen.Bass.AddOn.Tags.TAG_INFO tagInfo)
        {
            RootDispatcher.Dispatcher.Invoke(() => 
            {
                this.Title = AudioControllerService.Instance.TagInfo.title;
                this.Artist = AudioControllerService.Instance.TagInfo.artist;
                this.Album = AudioControllerService.Instance.TagInfo.album;
                this.StreamInfo = AudioControllerService.Instance.ChannelInfo.ToString();
                this.LengthMMSS = AudioControllerService.Instance.GetTotalTimeString();
                this.LengthInBytes = AudioControllerService.Instance.LengthInBytes;

                if (MetaUpdatedEvent != null)
                {
                    System.Diagnostics.Debug.WriteLine("Firing MetaUpdatedEvent");
                    MetaUpdatedEvent(AudioControllerService.Instance.TagInfo, null);
                }

            });
            
            //SetVolume(volume);

            //AudioControllerService.Instance.SetPan(pan);            
        }

        private void Instance_StatusChanged(BASSActive status)
        {
            switch (status)
            {
                case BASSActive.BASS_ACTIVE_PAUSED:
                    PlayerStatus = PLAYER_STATUS.PAUSED;
                    break;
                case BASSActive.BASS_ACTIVE_PLAYING:
                    PlayerStatus = PLAYER_STATUS.PLAYING;
                    break;
                case BASSActive.BASS_ACTIVE_STALLED:
                    PlayerStatus = PLAYER_STATUS.STALLED;
                    break;
                case BASSActive.BASS_ACTIVE_STOPPED:
                    PlayerStatus = PLAYER_STATUS.STOPPED;
                    break;
            }
        }

        private void Instance_StreamCreated(int channel)
        {
            this.Title = AudioControllerService.Instance.TagInfo.title;
            this.Artist = AudioControllerService.Instance.TagInfo.artist;
            this.Album = AudioControllerService.Instance.TagInfo.album;
            this.StreamInfo = AudioControllerService.Instance.ChannelInfo.ToString();
            this.LengthMMSS = AudioControllerService.Instance.GetTotalTimeString();
            this.LengthInBytes = AudioControllerService.Instance.LengthInBytes;

            TAG_INFO tagInfo = AudioControllerService.Instance.TagInfo;

            if (tagInfo.PictureCount > 0)
            { 
                //System.Drawing.Image image = AudioControllerService.Instance.TagInfo.PictureGetImage(0);
                this.PictureData = AudioControllerService.Instance.TagInfo.PictureGet(0).Data;
            }
            else
            {
                this.PictureData = null;
            }

            SetVolume(volume);

            AudioControllerService.Instance.SetPan(pan);

            RaisePropertyChanged(() => Stream);
        }

        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(() => TracksCount);
        }

        private void Instance_IndexChanged(int index)
        {
            RaisePropertyChanged(() => CurrentIndex);
        }
        
        const int TIMER_MS_INTERVAL = 200;

        void RegisterMessages()
        {

        }

        void _timer_Tick(object sender, EventArgs e)
        {
            BASSActive status = AudioControllerService.Instance.GetStreamStatus();

            //if (status != _lastStatus)
            //{
            //    _lastStatus = status;
            //    _audioPlayer_StatusChanged();
            //}

            switch (status)
            {
                case BASSActive.BASS_ACTIVE_PLAYING:

                    //PlayerStatus = PLAYER_STATUS.PLAYING;
                    
                    RaisePropertyChanged(() => Position);

                    string time;

                    if (DisplayMode == DISPLAY_MODE.CURRENT_TIME)
                        time = AudioControllerService.Instance.GetElapsedTimeString();
                    else
                        time = AudioControllerService.Instance.GetRemainingTimeString();

                    //BPM = _audioPlayer.BPM.ToString("#0");

                    DisplayedTime = time;

                    break;
                case BASSActive.BASS_ACTIVE_PAUSED:

                    //PlayerStatus = PLAYER_STATUS.PAUSED;
                    
                    break;
                case BASSActive.BASS_ACTIVE_STALLED:
                    //StreamPercent = _audioPlayer.GetStreamProgress();

                    //PlayerStatus = PLAYER_STATUS.STALLED;
                    
                    break;
                case BASSActive.BASS_ACTIVE_STOPPED:

                    //PlayerStatus = PLAYER_STATUS.STOPPED;
                    
                    break;
            }
        }

        public int CurrentIndex
        {
            get { return TracklistProvider.Instance.CurrentIndex + 1; }
        }

        private byte[] pictureData;

        public byte[] PictureData
        {
            get { return pictureData; }
            set {
                pictureData = value;
                RaisePropertyChanged(() => PictureData);
            }
        }

        public int Stream
        {
            get { return AudioControllerService.Instance.StreamMixer; }
        }

        public int TracksCount
        {
            get { return TracklistProvider.Instance.Tracks.Count; }
        }


        public LOOP_MODE LoopMode
        {
            get { return AudioControllerService.Instance.LoopMode; }
        }

        private PLAYER_STATUS playerStatus;

        public PLAYER_STATUS PlayerStatus
        {
            get { return playerStatus; }
            set
            {
                playerStatus = value;
                RaisePropertyChanged(() => PlayerStatus);
            }
        }

        private WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY currentSpectrumDisplay;

        public WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY CurrentSpectrumDisplay
        {
            get { return currentSpectrumDisplay; }
            set
            {
                currentSpectrumDisplay = value;
                RaisePropertyChanged(() => CurrentSpectrumDisplay);
            }
        }

        private float volume;

        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                RaisePropertyChanged(() => Volume);
                SetVolume(volume);
            }
        }

        private string displayedTime;

        public string DisplayedTime
        {
            get
            {
                return displayedTime;
            }
            set
            {
                displayedTime = value;
                RaisePropertyChanged(() => DisplayedTime);
            }
        }

        private DISPLAY_MODE displayMode;

        public DISPLAY_MODE DisplayMode
        {
            get
            {
                return displayMode;
            }
            set
            {
                displayMode = value;
                RaisePropertyChanged(() => DisplayMode);
            }
        }

        public long Position
        {
            get
            {
                return AudioControllerService.Instance.GetPosition();
            }
            set
            {
                AudioControllerService.Instance.SetPosition(value);
                RaisePropertyChanged(() => Position);
            }
        }

        private float pan;
        public float Pan
        {
            get
            {
                return pan;
            }
            set
            {
                pan = value;
                RaisePropertyChanged(() => Pan);
                AudioControllerService.Instance.SetPan(pan);
            }
        }

        private ObservableCollection<PluginVM> plugins;

        public ObservableCollection<PluginVM> Plugins
        {
            get { return plugins; }
            set
            {
                plugins = value;
                RaisePropertyChanged(() => Plugins);
            }
        }

        private long lengthInBytes;

        public long LengthInBytes
        {
            get { return lengthInBytes; }
            set
            {
                lengthInBytes = value;
                RaisePropertyChanged(() => LengthInBytes);
            }
        }

        private string lengthMMSS;

        public string LengthMMSS
        {
            get { return lengthMMSS; }
            set
            {
                lengthMMSS = value;
                RaisePropertyChanged(() => LengthMMSS);
            }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private string artist;

        public string Artist
        {
            get { return artist; }
            set
            {
                artist = value;
                RaisePropertyChanged(() => Artist);
            }
        }

        private string album;

        public string Album
        {
            get { return album; }
            set
            {
                album = value;
                RaisePropertyChanged(() => Album);
            }
        }

        private string streamInfo;

        public string StreamInfo
        {
            get { return streamInfo; }
            set
            {
                streamInfo = value;
                RaisePropertyChanged(() => StreamInfo);
            }
        }

        void Reload()
        {
            var container = TinyIoCContainer.Current;

            IAudioConfigProvider serviceAudio = container.Resolve<IAudioConfigProvider>(ContainerNSR.AUDIO_SETTINGS);

            serviceAudio.Load();

            this.Volume = serviceAudio.Volume;
            this.Pan = serviceAudio.Pan;            
        }        

        private void SetVolume(float volume)
        {
            //if (_isMuteMode == false || volume == 0)
            AudioControllerService.Instance.SetVolume(volume);
        }

        void OpenFile(string[] files)
        {
            if (files.Length > 0)
            {
                List<string> supportedFiles = new List<string>();

                foreach (string path in files)
                {
                    if (Un4seen.Bass.Utils.BASSAddOnIsFileSupported(AudioControllerService.Instance.PluginsLoaded, path) || path.StartsWith("http"))
                    {
                        supportedFiles.Add(path);
                    }
                    else if (Utils.AudioUtility.IsPlaylist(path))
                    {
                        supportedFiles.AddRange(Utils.AudioUtility.GetTracksFromPlaylistFile(path, AudioControllerService.Instance.PluginsLoaded));
                    }                    
                }

                IEnumerable<TrackVM> tracks = new List<TrackVM>(supportedFiles.Select((i, o) => new TrackVM(i)));

                TracklistProvider.Instance.ClearTracks();

                if (tracks.Count() > 0)
                {
                    TracklistProvider.Instance.AddTracks(tracks.ToArray());

                    TracklistProvider.Instance.SetCurrentIndex(0);

                    AudioControllerService.Instance.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
                }
            }
        }

        void OpenFile()
        {
            var container = TinyIoCContainer.Current;

            IDialogFileService service = container.Resolve<IDialogFileService>(ContainerNSR.DLG_OPEN_MULTIPLE_FILE);

            string[] files = service.OpenMultipleFiles();

            OpenFile(files);
        }

        void SwitchDisplayMode()
        {
            switch (displayMode)
            {
                case DISPLAY_MODE.CURRENT_TIME:
                    DisplayMode = DISPLAY_MODE.REMAINING_TIME;
                    break;
                case DISPLAY_MODE.REMAINING_TIME:
                    DisplayMode = DISPLAY_MODE.CURRENT_TIME;
                    break;
            }

        }

        void Backward()
        {
            int currentIndex = TracklistProvider.Instance.CurrentIndex;

            if (TracklistProvider.Instance.Tracks.Count > 0)
            {
                if (currentIndex > 0)
                    TracklistProvider.Instance.SetCurrentIndex(currentIndex - 1);
                else
                    TracklistProvider.Instance.SetCurrentIndex(TracklistProvider.Instance.Tracks.Count - 1);

                AudioControllerService.Instance.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
            }
        }

        void Forward()
        {
            if (TracklistProvider.Instance.Tracks.Count > 0)
            {
                int lastTrackIndex = TracklistProvider.Instance.Tracks.Count - 1;

                if (TracklistProvider.Instance.CurrentIndex < lastTrackIndex)
                {
                    TracklistProvider.Instance.SetCurrentIndex(TracklistProvider.Instance.CurrentIndex + 1);
                }
                else
                {
                    TracklistProvider.Instance.SetCurrentIndex(0);
                }

                AudioControllerService.Instance.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
            }
        }

        void OpenFolder()
        {
            //TODO
        }

        void Play()
        {
            BASSActive status = AudioControllerService.Instance.GetStreamStatus();

            switch (status)
            {
                case BASSActive.BASS_ACTIVE_PLAYING:

                    AudioControllerService.Instance.Pause();

                    break;
                case BASSActive.BASS_ACTIVE_PAUSED:

                    AudioControllerService.Instance.Play(false);                    

                    break;
                default:

                    if (TracklistProvider.Instance.Tracks.Count > 0)
                    {
                        if (TracklistProvider.Instance.CurrentIndex < 0)
                        {
                            TracklistProvider.Instance.SetCurrentIndex(0);
                            //MessengerProvider.Instance.NotifyColleagues(MessengerNSR.TRACKLIST_INDEX_CHANGED, currentIndex);
                        }
                        //AudioControllerService.Instance.Play(true);
                        AudioControllerService.Instance.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
                    }

                    break;
            }

            status = AudioControllerService.Instance.GetStreamStatus();

            if (status != BASSActive.BASS_ACTIVE_PLAYING && TracklistProvider.Instance.Tracks.Count == 0)
            {
                OpenFile();                
            }
        }

        void Stop()
        {
            AudioControllerService.Instance.Stop();
        }

        void ChangeLoopMode()
        {
            switch (AudioControllerService.Instance.LoopMode)
            {
                case LOOP_MODE.NONE:
                    AudioControllerService.Instance.SetLoopMode(LOOP_MODE.TRACK);
                    break;
                case LOOP_MODE.TRACK:
                    AudioControllerService.Instance.SetLoopMode(LOOP_MODE.PLAYLIST);
                    break;
                case LOOP_MODE.PLAYLIST:
                    AudioControllerService.Instance.SetLoopMode(LOOP_MODE.NONE);
                    break;
            }

            RaisePropertyChanged(() => LoopMode);
        }


        void ChangeSpectrumDisplay()
        {
            switch (currentSpectrumDisplay)
            {
                case WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.NONE:
                    currentSpectrumDisplay = WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.SPECTRUM_LINE;
                    break;
                case WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.SPECTRUM_LINE:
                    currentSpectrumDisplay = WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.WAVE_FORM;
                    break;
                case WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.WAVE_FORM:
                    currentSpectrumDisplay = WpfControlLibraryBass.Elements.SpectrumAnalyzer.DISPLAY.NONE;
                    break;
            }

            RaisePropertyChanged(() => CurrentSpectrumDisplay);
        }

        void SaveAudioSettings()
        {
            System.Diagnostics.Debug.WriteLine("Saving display settings");

            var container = TinyIoCContainer.Current;

            IAudioConfigProvider serviceAudio = container.Resolve<IAudioConfigProvider>(ContainerNSR.AUDIO_SETTINGS);

            serviceAudio.Load();

            serviceAudio.Volume = this.volume;
            serviceAudio.Pan = this.pan;

            serviceAudio.Save();
        }

        void Drop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                OpenFile(files);                
            }
        }

        void DragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    e.Effects = DragDropEffects.All;
                }
            }
        }
        
        void OpenUrl()
        {
            var container = TinyIoCContainer.Current;

            IOpenUrl service = container.Resolve<IOpenUrl>(ContainerNSR.DLG_OPEN_URL);

            string address = service.OpenUrl();

            if (!String.IsNullOrEmpty(address))
            {
                OpenFile(new string[] { address });
            }
        }

        public ICommand OpenFileCommand { get { return new RelayCommand(OpenFile); } }
        public ICommand OpenFolderCommand { get { return new RelayCommand(OpenFolder); } }
        public ICommand SwitchDisplayModeCommand { get { return new RelayCommand(SwitchDisplayMode); } }
        public ICommand BackwardCommand { get { return new RelayCommand(Backward); } }
        public ICommand ForwardCommand { get { return new RelayCommand(Forward); } }
        public ICommand PlayCommand { get { return new RelayCommand(Play); } }
        public ICommand StopCommand { get { return new RelayCommand(Stop); } }
        public ICommand ChangeLoopModeCommand { get { return new RelayCommand(ChangeLoopMode); } }
        public ICommand ChangeSpectrumDisplayCommand { get { return new RelayCommand(ChangeSpectrumDisplay); } }
        public ICommand SaveAudioSettingsCommand { get { return new RelayCommand(SaveAudioSettings); } }
        public ICommand DragEnterCommand { get { return new MvvmFoundation.Wpf.RelayCommand<DragEventArgs>(DragEnter); } }
        public ICommand DropCommand { get { return new MvvmFoundation.Wpf.RelayCommand<DragEventArgs>(Drop); } }
        public ICommand OpenUrlCommand { get { return new MvvmFoundation.Wpf.RelayCommand(OpenUrl); } }
    }
}

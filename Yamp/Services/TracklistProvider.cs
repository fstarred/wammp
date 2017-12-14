using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using Yemp.Components;
using Yemp.ViewModel;

namespace Yemp.Services
{
    public class TracklistProvider
    {
        public const int NO_TRACK_SELECTED_INDEX = -1;

        private TracklistProvider()
        {
            Tracks = new ItemsChangeObservableCollection<TrackVM>();
            Tracks.CollectionChanged += TracklistProvider_CollectionChanged;
            SetCurrentIndex(NO_TRACK_SELECTED_INDEX);
        }


        public delegate void IndexChangedEventHandler(int index);

        public event IndexChangedEventHandler IndexChanged;

        public void OnIndexChanged(int index)
        {
            if (IndexChanged != null)
                IndexChanged(index);
        }
        
        public delegate void TrackTagInfoUpdatedEventHandler(TrackVM trackVM, TAG_INFO tagInfo);

        public event TrackTagInfoUpdatedEventHandler TrackTagInfoUpdated;

        public void OnTrackTagInfoUpdated(TrackVM trackVM, TAG_INFO tagInfo)
        {
            if (TrackTagInfoUpdated != null)
                TrackTagInfoUpdated(trackVM, tagInfo);
        }
        

        private void TracklistProvider_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {                        
            if (AllowCollectionChanged || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                int index = Tracks.IndexOf(CurrentTrackReference);
                SetCurrentIndex(index);
            }
            else
            {
                throw new InvalidOperationException("tracks modification is only permitted by provider methods");
            }
        }

        static TracklistProvider instance;

        public static TracklistProvider Instance
        {
            get
            {
                return instance ?? (instance = new TracklistProvider());
            }
        }

        bool AllowCollectionChanged { get; set; }

        TrackVM CurrentTrackReference { get; set; }

        public int CurrentIndex { get; private set; }

        public TrackVM GetCurrentTrack()
        {
            return this.CurrentTrackReference;
        }

        public ItemsChangeObservableCollection<TrackVM> Tracks { get; private set; }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ClearTracks()
        {
            AllowCollectionChanged = true;

            this.Tracks.Clear();
            
            AllowCollectionChanged = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveTracks(params TrackVM[] tracks)
        {
            AllowCollectionChanged = true;

            foreach (TrackVM item in tracks)
            {
                this.Tracks.Remove(item);
            }

            if (this.CurrentIndex >= this.Tracks.Count)
                this.SetCurrentIndex(NO_TRACK_SELECTED_INDEX);

            AllowCollectionChanged = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddTracks(params TrackVM[] tracks)
        {
            AllowCollectionChanged = true;
            
            foreach (TrackVM item in tracks)
            {
                this.Tracks.Add(item);
            }
            
            AllowCollectionChanged = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void MoveTrack(int oldindex, int newindex)
        {
            AllowCollectionChanged = true;

            Tracks.Move(oldindex, newindex);

            AllowCollectionChanged = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetCurrentIndex(int index)
        {
            this.CurrentIndex = index;
            this.CurrentTrackReference = index >= 0 ? Tracks.ElementAt(index) : null;
            OnIndexChanged(index);
        }

        public TAG_INFO UpdateTrackTagInfo(TrackVM track)
        {
            string path = track.Location;

            bool ret = false;

            bool isURL = Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);

            int stream = Bass.FALSE;

            if (isURL == false)
            {
                stream = Bass.BASS_StreamCreateFile(path, 0L, 0L, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO);

                if (stream == Bass.FALSE)
                {
                    stream = Bass.BASS_MusicLoad(path, 0L, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_MUSIC_PRESCAN, 0);
                }
            }
            else
            {
                stream = Bass.BASS_StreamCreateURL(path, 0, BASSFlag.BASS_STREAM_DECODE, null, IntPtr.Zero);
            }

            TAG_INFO tagInfo = new TAG_INFO(path);

            bool isTagAvailable = false;

            if (stream != Bass.FALSE)
                isTagAvailable = isURL ? BassTags.BASS_TAG_GetFromURL(stream, tagInfo) : BassTags.BASS_TAG_GetFromFile(stream, tagInfo);

            double length = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));            

            Bass.BASS_StreamFree(stream);

            ret = isTagAvailable;

            OnTrackTagInfoUpdated(track, tagInfo);

            return tagInfo; ;
        }
        
    }
}

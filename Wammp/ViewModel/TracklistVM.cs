using MvvmFoundation.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TinyIoC;
using Un4seen.Bass;
using Wammp.Components;
using Wammp.Naming;
using Wammp.Services;
using WammpCommons.Model;
using WammpCommons.Utils;
using WammpCommons.ViewModel;

namespace Wammp.ViewModel
{
    class TracklistVM : BaseViewModel
    {
        Task task = null;

        public TracklistVM()
        {
            if (!Utility.IsDesignMode())
            {
                AudioControllerService.Current.StatusChanged += Instance_StatusChanged;

                TracklistProvider.Instance.IndexChanged += Instance_IndexChanged;
                TracklistProvider.Instance.Tracks.CollectionChanged += Tracks_CollectionChanged;                

                Reload();
                RegisterMessages();

                RaisePropertyChanged(() => Tracks);                
            }
        }

        public event System.EventHandler MessageUpdated;

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

        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //this.Tracks = (ObservableCollection<TrackVM>)sender;                        
            //System.Diagnostics.Debug.WriteLine("Tracks_CollectionChanged");
            //RaisePropertyChanged(() => Tracks);
        }

        private void Instance_IndexChanged(int index)
        {
            RaiseObjectChangedSafeInvoker(() => CurrentIndex);

            if (TracklistProvider.Instance.CurrentIndex != TracklistProvider.NO_TRACK_SELECTED_INDEX)
            {
                TriggerSafeEvent(MessageUpdated, new MessageEventArgs
                    {
                        Message = string.Format("{0} - {1}", CurrentIndex, TracklistProvider.Instance.GetCurrentTrack().Title)
                    }
                );                
            }            
        }

        void RegisterMessages()
        {
            //MessengerProvider.Instance.Register<PLAYER_STATUS>(MessengerNSR.PLAYER_STATUS_CHANGED_MESSAGE, i =>
            //{
            //    PlayerStatus = i;
            //});
            //MessengerProvider.Instance.Register<IEnumerable<TrackVM>>(MessengerNSR.TRACKLIST_CHANGED_MESSAGE, i =>
            //{
            //    this.Tracks.Clear();
            //    this.Tracks = new ObservableCollection<TrackVM>(i);
            //});
            //MessengerProvider.Instance.Register<int>(MessengerNSR.TRACKLIST_INDEX_CHANGED, i =>
            //{
            //    CurrentIndex = i;
            //});
        }

        void Reload()
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            service.Load();

            //MessengerProvider.Instance.NotifyColleagues(MessengerNSR.TRACKLIST_CHANGED_MESSAGE, this.tracks);
        }

        public int CurrentIndex
        {
            get { return TracklistProvider.Instance.CurrentIndex + 1; }
        }

        public IEnumerable<TrackVM> SelectedTracks
        {
            get { return Tracks.Where((o) => o.IsSelected); }           
        }

       
        //private ObservableCollection<TrackVM> tracks;

        //public ObservableCollection<TrackVM> Tracks {
        //    get
        //    {
        //        return tracks;
        //    }
        //    set
        //    {
        //        tracks = value;
        //        RaisePropertyChanged(() => Tracks);
        //    }
        //}

        public ItemsChangeObservableCollection<TrackVM> Tracks
        {
            get
            {
                return TracklistProvider.Instance.Tracks;
            }
        }
        

            private PLAYER_STATUS playerStatus;

        public PLAYER_STATUS PlayerStatus
        {
            get
            {
                return playerStatus;
            }
            set
            {
                playerStatus = value;
                RaisePropertyChanged(() => PlayerStatus);
            }
        }

        void AddTrack(string[] files)
        {
            if (files.Length > 0)
            {
                List<string> supportedFiles = new List<string>();

                foreach (string path in files)
                {
                    if (Un4seen.Bass.Utils.BASSAddOnIsFileSupported(AudioControllerService.Current.PluginsLoaded, path) || path.StartsWith("http"))
                    {
                        supportedFiles.Add(path);
                    }
                    else if (Utils.AudioUtility.IsPlaylist(path))
                    {
                        supportedFiles.AddRange(Utils.AudioUtility.GetTracksFromPlaylistFile(path, AudioControllerService.Current.PluginsLoaded));
                    }
                }

                IEnumerable<TrackVM> tracks = new List<TrackVM>(supportedFiles.Select((i, o) => new TrackVM(i)));

                if (tracks.Count() > 0)
                {
                    TracklistProvider.Instance.AddTracks(tracks.ToArray());
                }
            }
        }

        void AddTrack()
        {
            var container = TinyIoCContainer.Current;

            IDialogFileService service = container.Resolve<IDialogFileService>(ContainerNSR.DLG_OPEN_MULTIPLE_FILE);

            string[] files = service.OpenMultipleFiles();

            AddTrack(files);
        }

        void Play()
        {
            BASSActive status = AudioControllerService.Current.GetStreamStatus();

            if (SelectedTracks.Count() > 0 && status == BASSActive.BASS_ACTIVE_STOPPED)
            {
                TracklistProvider.Instance.SetCurrentIndex(Tracks.IndexOf(SelectedTracks.First()));
                AudioControllerService.Current.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
            }
            else
            {
                switch (status)
                {
                    case BASSActive.BASS_ACTIVE_PLAYING:

                        AudioControllerService.Current.Pause();

                        break;
                    case BASSActive.BASS_ACTIVE_PAUSED:

                        AudioControllerService.Current.Play(false);

                        break;
                    default:

                        if (TracklistProvider.Instance.Tracks.Count > 0)
                        {
                            if (TracklistProvider.Instance.CurrentIndex < 0)
                            {
                                TracklistProvider.Instance.SetCurrentIndex(0);
                            }
                            AudioControllerService.Current.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
                        }

                        break;
                }

                if (status != BASSActive.BASS_ACTIVE_PLAYING && TracklistProvider.Instance.Tracks.Count == 0)
                {
                    AddTrack();

                    if (TracklistProvider.Instance.Tracks.Count > 0)
                    {
                        TracklistProvider.Instance.SetCurrentIndex(0);
                        AudioControllerService.Current.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
                        //AudioControllerService.Instance.Play(false);
                    }
                }
            }
        }

        void RemoveTrack()
        {
            TracklistProvider.Instance.RemoveTracks(SelectedTracks.ToArray());
        }

        void MoveTrack(DIRECTION direction)
        {
            //var selectedTracksOrdered = from selecteditem in SelectedTracks
            //           orderby Tracks.IndexOf(selecteditem)
            //           select selecteditem;

            IEnumerable<TrackVM> selectedTracksOrdered;

            switch (direction)
            {
                case DIRECTION.UP:

                    selectedTracksOrdered = SelectedTracks.OrderBy(i => Tracks.IndexOf(i));

                    foreach (TrackVM trackVM in selectedTracksOrdered)
                    {
                        int oldIndex = Tracks.IndexOf(trackVM);

                        if (oldIndex == 0)
                            break;

                        TracklistProvider.Instance.MoveTrack(oldIndex, oldIndex - 1);
                    }

                    break;
                case DIRECTION.DOWN:

                    selectedTracksOrdered = SelectedTracks.OrderByDescending(i => Tracks.IndexOf(i));

                    int lastTrackIndex = TracklistProvider.Instance.Tracks.Count - 1;

                    foreach (TrackVM trackVM in selectedTracksOrdered)
                    {
                        int oldIndex = Tracks.IndexOf(trackVM);

                        if (oldIndex == lastTrackIndex)
                            break;

                        TracklistProvider.Instance.MoveTrack(oldIndex, oldIndex + 1);
                    }

                    break;
            }
        }

        void SavePlaylist()
        {
            new PlaylistService().Save();
        }

        void ClearTracklist()
        {
            TracklistProvider.Instance.ClearTracks();
        }

        void Stop()
        {
            AudioControllerService.Current.Stop();
        }

        void PlaySelectedTrack()
        {
            AudioControllerService.Current.Stop();
            Play();
        }



        void Drop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                AddTrack(files);
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

        void UpdateInfo()
        {
            if (task == null || task.Status != TaskStatus.Running)
            {
                var container = TinyIoC.TinyIoCContainer.Current;

                IMessageProvider messageProvider = container.Resolve<IMessageProvider>(Naming.ContainerNSR.MESSAGE_PROVIDER);

                task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < SelectedTracks.Count(); i++)
                    {
                        TrackVM item = SelectedTracks.ElementAt(i);
                        TracklistProvider.Instance.UpdateTrackTagInfo(item);
                    }
                }).ContinueWith(o => 
                {
                    RootDispatcher.Dispatcher.Invoke(() =>
                    {
                        string message = messageProvider.TrackInfoUpdated;
                        
                        if (MessageUpdated != null)
                            MessageUpdated(this, new MessageEventArgs { Message = message });
                    });
                });
            }            
        }

        void AddRemoteTrack()
        {
            var container = TinyIoCContainer.Current;

            IOpenUrl service = container.Resolve<IOpenUrl>(ContainerNSR.DLG_OPEN_URL);

            string address = service.OpenUrl();

            if (!System.String.IsNullOrEmpty(address))
            {
                AddTrack(new string[] { address });
            }
        }


        public ICommand MoveTrackCommand { get { return new RelayCommand<DIRECTION>(MoveTrack); } }
        public ICommand PlayCommand { get { return new RelayCommand(Play); } }
        public ICommand PlaySelectedTrackCommand { get { return new RelayCommand(PlaySelectedTrack); } }
        public ICommand StopCommand { get { return new RelayCommand(Stop); } }
        public ICommand AddTrackCommand { get { return new RelayCommand(AddTrack); } }
        public ICommand RemoveTrackCommand { get { return new RelayCommand(RemoveTrack); } }
        public ICommand SavePlaylistCommand { get { return new RelayCommand(SavePlaylist); } }
        public ICommand ClearTracklistCommand { get { return new RelayCommand(ClearTracklist); } }
        public ICommand DragEnterCommand { get { return new MvvmFoundation.Wpf.RelayCommand<DragEventArgs>(DragEnter); } }
        public ICommand DropCommand { get { return new MvvmFoundation.Wpf.RelayCommand<DragEventArgs>(Drop); } }
        public ICommand UpdateInfoCommand { get { return new MvvmFoundation.Wpf.RelayCommand(UpdateInfo); } }
        public ICommand AddRemoteTrackCommand { get { return new MvvmFoundation.Wpf.RelayCommand(AddRemoteTrack); } }

    }
}
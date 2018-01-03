using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using TinyIoC;
using Un4seen.Bass;
using Wammp.Model;
using Wammp.Naming;
using Wammp.Services;
using WammpCommons.Business;
using WammpCommons.Model;
using WammpCommons.Services;
using WammpCommons.Utils;
using WammpCommons.ViewModel;
using WammpPluginContracts;

namespace Wammp.ViewModel
{
    public enum DISPLAY_MODE
    {
        CURRENT_TIME,
        REMAINING_TIME
    }

    public enum PLAYER_STATUS
    {
        STOPPED,
        PLAYING,
        PAUSED,
        STALLED
    };

    class MainVM : BaseViewModel
    {
        public MainVM()
        {
            this.Plugins = new List<PluginVM>();

            if (!Utility.IsDesignMode())
            {
                var container = TinyIoCContainer.Current;

                CheckBassRegistration();

                bool init = AudioControllerService.Instance.Init();

                CheckProxySettings();

                AudioControllerService.Instance.CreateMixerChannel();

                AudioControllerService.Instance.MetaUpdated += Instance_MetaUpdated;
                AudioControllerService.Instance.StreamCreated += Instance_StreamCreated;
                AudioControllerService.Instance.StreamEnded += Instance_StreamEnded;
                AudioControllerService.Instance.StatusChanged += Instance_StatusChanged;

                TracklistProvider.Instance.IndexChanged += Instance_IndexChanged;
                TracklistProvider.Instance.Tracks.CollectionChanged += Tracks_CollectionChanged;
                TracklistProvider.Instance.TrackTagInfoUpdated += Instance_TrackTagInfoUpdated;

                
                //LoadPlugins();
                Reload();

                RegisterMessages();

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new System.TimeSpan(0, 0, 0, 0, MS_TIMER_TRACK_INFO_UPDATE);
                timer.Tick += Timer_Tick;
                timer.Start();

                //IAppUpdateCheckService service = container.Resolve<IAppUpdateCheckService>(ContainerNSR.UPDATE_CHECK_SERVICE);

                //service.NewVersionFoundEvent += Service_NewVersionFoundEvent;

                //CheckForUpdates();
            }
        }

        private void Instance_StatusChanged(BASSActive status)
        {
            

            switch (status)
            {
                case BASSActive.BASS_ACTIVE_STOPPED:
                    foreach (PluginVM plugin in Plugins)
                    {
                        plugin.Plugin.ChannelDispose();
                    }
                break;
            }
        }

        public event System.EventHandler RequestHomepageEvent;
        public event System.EventHandler PluginInitFailedEvent;

        void Service_NewVersionFoundEvent(object sender, System.EventArgs e)
        {
            VersionCheckEventArgs eventArgs = (VersionCheckEventArgs)e;

            var container = TinyIoCContainer.Current;

            IDialogMessage service = container.Resolve<IDialogMessage>(ContainerNSR.DLG_OPEN_MESSAGE);

            MSG_RESPONSE response = MSG_RESPONSE.CANCEL;

            RootDispatcher.Dispatcher.Invoke(() =>
            {
                response = service.ShowMessage(string.Format("A new version ( {0} ) is available. Do you want to download it?", eventArgs.Version), "Version information", true);

                if (response == MSG_RESPONSE.OK)
                {
                    if (RequestHomepageEvent != null)
                        RequestHomepageEvent(sender, e);
                }
            });

        }


        void CheckForUpdates()
        {
            var container = TinyIoCContainer.Current;
            
            IResourceProvider resource = container.Resolve<IResourceProvider>(ContainerNSR.RESOURCE_PROVIDER);

            IConfigProvider config = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            ServiceUpdater updater = new ServiceUpdater();

            IWebProxy proxy = null;

            if (config.EnableProxy)
            {
                proxy = new System.Net.WebProxy(config.Host, config.Port);
                if (config.EnableCredentials)
                    proxy.Credentials = new System.Net.NetworkCredential(config.User, config.Password, config.Domain);
            }

            System.Version currentVersion = Utility.GetVersionInfo(this.GetType().Assembly);
            
            Task.Factory.StartNew(() => {

                ServiceUpdater.VersionInfo version = updater.GetMetaInfoVersion(resource.VersionCheckUri.ToString());

                return version;

            }).ContinueWith((o) => {

                if (o.Status != TaskStatus.Faulted)
                {
                    System.Version latestVersion = o.Result.LatestVersion;

                    bool isVersionUpToDate = latestVersion <= currentVersion;

                    VersionCheckEventArgs eventArgs = new VersionCheckEventArgs { Version = latestVersion };

                    if (isVersionUpToDate == false)
                    {
                        System.Diagnostics.Debug.WriteLine("New version found: " + eventArgs.Version.ToString());

                        IDialogMessage service = container.Resolve<IDialogMessage>(ContainerNSR.DLG_OPEN_MESSAGE);

                        MSG_RESPONSE response = MSG_RESPONSE.CANCEL;

                        RootDispatcher.Dispatcher.Invoke((System.Action)delegate
                        {
                            response = service.ShowMessage(String.Format("A new version ( {0} ) is available. Do you want to download it?", eventArgs.Version), "Version information", true);

                            if (response == MSG_RESPONSE.OK)
                            {
                                if (RequestHomepageEvent != null)
                                    RequestHomepageEvent(this, eventArgs);
                            }
                        });
                    }
                }
                
            });
                
                
        }

        void CheckBassRegistration()
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            service.Load(); 
            
            if (!(string.IsNullOrEmpty(service.BassUser) || string.IsNullOrEmpty(service.BassCode)))
            {
                AudioControllerService.Instance.RegisterBass(service.BassUser, service.BassCode);
            }
        }

        void CheckProxySettings()
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            service.Load();

            if (service.EnableProxy || service.EnableCredentials)
            {
                AudioControllerService.Instance.SetProxy(service.Host, service.Port, service.User, service.Password);
            }
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {            
            System.Diagnostics.Debug.WriteLine("Starting track update...");            

            if (task == null || task.Status != TaskStatus.Running)
            {
                TrackVM[] tracks = TracklistProvider.Instance.Tracks.Where(x => x.Location.StartsWith(("http"))).ToArray();

                task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < tracks.Length; i++)
                    {
                        TrackVM item = tracks[i];
                        TracklistProvider.Instance.UpdateTrackTagInfo(item);
                    }
                });
            }            
        }

        private void Instance_MetaUpdated(Un4seen.Bass.AddOn.Tags.TAG_INFO tagInfo)
        {
            RootDispatcher.Dispatcher.Invoke(new System.Action(() =>
            {
                System.Diagnostics.Debug.WriteLine("Track meta updated");

                TrackVM track = TracklistProvider.Instance.GetCurrentTrack();

                track.Title = tagInfo.title;
                track.Album = tagInfo.album;
                track.Artist = tagInfo.artist;
                track.Length = Un4seen.Bass.Utils.FixTimespan(tagInfo.duration, "MMSS");                
            }));

            foreach (PluginVM item in Plugins)
            {
                if (item.IsEnabled)
                {
                    item.Plugin.RetrieveInfo(TracklistProvider.Instance.GetCurrentTrack().Location, AudioControllerService.Instance.ChannelInfo, tagInfo);                   
                }
            }
        }

        private void Instance_TrackTagInfoUpdated(TrackVM trackVM, Un4seen.Bass.AddOn.Tags.TAG_INFO tagInfo)
        {
            RootDispatcher.Dispatcher.Invoke(new System.Action(() =>
            {
                System.Diagnostics.Debug.WriteLine("Instance_TrackTagInfoUpdated: " + tagInfo.title);

                ObservableCollection<TrackVM> tracks = TracklistProvider.Instance.Tracks;

                int index = tracks.IndexOf(trackVM);
                if (index >= 0)
                {
                    tracks[index].Title = tagInfo.title;
                    tracks[index].Artist = tagInfo.artist;
                    tracks[index].Album = tagInfo.album;
                    tracks[index].Length = Un4seen.Bass.Utils.FixTimespan(tagInfo.duration, "MMSS");
                }

            }));

        }



        Task task = null;

        const int MS_TIMER_TRACK_INFO_UPDATE = 3 * 60 * 1000;

        // Help UI to get notified using dispatcher asyncronously
        private void Tracks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                //if (task == null || task.Status != TaskStatus.Running)
                //{
                    TrackVM[] tracks = (e.NewItems.Cast<TrackVM>()).ToArray();

                    task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        for (int i = 0; i < tracks.Length; i++)
                        {
                            TrackVM item = tracks[i];
                            TracklistProvider.Instance.UpdateTrackTagInfo(item);
                        }
                    });                    
                //}
            }

            foreach (PluginVM item in Plugins)
            {
                if (item.IsEnabled)
                {
                    item.Plugin.TracklistUpdated(TracklistProvider.Instance.Tracks.Select(o => o.Location).ToArray());
                }
            }
        }


        private void Instance_StreamCreated(int channel)
        {
            int stream = AudioControllerService.Instance.Stream;

            foreach (PluginVM item in Plugins)
            {
                if (item.IsEnabled)
                {
                    item.Plugin.RetrieveInfo(TracklistProvider.Instance.GetCurrentTrack().Location, AudioControllerService.Instance.ChannelInfo, AudioControllerService.Instance.TagInfo);

                    int pStream = item.Plugin.PrepareStream(stream);

                    if (pStream != Bass.FALSE)
                        stream = pStream;
                }
            }

            AudioControllerService.Instance.AddMixerChannel(stream);

            //AudioControllerService.Instance.SetPosition(0);

            AudioControllerService.Instance.Play(false);
        }

        private void Instance_StreamEnded()
        {
            LOOP_MODE loopMode = AudioControllerService.Instance.LoopMode;

            int currentTrackIndex = TracklistProvider.Instance.CurrentIndex;

            if (loopMode != LOOP_MODE.TRACK)
            {
                bool isLastTrack = (currentTrackIndex + 1) < TracklistProvider.Instance.Tracks.Count == false;

                if (isLastTrack)
                {
                    if (loopMode == LOOP_MODE.PLAYLIST)
                    {
                        if (TracklistProvider.Instance.Tracks.Count > 0)
                        {
                            TracklistProvider.Instance.SetCurrentIndex(0);
                            AudioControllerService.Instance.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
                        }
                    }
                    else
                        AudioControllerService.Instance.Stop();
                }

                const bool autoForwardTracklist = true;

                if (autoForwardTracklist)
                {
                    if (isLastTrack == false)
                    {
                        TracklistProvider.Instance.SetCurrentIndex(currentTrackIndex + 1);
                        AudioControllerService.Instance.LoadFile(TracklistProvider.Instance.GetCurrentTrack().Location);
                    }
                }

            }
        }

        private void Instance_IndexChanged(int index)
        {
            BASSActive status = AudioControllerService.Instance.GetStreamStatus();

            if (TracklistProvider.Instance.CurrentIndex == TracklistProvider.NO_TRACK_SELECTED_INDEX
                && status == BASSActive.BASS_ACTIVE_PLAYING
                || status == BASSActive.BASS_ACTIVE_PAUSED)
            {
                AudioControllerService.Instance.Stop();
            }

            foreach (PluginVM item in Plugins)
            {
                item.Plugin.CurrentTrackIndexChanged(index);
            }
        }

        void RegisterMessages()
        {

        }

        public IList<PluginVM> Plugins { get; private set; }

        void Reload()
        {
            var container = TinyIoCContainer.Current;

            IAudioConfigProvider serviceAudio = container.Resolve<IAudioConfigProvider>(ContainerNSR.AUDIO_SETTINGS);

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            serviceAudio.Load();

            service.Load();

            IEnumerable<TrackVM> list = (service.Tracks.Select((i, o) => new TrackVM(i)));

            TracklistProvider.Instance.AddTracks(list.ToArray());

            Utils.AudioUtility.SetDevice(serviceAudio.Device);
        }

        void LoadPlugins()
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            //IAppUpdateCheckService updateCheckService = container.Resolve<IAppUpdateCheckService>(ContainerNSR.UPDATE_CHECK_SERVICE);

            service.Load();

            IList<PluginVM> source = ((PluginsHandler.Instance.Plugins).Select((i, o) => new PluginVM(i))).ToList();

            foreach (PluginVM item in source)
            {
                SimplePlugin plugin = service.Plugins.Where((i, o) => i.Name.Equals(item.Plugin.Name)).FirstOrDefault();
                if (plugin != null)
                {
                    item.IsEnabled = plugin.IsEnabled;
                    item.Position = plugin.Position;
                }
            }

            this.Plugins = new List<PluginVM>(source.OrderBy(i => i.Position));

            foreach (PluginVM plugin in Plugins)
            {
                if (plugin.IsEnabled)
                {
                    bool init = plugin.Plugin.Init(AudioControllerService.Instance.LibPath, AudioControllerService.Instance.StreamMixer);
                    if (!init)
                    {
                        plugin.IsEnabled = false;

                        TriggerSafeEvent(PluginInitFailedEvent, new MessageEventArgs
                        {
                            Message = string.Format("{0} - plugin init failed", plugin.Plugin.Name)                            
                        });
                    }

                    //plugin.Plugin.UpdateCheckService = updateCheckService; // TODO should be injected instead

                    plugin.Plugin.SendActionSignal += (s, e) =>
                    {
                        SignalArgs args = (SignalArgs)e;
                        switch (args.Action)
                        {
                            case PLAYER_ACTION.RESET:                                
                                AudioControllerService.Instance.Pause();
                                AudioControllerService.Instance.SetPosition(0);                                
                                break;
                            case PLAYER_ACTION.PLAY:
                                AudioControllerService.Instance.Play(false);
                                break;
                            case PLAYER_ACTION.STOP:
                                AudioControllerService.Instance.Stop();
                                break;
                        }
                    };
                }
            }

            RaisePropertyChanged(() => Plugins);
        }

        void Init()
        {
            if (!Utility.IsDesignMode())
            {
                LoadPlugins(); // load plugins

                CheckForUpdates(); // check for updates
            }
        }

        public ICommand InitCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Init); } }
    }
}

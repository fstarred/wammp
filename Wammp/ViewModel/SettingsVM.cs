using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TinyIoC;
using Un4seen.Bass;
using Wammp.Model;
using Wammp.Naming;
using Wammp.Services;
using Wammp.Utils;
using WammpCommons.Business;
using WammpCommons.Model;
using WammpCommons.Services;
using WammpCommons.Utils;
using WammpCommons.ViewModel;

namespace Wammp.ViewModel
{
    public enum DIRECTION { UP, DOWN }

    class SettingsVM : BaseViewModel, IPasswordHandler
    {
        public event EventHandler PasswordResetEvent;
        public event EventHandler SettingsSavedEvent;
        public event EventHandler SettingsReloadEvent;
        public event EventHandler RequestHomepageEvent;

        public SettingsVM()
        {
            if (Utility.IsDesignMode() == false)
            {
                Reload();

                var container = TinyIoCContainer.Current;

                //IAppUpdateCheckService service = container.Resolve<IAppUpdateCheckService>(ContainerNSR.UPDATE_CHECK_SERVICE);

                //service.NetworkErrorEvent += Service_NetworkErrorEvent;
                //service.NewVersionFoundEvent += Service_NewVersionFoundEvent;
                //service.VersionUpToDateEvent += Service_VersionUpToDateEvent;

                TriggerSafeEvent(PasswordResetEvent);
            }
        }

        Task task = null;

        private void OnVersionUpToDateEvent(object sender, EventArgs e)
        {
            VersionCheckEventArgs eventArgs = (VersionCheckEventArgs)e;

            var container = TinyIoCContainer.Current;

            IDialogMessage service = container.Resolve<IDialogMessage>(ContainerNSR.DLG_OPEN_MESSAGE);

            RootDispatcher.Dispatcher.Invoke((Action)delegate {
                service.ShowMessage(String.Format("Version {0} is up to date", eventArgs.Version), "Version information");
            });
        }

        private void OnNewVersionFoundEvent(object sender, EventArgs e)
        {
            VersionCheckEventArgs eventArgs = (VersionCheckEventArgs)e;

            var container = TinyIoCContainer.Current;

            IDialogMessage service = container.Resolve<IDialogMessage>(ContainerNSR.DLG_OPEN_MESSAGE);

            MSG_RESPONSE response = MSG_RESPONSE.CANCEL;

            RootDispatcher.Dispatcher.Invoke((Action)delegate
            {
                response = service.ShowMessage(String.Format("A new version ( {0} ) is available. Do you want to download it?", eventArgs.Version), "Version information", true);

                if (response == MSG_RESPONSE.OK)
                {
                    if (RequestHomepageEvent != null)
                        RequestHomepageEvent(sender, e);
                }
            });
        }

        private void OnNetworkErrorEvent(object sender, EventArgs e)
        {
            VersionCheckEventArgs eventArgs = (VersionCheckEventArgs)e;

            var container = TinyIoCContainer.Current;

            IDialogMessage service = container.Resolve<IDialogMessage>(ContainerNSR.DLG_OPEN_MESSAGE);

            RootDispatcher.Dispatcher.Invoke((Action)delegate
            {
                service.ShowMessage(eventArgs.ErrorMessage, "Network error");
            });
        }

        void Save()
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            service.Load();

            service.Domain = this.domain;
            service.EnableCredentials = this.enableCredentials;
            service.EnableProxy = this.enableProxy;
            service.Host = this.host;
            service.Password = SecurityUtils.EncryptString(this.password);
            service.Port = this.port;
            service.User = this.user;
            service.BassUser = this.bassUser;
            service.BassCode = this.bassCode;
            service.SelectedTheme = this.selectedTheme;            
            service.Plugins = this.plugins.Select((i, o) => new SimplePlugin
            {
                Name = i.Plugin.Name,
                IsEnabled = i.IsEnabled,
                Position = this.plugins.IndexOf(i)                

            }).ToList();

            foreach (PluginVM item in this.plugins)
            {
                if (item.Plugin.CanLoadSave())
                    item.Plugin.Save();                
            }

            service.Save();

            IAudioConfigProvider audioservice = container.Resolve<IAudioConfigProvider>(ContainerNSR.AUDIO_SETTINGS);

            audioservice.Load();

            audioservice.Device = this.selectedDevice;

            audioservice.Save();            

            TriggerSafeEvent(SettingsSavedEvent);

            AudioUtility.SetDevice(this.selectedDevice);
        }


        void Reload()
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            service.Load();
            
            this.Domain = service.Domain;
            this.EnableCredentials = service.EnableCredentials;
            this.EnableProxy = service.EnableProxy;
            this.Host = service.Host;
            this.Password = SecurityUtils.DecryptString(service.Password);
            this.Port = service.Port;
            this.User = service.User;
            this.BassUser = service.BassUser;
            this.BassCode = service.BassCode;
            this.SelectedTheme = service.SelectedTheme;

            IList<PluginVM> source = ((PluginsHandler.Instance.Plugins).Select((i, o) => new PluginVM(i))).ToList();

            foreach (PluginVM item in source)
            {
                SimplePlugin plugin = service.Plugins.Where((i, o) => i.Name.Equals(item.Plugin.Name)).FirstOrDefault();
                if (plugin != null)
                {
                    item.IsEnabled = plugin.IsEnabled;
                    item.Position = plugin.Position;                    
                }
                if (item.IsEnabled && item.Plugin.CanLoadSave())
                    item.Plugin.Load();
            }

            //this.Plugins = new ObservableCollection<PluginVM>(source.OrderBy(i => i.Position));
            /* avoid memory leaks (multiple calls on Helper.TabControlBehavior) */
            if (this.plugins == null)
                this.plugins = new ObservableCollection<PluginVM>();
            this.plugins.ToList().All(i => this.plugins.Remove(i));
            source.OrderBy(i => i.Position).All(o => { this.plugins.Add(o); return true; });

            IAudioConfigProvider audioservice = container.Resolve<IAudioConfigProvider>(ContainerNSR.AUDIO_SETTINGS);

            audioservice.Load();

            this.SelectedDevice = audioservice.Device;

            TriggerSafeEvent(SettingsReloadEvent);
        }

        private int selectedDevice;

        public int SelectedDevice
        {
            get { return selectedDevice; }
            set {
                selectedDevice = value;
                RaisePropertyChanged(() => SelectedDevice);
            }
        }


        public BASS_DEVICEINFO[] Devices
        {
            get { return AudioUtility.GetDevices(); }            
        }

        private string selectedTheme;

        public string SelectedTheme
        {
            get { return selectedTheme; }
            set {
                selectedTheme = value;
                RaisePropertyChanged(() => SelectedTheme);
            }
        }


        private bool enableProxy;

        public bool EnableProxy
        {
            get { return enableProxy; }
            set
            {
                enableProxy = value;
                RaisePropertyChanged(() => EnableProxy);
            }
        }

        private bool enableCredentials;

        public bool EnableCredentials
        {
            get { return enableCredentials; }
            set
            {
                enableCredentials = value;
                RaisePropertyChanged(() => EnableCredentials);
            }
        }

        private string host;

        public string Host
        {
            get { return host; }
            set
            {
                host = value;
                RaisePropertyChanged(() => Host);
            }
        }

        private int port;

        public int Port
        {
            get { return port; }
            set
            {
                port = value;
                RaisePropertyChanged(() => Port);
            }
        }

        private string user;

        public string User
        {
            get { return user; }
            set
            {
                user = value;
                RaisePropertyChanged(() => User);
            }
        }

        private string bassUser;

        public string BassUser
        {
            get { return bassUser; }
            set
            {
                bassUser = value;
                RaisePropertyChanged(() => BassUser);
            }
        }


        private string bassCode;

        public string BassCode
        {
            get { return bassCode; }
            set
            {
                bassCode = value;
                RaisePropertyChanged(() => BassCode);
            }
        }

        private SecureString password;

        public SecureString Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        private string domain;

        public string Domain
        {
            get { return domain; }
            set
            {
                domain = value;
                RaisePropertyChanged(() => Domain);
            }
        }

        private PluginVM selectedPlugin;

        public PluginVM SelectedPlugin
        {
            get { return selectedPlugin; }
            set {
                selectedPlugin = value;
                RaisePropertyChanged(() => SelectedPlugin);
            }
        }


        private ObservableCollection<PluginVM> plugins;

        public ObservableCollection<PluginVM> Plugins
        {
            get { return plugins; }
            set {
                plugins = value;
                RaisePropertyChanged(() => Plugins);
            }
        }


        void CheckForUpdates()
        {
            if (task == null || task.Status != TaskStatus.Running)
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

                task = Task.Factory.StartNew(() => {

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
                            OnNewVersionFoundEvent(this, eventArgs);
                        }
                        else
                        {
                            OnVersionUpToDateEvent(this, eventArgs);
                        }
                    }
                    else
                    {
                        VersionCheckEventArgs eventArgs = new VersionCheckEventArgs { ErrorMessage = o.Exception.Message };

                        OnNetworkErrorEvent(this, eventArgs);
                    }                    

                });
            }            
        }
        
        void ChangePluginOrder(DIRECTION input)
        {
            int index = plugins.IndexOf(selectedPlugin);
            switch (input)
            {
                case DIRECTION.UP:
                    if (index > 0)
                    {
                        Plugins.Move(index, index - 1);
                    }
                    break;
                case DIRECTION.DOWN:
                    if (index < plugins.Count - 1)
                    {
                        Plugins.Move(index, index + 1);
                    }
                    break;
            }
        }

        void TogglePlugin()
        {
            SelectedPlugin.IsEnabled = !selectedPlugin.IsEnabled;
        }

        void Dispose()
        {
            this.SettingsSavedEvent = null;
            this.SettingsReloadEvent = null;
            this.RequestHomepageEvent = null;            

            Reload();
        }

        public ICommand SaveCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Save); } }
        public ICommand ReloadCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Reload); } }
        public ICommand CheckUpdatesCommand { get { return new MvvmFoundation.Wpf.RelayCommand(CheckForUpdates); } }
        public ICommand ChangePluginOrderCommand { get { return new MvvmFoundation.Wpf.RelayCommand<DIRECTION>(ChangePluginOrder); } }
        public ICommand TogglePluginCommand { get { return new MvvmFoundation.Wpf.RelayCommand(TogglePlugin); } }
        public ICommand DisposeCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Dispose); } }

    }
}

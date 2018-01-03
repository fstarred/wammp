using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using WammpCommons.Business;
using WammpCommons.Model;
using WammpCommons.ViewModel;


// https://stackoverflow.com/questions/14932339/abstract-class-i-dont-need-to-override-all-the-methods


namespace WammpPluginContracts
{
    public abstract class APlugin : BaseViewModel, IPlugin
    {
        //public IAppUpdateCheckService UpdateCheckService { get; set; }

        protected string message;

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                RaisePropertyChanged(() => Message);
            }
        }

        public abstract string MetaInfoUrl { get; }

        public event EventHandler VersionUpToDateEvent;
        public event EventHandler NewVersionFoundEvent;
        public event EventHandler NetworkErrorEvent;

        public event EventHandler<EventArgs> SettingsSaved;
        public event EventHandler<SignalArgs> SendActionSignal;
        public event EventHandler MessageEvent;

        //The event-invoking method that derived classes can override.
        protected virtual void OnSettingsSaved(object sender, EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<EventArgs> handler = SettingsSaved;
            if (handler != null)
            {
                handler(sender, e);                
            }

            Message = "Settings saved";
        }

        //The event-invoking method that derived classes can override.
        protected virtual void OnSendActionSignal(object sender, SignalArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<SignalArgs> handler = SendActionSignal;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public abstract string AuthorName
        {
            get;
        }

        public abstract string Name 
        {
            get;
        }

        public abstract object View
        {
            get;
        }

        public abstract VIEW_TYPE ViewType
        {
            get;
        }

        public virtual IWebProxy Proxy
        {
            get;
            set;
        }

        public virtual bool CanLoadSave()
        {
            return false;
        }

        public virtual void ChannelDispose() { }

        public virtual void CurrentTrackIndexChanged(int index) { }

        public virtual byte[] Icon()
        {
            return GetIcon("WammpPluginContracts.question-circle.png", System.Reflection.Assembly.GetExecutingAssembly());
        }

        public virtual bool Init(string libpath, int mixer)
        {
            //this.GetVersionCommand = new MvvmFoundation.Wpf.RelayCommand(() =>
            //{
                //TinyIoC.TinyIoCContainer current = TinyIoC.TinyIoCContainer.Current;
            //});
            return true;
        }

        public virtual void Load() { }
        
        public abstract int PrepareStream(int source);

        public virtual void RetrieveInfo(string filename, BASS_CHANNELINFO info, TAG_INFO tagInfo) { }

        public virtual void Save() { }

        public virtual void TracklistUpdated(string[] filenames) { }

        protected virtual byte[] GetIcon(string input, System.Reflection.Assembly assembly)
        {
            byte[] buffer = null;
            using (var stream = assembly.GetManifestResourceStream(input))
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }

        ServiceUpdater updater;
        public virtual Version GetLatestVersion()
        {
            updater = new ServiceUpdater(this.Proxy);
            ServiceUpdater.VersionInfo version = updater.GetMetaInfoVersion(MetaInfoUrl);
            return version.LatestVersion;
        }

        public virtual Version GetCurrentVersion()
        {
            Assembly assembly = this.GetType().Assembly;
            FileVersionInfo output = FileVersionInfo.GetVersionInfo(assembly.Location);            
            return new Version(output.ProductVersion);
        }

        
        System.Threading.Tasks.Task task;
        public virtual bool IsVersionUptoDate()
        {
            System.Version currentVersion = GetCurrentVersion();

            if (task == null || task.Status != TaskStatus.Running)
            {
                task = Task.Factory.StartNew(() => {

                    Version version = GetLatestVersion();

                    return version;

                }).ContinueWith((o) => {

                    if (o.Status != TaskStatus.Faulted)
                    {
                        System.Version latestVersion = o.Result;

                        bool isVersionUpToDate = latestVersion <= currentVersion;

                        VersionCheckEventArgs eventArgs = new VersionCheckEventArgs { Version = latestVersion };

                        if (isVersionUpToDate == false)
                        {
                            System.Diagnostics.Debug.WriteLine("new version found");
                            TriggerSafeEvent(NewVersionFoundEvent, new MessageEventArgs
                            {
                                Message = string.Format("New version of plugin found: {0}", latestVersion.ToString())
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("version is up to date");
                            TriggerSafeEvent(VersionUpToDateEvent, new MessageEventArgs
                            {
                                Message = string.Format("version is up to date: {0}", currentVersion.ToString())
                            });
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(o.Exception.Message);
                        TriggerSafeEvent(NetworkErrorEvent, new MessageEventArgs
                        {
                            Message = string.Format(o.Exception.Message)
                        });
                        //Service_NetworkErrorEvent(this, eventArgs);
                    }

                });
            }
            

            return true;
        }
    }
}

using System.Windows;
using TinyIoC;
using System.Linq;
using Wammp.Components;
using Wammp.Naming;
using Wammp.Services;
using WammpCommons.Services;
using WammpCommons.ViewModel;
using WammpCommons.Utils;
using System.IO;

namespace Wammp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var container = TinyIoCContainer.Current;
            IMessageProvider messageProvider = new MessageResourceProvider();

            string path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "plugins");

            PluginsHandler.Instance.Load(path);

            container.Register<IDialogFileService>((c, n) =>
            {
                DialogFileService service = new DialogFileService();
                service.Filter = AudioControllerService.Current.FileSupportedExtFilter;

                return service;

            }, ContainerNSR.DLG_OPEN_MULTIPLE_FILE);

            container.Register<IDialogMessage>((c, n) =>
            {
                IDialogMessage service = new DialogMessage();

                return service;

            }, ContainerNSR.DLG_OPEN_MESSAGE);

            container.Register<IOpenUrl>((c, n) =>
            {
                IOpenUrl service = new DialogOpenUrl();

                return service;

            }, ContainerNSR.DLG_OPEN_URL);

            container.Register<IDialogFileService>((c, n) =>
            {
                DialogFileService service = new DialogFileService();
                //service.Filter = System.String.Format("Playlist file (*{0})|*{0}|M3U file (*{1})|*{1}|Impulse Media Playlist file (*{2})|*{2}", ".pls", ".m3u", ".impls"); // "wma file (*.wma)|*.wma";
                service.Filter = System.String.Format("Playlist file (*{0})|*{0}|M3U file (*{1})|*{1}", ".pls", ".m3u"); // "wma file (*.wma)|*.wma";

                return service;

            }, ContainerNSR.DLG_SAVE_PLAYLIST);

            container.Register<IConfigProvider>((c, n) =>
            {
                IConfigProvider config = new SettingsConfigProvider();
                return config;

            }, ContainerNSR.APP_SETTINGS);


            container.Register<IAudioConfigProvider>((c, n) =>
            {
                IAudioConfigProvider audioConfig = new AudioSettingsConfigProvider();
                return audioConfig;

            }, ContainerNSR.AUDIO_SETTINGS);

            container.Register<IResourceProvider>((c, n) =>
            {
                IResourceProvider resourceProvider = new ResourceProvider();
                return resourceProvider;

            }, ContainerNSR.RESOURCE_PROVIDER);

            //AppUpdateCheckService appUpdateCheckService = new AppUpdateCheckService();
            //container.Register<IAppUpdateCheckService>((c, n) =>
            //{
            //    IConfigProvider config = new SettingsConfigProvider();

            //    config.Load();

            //    System.Net.WebProxy proxy = null;

            //    if (config.EnableProxy)
            //    {
            //        proxy = new System.Net.WebProxy(config.Host, config.Port);
            //        if (config.EnableCredentials)
            //            proxy.Credentials = new System.Net.NetworkCredential(config.User, config.Password, config.Domain);
            //    }

            //    appUpdateCheckService.Uri = (System.Uri)Current.Resources["VersionCheckUri"];
            //    appUpdateCheckService.Proxy = proxy;

            //    return appUpdateCheckService;

            //}, ContainerNSR.UPDATE_CHECK_SERVICE);

            container.Register<IMessageProvider>((c, n) =>
            {
                return messageProvider;

            }, ContainerNSR.MESSAGE_PROVIDER);


            BaseViewModel.RootDispatcher = new DispatcherProviderImplementation(() => (
                App.Current != null
                    ? App.Current.Dispatcher
                    : Dispatcher)
            );

            // load initial configuration
            LoadConfig();            
        }

        void LoadConfig()
        {
            var container = TinyIoC.TinyIoCContainer.Current;

            IConfigProvider config = container.Resolve<IConfigProvider>(Naming.ContainerNSR.APP_SETTINGS);

            config.Load();

            ThemeSelector.ApplyTheme(config.SelectedTheme);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            var container = TinyIoCContainer.Current;

            IConfigProvider service = container.Resolve<IConfigProvider>(ContainerNSR.APP_SETTINGS);

            service.Load();

            service.Tracks = TracklistProvider.Instance.Tracks.Select(i => i.Location).ToList();

            service.Save();

            PluginsHandler.Instance.Dispose();            
        }
    }
}

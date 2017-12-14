using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using YempCommons.Business;
using YempCommons.Model;
using YempCommons.Services;

namespace YempCommons.Services
{
    public class AppUpdateCheckService : IAppUpdateCheckService
    {
        public event EventHandler<VersionCheckEventArgs> VersionUpToDateEvent;
        public event EventHandler<VersionCheckEventArgs> NewVersionFoundEvent;
        public event EventHandler<VersionCheckEventArgs> NetworkErrorEvent;

        public AppUpdateCheckService()
        {

        }

        public IWebProxy Proxy { get; set; }

        public Uri Uri { get; set; }

        public void CheckForUpdates(Version version, Assembly assembly)
        {
            ServiceUpdater updater = new ServiceUpdater(Proxy);

            try
            {
                ServiceUpdater.VersionInfo latestVersionInfo = updater.GetMetaInfoVersion(Uri.AbsoluteUri);

                if (latestVersionInfo != null)
                {
                    Version productVersion = new Version(FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion);

                    //Version productVersion = Utils.Utility.GetProductVersion();

                    bool isVersionUpToDate = latestVersionInfo.LatestVersion <= productVersion;

                    VersionCheckEventArgs eventArgs = new VersionCheckEventArgs
                    {
                        Version = latestVersionInfo.LatestVersion
                    };

                    if (isVersionUpToDate == false)
                    {
                        System.Diagnostics.Debug.WriteLine("New version found: " + eventArgs.Version.ToString());
                        if (NewVersionFoundEvent != null)
                            NewVersionFoundEvent(this, eventArgs);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Version is up to date: " + eventArgs.Version.ToString());
                        if (VersionUpToDateEvent != null)
                            VersionUpToDateEvent(this, eventArgs);
                    }
                }
            }
            catch (Exception e)
            {
                VersionCheckEventArgs eventArgs = new VersionCheckEventArgs { ErrorMessage = e.Message };

                if (NetworkErrorEvent != null)
                    NetworkErrorEvent(this, eventArgs);
            }
        }

    }
}

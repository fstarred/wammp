using System;
using System.Reflection;
using YempCommons.Model;

namespace YempCommons.Services
{
    public interface IAppUpdateCheckService
    {
        event EventHandler<VersionCheckEventArgs> VersionUpToDateEvent;
        event EventHandler<VersionCheckEventArgs> NewVersionFoundEvent;
        event EventHandler<VersionCheckEventArgs> NetworkErrorEvent;

        System.Net.IWebProxy Proxy { get; set; }
        
        Uri Uri { get; set; }

        void CheckForUpdates(Version version, Assembly assembly );
    }
}

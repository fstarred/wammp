using System;
using System.Reflection;
using WammpCommons.Model;

namespace WammpCommons.Services
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

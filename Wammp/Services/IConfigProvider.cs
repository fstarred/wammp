using System.Collections.Generic;
using Wammp.Model;

namespace Wammp.Services
{
    interface IConfigProvider
    {
        bool EnableProxy { get; set; }
        bool EnableCredentials { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        string User { get; set; }
        string Password { get; set; }
        string Domain { get; set; }
        string BassUser { get; set; }
        string BassCode { get; set; }
        string SelectedTheme { get; set; }
        List<SimplePlugin> Plugins { get; set; }
        List<string> Tracks { get; set; }
        
        void Save();
        void Load();
    }
}

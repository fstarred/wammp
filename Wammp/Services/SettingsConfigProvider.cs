using System.Collections.Generic;
using Wammp.Model;
using Wammp.Settings;

namespace Wammp.Services
{
    class SettingsConfigProvider : IConfigProvider
    {
        public SettingsConfigProvider()
        {
            this.Settings = AppSettingsLocator.Instance;
        }

        private AppSettings Settings { get; set; }
        
        public bool EnableProxy { get; set; }
        public bool EnableCredentials { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string BassUser { get; set; }
        public string BassCode { get; set; }
        public string SelectedTheme { get; set; }
        public List<SimplePlugin> Plugins { get; set; }
        public List<string> Tracks { get; set; }

        public void Save()
        {
            Settings.Domain = this.Domain;
            Settings.EnableCredentials = this.EnableCredentials;
            Settings.EnableProxy = this.EnableProxy;
            Settings.Host = this.Host;
            Settings.Password = this.Password;
            Settings.Port = this.Port;
            Settings.User = this.User;
            Settings.Plugins = this.Plugins;
            Settings.Tracks = this.Tracks;
            Settings.BassUser = this.BassUser;
            Settings.BassCode = this.BassCode;
            Settings.SelectedTheme = this.SelectedTheme;

            Settings.Save();
        }

        public void Load()
        {
            this.Domain = Settings.Domain;
            this.EnableCredentials = Settings.EnableCredentials;
            this.EnableProxy = Settings.EnableProxy;
            this.Host = Settings.Host;
            this.Password = Settings.Password;
            this.Port = Settings.Port;
            this.User = Settings.User;
            this.BassUser = Settings.BassUser;
            this.BassCode = Settings.BassCode;
            this.SelectedTheme = Settings.SelectedTheme;
            this.Plugins = Settings.Plugins ?? new List<SimplePlugin>();
            this.Tracks = Settings.Tracks ?? new List<string>();            
        }
    }
}

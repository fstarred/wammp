using System.Collections.Generic;
using System.Configuration;
using Wammp.Model;

namespace Wammp.Settings
{
    sealed class AppSettings : ApplicationSettingsBase
    {
        [UserScopedSettingAttribute()]
        [DefaultSettingValue("false")]
        public bool EnableProxy
        {
            get { return (bool)(this["EnableProxy"]); }
            set { this["EnableProxy"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("false")]
        public bool EnableCredentials
        {
            get { return (bool)(this["EnableCredentials"]); }
            set { this["EnableCredentials"] = value; }
        }

        [UserScopedSettingAttribute()]
        public string Host
        {
            get { return (string)(this["Host"]); }
            set { this["Host"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("8080")]
        public int Port
        {
            get { return (int)(this["Port"]); }
            set { this["Port"] = value; }
        }

        [UserScopedSettingAttribute()]
        public string User
        {
            get { return (string)(this["User"]); }
            set { this["User"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("")]
        public string Password
        {
            get { return (string)(this["Password"]); }
            set { this["Password"] = value; }
        }

        [UserScopedSettingAttribute()]
        public string Domain
        {
            get { return (string)(this["Domain"]); }
            set { this["Domain"] = value; }
        }

        [UserScopedSettingAttribute()]
        public string BassUser
        {
            get { return (string)(this["BassUser"]); }
            set { this["BassUser"] = value; }
        }

        [UserScopedSettingAttribute()]
        public string BassCode
        {
            get { return (string)(this["BassCode"]); }
            set { this["BassCode"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue(@"/Wammp;component/Themes/ExpressionDark.xaml")]
        public string SelectedTheme
        {
            get { return (string)(this["SelectedTheme"]); }
            set { this["SelectedTheme"] = value; }
        }

        [UserScopedSettingAttribute()]
        public List<SimplePlugin> Plugins
        {
            get { return (List<SimplePlugin>)(this["Plugins"]); }
            set { this["Plugins"] = value; }
        }

        [UserScopedSettingAttribute()]
        public List<string> Tracks
        {
            get { return (List<string>)(this["Tracks"]); }
            set { this["Tracks"] = value; }
        }
    }
}

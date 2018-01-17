using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wammp.Settings
{
    class AudioControllerSettings : ApplicationSettingsBase
    {
        [UserScopedSettingAttribute()]
        [DefaultSettingValue("true")]
        public bool UpgradeRequired
        {
            get { return (bool)(this["UpgradeRequired"]); }
            set { this["UpgradeRequired"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("1")]
        public int Device
        {
            get { return (int)(this["Device"]); }
            set { this["Device"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("1")]
        public float Volume
        {
            get { return (float)(this["Volume"]); }
            set { this["Volume"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValue("0")]
        public float Pan
        {
            get { return (float)(this["Pan"]); }
            set { this["Pan"] = value; }
        }

        [UserScopedSettingAttribute()]
        public StringCollection EqValues
        {
            get { return (StringCollection)(this["EqValues"]); }
            set { this["EqValues"] = value; }
        }
    }
}

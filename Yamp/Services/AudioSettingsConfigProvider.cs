﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yemp.Settings;

namespace Yemp.Services
{
    class AudioSettingsConfigProvider : IAudioConfigProvider
    {
        private AudioControllerSettings Settings { get; set; }

        public AudioSettingsConfigProvider()
        {
            Settings = new AudioControllerSettings();
        }

        public float Volume { get; set; }
        public float Pan { get; set; }
        public int[] EqValues { get; set; }
        public int Device { get; set; }

        public void Save()
        {
            Settings.Volume = this.Volume;
            Settings.Pan = this.Pan;
            Settings.Device = this.Device;

            StringCollection sc = new StringCollection();
            sc.AddRange(this.EqValues.Select(i => i.ToString()).ToArray());

            Settings.EqValues = sc;

            Settings.Save();
        }

        public void Load()
        {
            this.Volume = Settings.Volume;
            this.Pan = Settings.Pan;
            this.EqValues = Settings.EqValues != null ?
                Settings.EqValues.Cast<string>().Select(i => Int32.Parse(i)).ToArray() :
                new int[0];
            this.Device = Settings.Device;
        }
    }
}

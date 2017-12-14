using System;

namespace YempCommons.Model
{
    public class VersionCheckEventArgs : EventArgs
    {
        public Version Version { get; set; }

        public string ErrorMessage { get; set; }
    }
}

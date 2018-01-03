using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace WammpCommons.Utils
{
    public static class Utility
    {
        public static bool IsDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        public static Version GetVersionInfo(Assembly assembly)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            return new Version(fileVersionInfo.ProductVersion);
        }

        public static Version GetEntryAssemblyVersion()
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);

            return new Version(fileVersionInfo.ProductVersion);
        }

        public static string GetAppLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public static bool IsValidRegex(string pattern)
        {
            try
            {
                System.Text.RegularExpressions.Regex.Match(String.Empty, pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

    }
}

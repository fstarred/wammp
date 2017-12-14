using System;
using System.Windows;

namespace Yemp.Components
{
    public class ThemeSelector : DependencyObject
    {
        public static int WorkingDictionaryIndex { get; set; }

        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.RegisterAttached("Theme", typeof(string),
                typeof(ThemeSelector),
                new UIPropertyMetadata(null, ThemeChanged));

        public static string GetTheme(DependencyObject obj)
        {
            return (string)obj.GetValue(ThemeProperty);
        }

        public static void SetTheme(DependencyObject obj, string value)
        {
            obj.SetValue(ThemeProperty, value);
        }

        static void ThemeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is FrameworkElement) // works only on FrameworkElement objects
            {
                ApplyTheme(e.NewValue.ToString());
            }
        }

        public static void ApplyTheme(string theme)
        {
            ResourceDictionary dictos = App.Current.Resources.MergedDictionaries[WorkingDictionaryIndex];
            dictos.MergedDictionaries.Clear();
            dictos.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(theme, UriKind.Relative) });
        }
    }
}

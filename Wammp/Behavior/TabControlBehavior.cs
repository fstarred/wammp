using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Imaging;
using Wammp.ViewModel;
using WammpPluginContracts;

namespace Wammp.Behavior
{
    class TabControlBehavior : Behavior<TabControl>
    {
        public VIEW_TYPE ViewType { get; set; }

        public static readonly DependencyProperty PluginsProperty =
           DependencyProperty.RegisterAttached("Plugins", typeof(IEnumerable<PluginVM>), typeof(TabControlBehavior), new PropertyMetadata(PluginsChanged));

        private static void PluginsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TabControlBehavior behavior = obj as TabControlBehavior;

            TabControl tabctrl = behavior.AssociatedObject;

            foreach (PluginVM item in behavior.Plugins)
            {
                if (item.IsEnabled)
                {
                    if (item.Plugin.ViewType == behavior.ViewType)
                    {
                        byte[] icon = item.Plugin.Icon();
                        BitmapImage image = new BitmapImage();

                        if (icon != null && icon.Length > 0)
                        {
                            image = (BitmapImage)new Wammp.Converter.DataToImageConverter().Convert(icon, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
                        }

                        Image imageControl = new Image();

                        imageControl.Source = image;
                        imageControl.Width = 32;
                        imageControl.Height = 32;
                        if (image.CanFreeze)
                        {
                            image.Freeze();
                        }

                        TabItem t = new TabItem();
                        //t.Header = image ?? item.Plugin.Name;
                        t.Header = imageControl;
                        //t.Content = item.Plugin.View;
                        UserControlBase ucb = new WammpPluginContracts.UserControlBase();

                        ((ContentPresenter)ucb.FindName("ContentPresenter")).Content = item.Plugin.View;
                        t.Content = ucb;

                        tabctrl.Items.Add(t);
                    }
                }
            }
        }

        public IEnumerable<PluginVM> Plugins
        {
            get { return (IEnumerable<PluginVM>)GetValue(PluginsProperty); }
            set { SetValue(PluginsProperty, value); }
        }

        protected override void OnAttached()
        {
            //TabControl tabctrl = this.AssociatedObject;

            //foreach (PluginVM item in Plugins)
            //{
            //    if (item.IsEnabled)
            //    {
            //        if (item.Plugin.ViewType == this.ViewType)
            //        {
            //            byte[] icon = item.Plugin.Icon();
            //            BitmapImage image = new BitmapImage();

            //            if (icon != null && icon.Length > 0)
            //            {
            //                image = (BitmapImage)new Yemp.Converter.DataToImageConverter().Convert(icon, typeof(BitmapImage), null, CultureInfo.CurrentCulture);
            //            }

            //            Image imageControl = new Image();

            //            imageControl.Source = image;
            //            imageControl.Width = 32;
            //            imageControl.Height = 32;
            //            if (image.CanFreeze)
            //            {
            //                image.Freeze();
            //            }

            //            TabItem t = new TabItem();
            //            //t.Header = image ?? item.Plugin.Name;
            //            t.Header = imageControl;
            //            //t.Content = item.Plugin.View;
            //            UserControlBase ucb = new YempPluginContracts.UserControlBase();

            //            ((ContentPresenter)ucb.FindName("ContentPresenter")).Content = item.Plugin.View;
            //            t.Content = ucb;
                        
            //            tabctrl.Items.Add(t);
            //        }
            //    }                           
            //}
        }

        protected override void OnDetaching()
        {

        }
    }
}

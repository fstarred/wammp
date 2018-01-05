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
        enum TAB_TYPE { STANDARD, PLUGIN }

        public VIEW_TYPE ViewType { get; set; }

        public static readonly DependencyProperty PluginsProperty =
           DependencyProperty.RegisterAttached("Plugins", typeof(IEnumerable<PluginVM>), typeof(TabControlBehavior), new PropertyMetadata(PluginsChanged));

        private static void PluginsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TabControlBehavior behavior = obj as TabControlBehavior;

            TabControl tabctrl = behavior.AssociatedObject;

            int pluginsCount = System.Linq.Enumerable.ToArray(behavior.Plugins).Length;

            if (tabctrl != null && pluginsCount > 0 && !behavior.IsAlreadyInit)
            {
                if (!behavior.IsAlreadyInit)
                {
                    foreach (TabItem item in tabctrl.Items)
                    {
                        item.Tag = TAB_TYPE.STANDARD;
                    }
                }

                behavior.IsAlreadyInit = true;

                foreach (PluginVM item in behavior.Plugins)
                {
                    if (item.IsEnabled)
                    {
                        switch (behavior.ViewType)
                        {
                            case VIEW_TYPE.MAIN:

                                if (item.Plugin.View != null)
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
                                    t.Tag = TAB_TYPE.PLUGIN;
                                    //t.Header = image ?? item.Plugin.Name;
                                    t.Header = imageControl;
                                    //t.Content = item.Plugin.View;
                                    UserControlBase ucb = new WammpPluginContracts.UserControlBase();

                                    ((ContentPresenter)ucb.FindName("ContentPresenter")).Content = item.Plugin.View;
                                    if (item.Plugin.ToolBar != null)
                                    {
                                        ToolBar tb = item.Plugin.ToolBar as ToolBar;
                                        ((ToolBarTray)ucb.FindName("ToolBarTray")).ToolBars.Add(tb);
                                    }
                                    t.Content = ucb;

                                    tabctrl.Items.Add(t);
                                }

                                break;

                            case VIEW_TYPE.SETTINGS:

                                if (item.Plugin.SettingsView != null)
                                {
                                    TabItem t = new TabItem();
                                    t.Tag = TAB_TYPE.PLUGIN;
                                    t.Header = item.Plugin.Name;

                                    UserControl uc = item.Plugin.SettingsView as UserControl;

                                    Panel p = uc.Content as Panel;

                                    if (p != null)
                                        Helper.NotifierPanelHelper.SetEnableNotifier(p, true);

                                    t.Content = uc;

                                    //t.Content = item.Plugin.SettingsView;

                                    tabctrl.Items.Add(t);
                                }

                                break;
                        }                            
                    }
                }
            }
        }
        public IEnumerable<PluginVM> Plugins
        {
            get { return (IEnumerable<PluginVM>)GetValue(PluginsProperty); }
            set { SetValue(PluginsProperty, value); }
        }

        bool IsAlreadyInit { get; set; }
        protected override void OnAttached()
        {
            if (!IsAlreadyInit)
                PluginsChanged(this, new DependencyPropertyChangedEventArgs());
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

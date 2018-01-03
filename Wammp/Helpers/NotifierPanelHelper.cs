using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wammp.Helper
{
    public static class NotifierPanelHelper
    {
        public static readonly DependencyProperty EnableNotifierProperty =
            DependencyProperty.RegisterAttached(
                "EnableNotifier",
                typeof(bool),
                typeof(NotifierPanelHelper),
                new FrameworkPropertyMetadata(EnableNotifierChanged));

        /// <summary>
        /// Alarm Attached Routed Event
        /// </summary>
        public static readonly RoutedEvent ChildrenChangedEvent =
            EventManager.RegisterRoutedEvent("ChildrenChanged",
                                             RoutingStrategy.Bubble,
                                             typeof(RoutedEventHandler),
                                             typeof(NotifierPanelHelper));


        public static bool GetEnableNotifier(DependencyObject dp)
        {
            return (bool)dp.GetValue(EnableNotifierProperty);
        }

        public static void SetEnableNotifier(DependencyObject dp, bool value)
        {
            dp.SetValue(EnableNotifierProperty, value);
        }

        public static void AddChildrenChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.AddHandler(NotifierPanelHelper.ChildrenChangedEvent, handler);
            }
        }
        public static void RemoveChildrenChangedHandler(DependencyObject d, RoutedEventHandler handler)
        {
            UIElement uie = d as UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(NotifierPanelHelper.ChildrenChangedEvent, handler);
            }
        }

        private static readonly DependencyProperty IsInitializedProperty =
           DependencyProperty.RegisterAttached("IsInitialized", typeof(bool),
           typeof(NotifierPanelHelper));

        private static bool GetIsInitialized(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsInitializedProperty);
        }

        private static void SetIsInitialized(DependencyObject dp, bool value)
        {
            dp.SetValue(IsInitializedProperty, value);
        }

        private static void EnableNotifierChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Panel panel = obj as Panel;

                if (panel != null)
                {
                    panel.Loaded += (sender, re) =>
                    {
                        if (GetIsInitialized(panel) == false)
                        {
                            SetIsInitialized(panel, true);
                            RoutedEventArgs args = new RoutedEventArgs(ChildrenChangedEvent, panel);

                            HelperNotifier.AttachAllNotifierEvents(panel, args);                            
                        }
                    };
                }                
            }            
        }
        
        /// <summary>
        /// Helper class
        /// </summary>
        static class HelperNotifier
        {
            static IEnumerable<T> GetCollection<T>(Panel panel) { return panel.Children.OfType<T>(); }

            public static void AttachAllNotifierEvents(Panel panel, RoutedEventArgs args)
            {
                AttachTextBoxNotifier(panel, args);
                AttachComboBoxNotifier(panel, args);
                AttachCheckBoxNotifier(panel, args);
                AttachPasswordBoxNotifier(panel, args);
                AttachListBoxNotifier(panel, args);
                
                IEnumerable<Panel> panels = GetCollection<Panel>(panel);

                foreach (Panel childPanel in panels)
                {
                    AttachAllNotifierEvents(childPanel, args);
                }                
            }
            
            public static void AttachTextBoxNotifier(Panel panel, RoutedEventArgs eve)
            {
                GetCollection<TextBox>(panel).All(t =>
                    {
                        
                        t.TextChanged += (sender, e) =>
                        {
                            if (eve != null)
                            {
                                eve.Source = t;
                                panel.RaiseEvent(eve);
                            }
                        };
                        return true;
                    });
            }

            public static void AttachListBoxNotifier(Panel panel, RoutedEventArgs eve)
            {
                GetCollection<ListBox>(panel).All(t =>
                {
                    t.SelectionChanged += (sender, e) =>
                    {
                        if (eve != null)
                        {
                            eve.Source = t;
                            panel.RaiseEvent(eve);
                        }
                    };
                    return true;
                });
            }

            public static void AttachPasswordBoxNotifier(Panel panel, RoutedEventArgs eve)
            {
                GetCollection<PasswordBox>(panel).All(t =>
                {
                    t.PasswordChanged += (sender, e) =>
                    {
                        if (eve != null)
                        {
                            eve.Source = t;
                            panel.RaiseEvent(eve);
                        }
                    };
                    return true;
                });
            }

            public static void AttachCheckBoxNotifier(Panel panel, RoutedEventArgs eve)
            {
                GetCollection<CheckBox>(panel).All(t =>
                {
                    RoutedEventHandler rout = (sender, e) =>
                    {
                        if (eve != null)
                        {
                            eve.Source = t;
                            t.RaiseEvent(eve);
                        }
                    };

                    t.Checked += rout;
                    t.Unchecked += rout;
                    return true;
                });
            }

            public static void AttachComboBoxNotifier(Panel panel, RoutedEventArgs eve)
            {
                GetCollection<ComboBox>(panel).All(t =>
                {
                    t.SelectionChanged += (sender, e) =>
                    {
                        if (eve != null)
                        {
                            eve.Source = t;
                            panel.RaiseEvent(eve);
                        }
                    };
                    return true;
                });
            }
        }

    }
}

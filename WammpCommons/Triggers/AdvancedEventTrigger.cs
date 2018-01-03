using System;
using System.Windows;

namespace WammpCommons.Triggers
{
    public class AdvancedEventTrigger : System.Windows.Interactivity.EventTrigger
    {
        public static readonly DependencyProperty EventArgsProperty =
            DependencyProperty.Register("EventArgs",
            typeof(object), typeof(AdvancedEventTrigger),
            new FrameworkPropertyMetadata(null));

        public static object GetEventArgs(DependencyObject dp)
        {
            return (object)dp.GetValue(EventArgsProperty);
        }

        public static void SetEventArgs(DependencyObject dp, object value)
        {
            dp.SetValue(EventArgsProperty, value);
        }

        public AdvancedEventTrigger() : base()
        {

        }

        public AdvancedEventTrigger(string eventName) : base(eventName)
        {

        }

        protected override void OnEvent(EventArgs eventArgs)
        {
            SetValue(EventArgsProperty, eventArgs);
            base.InvokeActions(eventArgs);
        }
    }
}

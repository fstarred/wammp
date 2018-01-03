using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Wammp.Helper
{
    public class RoutedEventTrigger : EventTriggerBase<DependencyObject>
    {
        RoutedEvent _routedEvent;
        public RoutedEvent RoutedEvent
        {
            get
            {
                return _routedEvent;
            }
            set { _routedEvent = value; }
        }

        public RoutedEventTrigger() { }
        protected override void OnAttached()
        {
            System.Windows.Interactivity.Behavior behavior = base.AssociatedObject as System.Windows.Interactivity.Behavior;
            FrameworkElement associatedElement = base.AssociatedObject as FrameworkElement; if (behavior != null)
            {
                associatedElement = ((IAttachedObject)behavior).AssociatedObject as FrameworkElement;
            } if (associatedElement == null)
            {
                throw new ArgumentException("Routed Event trigger can only be associated to framework elements");
            }
            if (RoutedEvent != null)
            {
                associatedElement.AddHandler(RoutedEvent, new RoutedEventHandler(this.OnRoutedEvent));
            }
        }

        void OnRoutedEvent(object sender, RoutedEventArgs args)
        {
            base.OnEvent(args);
        }

        protected override string GetEventName()
        {
            return RoutedEvent.Name;
        }
    }
}

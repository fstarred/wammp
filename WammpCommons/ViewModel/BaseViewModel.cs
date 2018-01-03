using System;
using System.Linq.Expressions;
using WammpCommons.Services;

namespace WammpCommons.ViewModel
{
    public class BaseViewModel : ObservableObject
    {
        public static IDispatcherProvider RootDispatcher { get; set; }
        
        protected void ExecuteSafeAction(Action action)
        {
            if (RootDispatcher.Dispatcher.CheckAccess())
            {
                // do work on UI thread                
                action();
            }
            else
            {
                // or Invoke()
                RootDispatcher.Dispatcher.Invoke((Action)delegate ()
                {
                    action();
                });
            }
        }

        protected void RaiseObjectChangedSafeInvoker<T>(Expression<Func<T>> propertyExpresssion)
        {
            if (RootDispatcher.Dispatcher.CheckAccess())
            {
                // do work on UI thread                
                RaisePropertyChanged(propertyExpresssion);
            }
            else
            {
                // or Invoke()
                RootDispatcher.Dispatcher.Invoke((Action)delegate()
                {
                    RaisePropertyChanged(propertyExpresssion);
                });
            }
        }

        protected void TriggerSafeEvent(EventHandler eve)
        {
            TriggerSafeEvent(eve, EventArgs.Empty);
        }

        protected void TriggerSafeEvent(EventHandler eve, EventArgs args)
        {
            if (RootDispatcher.Dispatcher.CheckAccess())
            {
                if (eve != null)
                    eve.Invoke(this, args);
            }
            else
            {
                RootDispatcher.Dispatcher.Invoke((Action)delegate()
                {
                    if (eve != null)
                        eve.Invoke(this, args);
                });
            }
        }

        protected void RaiseEventBeginInvoker(EventHandler eve)
        {
            if (RootDispatcher.Dispatcher.CheckAccess())
            {
                if (eve != null)
                    eve.BeginInvoke(this, EventArgs.Empty, null, null);
            }
            else
            {
                RootDispatcher.Dispatcher.BeginInvoke((Action)delegate()
                {
                    if (eve != null)
                        eve.Invoke(this, EventArgs.Empty);
                });
            }
        }
    }
}

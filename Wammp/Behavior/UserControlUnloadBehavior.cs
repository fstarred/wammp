using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Wammp.Behavior
{
    class UserControlUnloadBehavior : Behavior<UserControl>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(UserControlUnloadBehavior));


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += (sender, e) =>
            {
                var window = Window.GetWindow(AssociatedObject);
                if (window != null)
                {
                    window.Closing -= Window_Closing; // OnDetaching is not called
                    window.Closing += Window_Closing;
                }
                    
            };            
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Command != null)
            {
                Command.Execute(null);
            }
        }
        /*
         * This is not called unlucky
         * */
        protected override void OnDetaching()
        {
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
                window.Closing -= Window_Closing;
        }
    }
}

using System.Security;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Wammp.Services;

namespace Wammp.Behavior
{
    class PasswordBoxConfigBehavior : Behavior<PasswordBox>
    {
        /// <summary>
        /// Property of the Password viewmodel to be associated
        /// </summary>
        public string AssociatedProperty { get; set; }

        /// <summary>
        /// Get the password's value from viewmodel
        /// </summary>
        private void ResetPassword()
        {
            PasswordBox passwordBox = this.AssociatedObject;

            if (passwordBox.DataContext != null)
            {
                SecureString s = (SecureString)passwordBox.DataContext.GetType().GetProperty(AssociatedProperty).GetValue(passwordBox.DataContext, null);

                passwordBox.Password = Utils.SecurityUtils.ToInsecureString(s);
            }
        }

        protected override void OnAttached()
        {
            PasswordBox passwordBox = this.AssociatedObject;

            if (passwordBox.DataContext != null)
            {
                ResetPassword();

                // get the changed passwordbox value and set into the viewmodel associated property
                passwordBox.PasswordChanged += (sender, e) =>
                {
                    System.Type dataContextType = passwordBox.DataContext.GetType();

                    dataContextType.GetProperty(AssociatedProperty).SetValue(passwordBox.DataContext, ((PasswordBox)sender).SecurePassword, null);

                    // if the viewmodel implements IPasswordHandler and raise the PasswordResetEvent, the passwordbox set
                    // the value taken from the viewmodel associatedproperty
                    if (typeof(IPasswordHandler).IsAssignableFrom(dataContextType))
                    {
                        ((IPasswordHandler)passwordBox.DataContext).PasswordResetEvent += PasswordBoxConfigBehavior_PasswordReset;
                    }
                };
            }
        }

        private void PasswordBoxConfigBehavior_PasswordReset(object sender, System.EventArgs e)
        {
            ResetPassword();
        }

        protected override void OnDetaching()
        {
            PasswordBox passwordBox = this.AssociatedObject;

            // remove the event handler
            if (passwordBox.DataContext != null)
            {
                System.Type dataContextType = passwordBox.DataContext.GetType();

                if (typeof(IPasswordHandler).IsAssignableFrom(dataContextType))
                {
                    ((IPasswordHandler)passwordBox.DataContext).PasswordResetEvent -= PasswordBoxConfigBehavior_PasswordReset;
                }
            }
        }
    }
}

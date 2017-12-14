using System;
using System.Windows.Input;
using Yemp.Services;
using YempCommons.ViewModel;

namespace Yemp.ViewModel
{
    class OpenUrlVM : BaseViewModel
    {
        public OpenUrlVM()
        {
            Address = "http://";
        }

        private string address;

        public string Address
        {
            get { return address; }
            set {
                address = value;
                RaisePropertyChanged(() => Address);
            }
        }
        
        public event EventHandler ResponseSent;

        void Ok()
        {            
            TriggerSafeEvent(ResponseSent);
        }

        void Cancel()
        {
            Address = null;
            TriggerSafeEvent(ResponseSent);
        }

        public ICommand OkCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Ok); } }
        public ICommand CancelCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Cancel); } }
    }
}

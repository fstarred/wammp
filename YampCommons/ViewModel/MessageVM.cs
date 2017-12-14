using System;
using System.Windows.Input;
using YempCommons.Services;

namespace YempCommons.ViewModel
{
    class MessageVM : BaseViewModel
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public bool IsConfirm { get; set; }

        public MSG_RESPONSE Response { get; set; }

        public event EventHandler ResponseSent;

        void Ok()
        {
            Response = MSG_RESPONSE.OK;
            TriggerSafeEvent(ResponseSent);
        }

        void Cancel()
        {
            Response = MSG_RESPONSE.CANCEL;
            TriggerSafeEvent(ResponseSent);
        }

        public ICommand OkCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Ok); } }
        public ICommand CancelCommand { get { return new MvvmFoundation.Wpf.RelayCommand(Cancel); } }
    }
}

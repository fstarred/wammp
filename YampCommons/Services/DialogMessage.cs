//using WinLogAnalyzer.View;

using YempCommons.View;
using YempCommons.ViewModel;

namespace YempCommons.Services
{ 

    public class DialogMessage : IDialogMessage
    {

        public MSG_RESPONSE ShowMessage(string message, string title, bool isconfirm)
        {
            MessageVM vm = new MessageVM();
            vm.IsConfirm = isconfirm;
            vm.Message = message;
            vm.Title = title;

            ViewMessage view = new ViewMessage
            {
                DataContext = vm
            };

            view.ShowDialog();

            return vm.Response;
        }

        public MSG_RESPONSE ShowMessage(string message, string title)
        {
            return ShowMessage(message, title, false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WammpCommons.Services
{
    public enum MSG_RESPONSE { OK, CANCEL };

    public interface IDialogMessage
    {
        MSG_RESPONSE ShowMessage(string message, string title, bool isconfirm);

        MSG_RESPONSE ShowMessage(string message, string title);
    }
}

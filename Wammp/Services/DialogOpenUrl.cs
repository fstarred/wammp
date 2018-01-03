using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wammp.View;
using Wammp.ViewModel;

namespace Wammp.Services
{
    class DialogOpenUrl : IOpenUrl
    {
        public string OpenUrl()
        {
            OpenUrlVM vm = new OpenUrlVM();

            WinRemoteUrl view = new WinRemoteUrl
            {
                DataContext = vm
            };

            view.ShowDialog();

            return vm.Address;
        }
    }
}

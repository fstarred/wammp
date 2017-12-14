using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yemp.View;
using Yemp.ViewModel;

namespace Yemp.Services
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

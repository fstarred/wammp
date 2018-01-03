using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wammp.ViewModel;

namespace Wammp.ServiceLocator
{
    class ViewModelLocator
    {
        private ViewModelLocator()
        {
            this.current = TinyIoC.TinyIoCContainer.Current;
            this.current.Register<SettingsVM>().AsSingleton(); // singleton
        }

        static ViewModelLocator instance;
        TinyIoC.TinyIoCContainer current;

        public static ViewModelLocator Instance
        {
            get
            {
                return instance ?? (instance = new ViewModelLocator());
            }
        }

        public SettingsVM SettingsVM
        {
            get
            {
                return current.Resolve<SettingsVM>();
            }
        }
    }
}

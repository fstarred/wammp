using YempCommons.ViewModel;
using YempPluginContracts;

namespace Yemp.ViewModel
{
    public class PluginVM : ObservableObject
    {
        public PluginVM(IPlugin plugin)
        {
            this.Plugin = plugin;
            this.IsEnabled = true;
            this.Position = 999;
        }

        public IPlugin Plugin { get; private set; }

        private int position;

        public int Position
        {
            get { return position; }
            set {
                position = value;
                RaisePropertyChanged(() => Position);
            }
        }


        private bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set {
                isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

    }
}

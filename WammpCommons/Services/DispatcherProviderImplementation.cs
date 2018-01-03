using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WammpCommons.Services
{
    public class DispatcherProviderImplementation : IDispatcherProvider
    {
        public DispatcherProviderImplementation(Func<Dispatcher> getDispatcher)
        {
            _getDispatcher = getDispatcher;
        }

        private Func<Dispatcher> _getDispatcher;

        public virtual Dispatcher Dispatcher => _getDispatcher?.Invoke();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace YampCommons.Services
{
    public interface IDispatcherProvider
    {
        Dispatcher Dispatcher { get; }
    }
}

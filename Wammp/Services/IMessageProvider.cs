using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wammp.Services
{
    interface IMessageProvider
    {
        string TrackInfoUpdated { get; }
    }
}

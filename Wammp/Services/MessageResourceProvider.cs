using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wammp.Services
{
    class MessageResourceProvider : IMessageProvider
    {
        public string TrackInfoUpdated
        {
            get
            {
                return Wammp.Properties.Resources.TracksInfoUpdated;
            }
        }
    }
}

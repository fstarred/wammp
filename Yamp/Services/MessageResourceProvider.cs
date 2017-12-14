using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yemp.Services
{
    class MessageResourceProvider : IMessageProvider
    {
        public string TrackInfoUpdated
        {
            get
            {
                return Yamp.Properties.Resources.TracksInfoUpdated;
            }
        }
    }
}

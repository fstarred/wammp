using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wammp.Services
{
    interface IAudioConfigProvider
    {
        float Volume { get; set; }
        float Pan { get; set; }        
        int[] EqValues { get; set; }
        int Device { get; set; }

        void Save();
        void Load();
    }
}

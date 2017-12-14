using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yemp.Converter
{
    public enum CONTROL_TYPE { VOLUME, PAN }

    class ControlValueInfo
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public CONTROL_TYPE ControlType { get; set; }
    }
}

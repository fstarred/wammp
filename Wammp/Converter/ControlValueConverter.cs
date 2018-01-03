using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wammp.Converter
{
    class ControlValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string output = null;

            ControlValueInfo info = (ControlValueInfo)parameter;

            switch (info.ControlType)
            {
                case CONTROL_TYPE.VOLUME:
                    float volume = (float)value;
                    output = String.Format("{0}%", Math.Floor(volume * 100 / info.Max));
                    break;
                case CONTROL_TYPE.PAN:
                    float pan = (float)value;
                    output = Math.Floor(pan * 100).ToString();
                    break;
            }

            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

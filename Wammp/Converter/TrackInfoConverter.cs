using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wammp.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    class TrackInfoConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string ret = (string)value;

            if (ret != null)
            {
                ret = ret.Replace(Environment.NewLine, " ").Replace("\r\r", " ");
                ret = ret.Length > 50 ? String.Format("{0}..", ret.Substring(0, 50)) : ret;
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wammp.Converter
{
    [ValueConversion(typeof(long), typeof(double))]
    class SliderConverter : DependencyObject, IValueConverter
    {

        public long TrackLength
        {
            get { return (long)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }

        public static readonly DependencyProperty LengthProperty =
            DependencyProperty.Register("TrackLength", typeof(long), typeof(SliderConverter));
        
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            double ret = 0;

            if (value != DependencyProperty.UnsetValue && TrackLength > 0)
            {
                long pos = (long)value;

                ret = pos * (int)parameter / TrackLength;
            }

            return ret;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double ret = 0;

            if (value != DependencyProperty.UnsetValue && TrackLength > 0)
            {
                ret = (double)value * TrackLength / (int)parameter;
            }

            return ret;
        }
    }
}

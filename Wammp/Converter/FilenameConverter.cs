using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wammp.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    class FilenameConverter : IValueConverter
    {
        enum PARAM_TYPE { FILE, DIRECTORY }

        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            string file = null;

            if (value != null)
                file = value.ToString();

            if (String.IsNullOrEmpty(file) == false)
            {
                PARAM_TYPE param = PARAM_TYPE.FILE;

                if (parameter != null && parameter.Equals("directory"))
                    param = PARAM_TYPE.DIRECTORY;

                switch (param)
                {
                    case PARAM_TYPE.FILE:
                        file = Path.GetFileName(file);
                        break;
                    case PARAM_TYPE.DIRECTORY:
                        file = Path.GetDirectoryName(file);
                        break;
                }                
            }

            return file;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

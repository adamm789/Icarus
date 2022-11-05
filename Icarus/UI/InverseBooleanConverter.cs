using System;
using System.Windows.Data;

namespace Icarus.UI
{
    // https://stackoverflow.com/a/1039681
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //if (targetType != typeof(bool))
            //throw new InvalidOperationException("The target must be a boolean");

            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        #endregion
    }
}

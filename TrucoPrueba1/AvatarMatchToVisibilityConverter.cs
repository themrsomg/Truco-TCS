using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrucoPrueba1
{
    public class AvatarMatchToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return Visibility.Collapsed;

            var avatarId = values[0] as string;
            var currentId = values[1] as string;

            return avatarId == currentId ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

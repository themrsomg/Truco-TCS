using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;

namespace TrucoPrueba1
{
    public class AvatarIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string avatarId = value as string;
            if (string.IsNullOrWhiteSpace(avatarId))
            {
                avatarId = "avatar_default";
            }

            string imagePath = $"pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/{avatarId}.png";

            try
            {
                var image = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                return image;
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/TrucoPrueba1;component/Resources/Avatars/avatar_default.png", UriKind.Absolute));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

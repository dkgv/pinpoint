using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using Pinpoint.Win.Extensions;

namespace Pinpoint.Win.Converters
{
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Bitmap) value).ToImageSource();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

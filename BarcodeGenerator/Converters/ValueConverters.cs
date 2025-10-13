using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BarcodeGenerator.Converters
{
    /// <summary>
    /// Converts boolean to brush for validation status indication
    /// </summary>
    public class BooleanToBrushConverter : IValueConverter
    {
        public static readonly BooleanToBrushConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid 
                    ? new SolidColorBrush(Color.FromRgb(212, 237, 218)) // Light green for valid
                    : new SolidColorBrush(Color.FromRgb(248, 215, 218)); // Light red for invalid
            }
            
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to text brush color for validation messages
    /// </summary>
    public class BooleanToTextBrushConverter : IValueConverter
    {
        public static readonly BooleanToTextBrushConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid 
                    ? new SolidColorBrush(Color.FromRgb(21, 87, 36)) // Dark green for valid
                    : new SolidColorBrush(Color.FromRgb(132, 32, 41)); // Dark red for invalid
            }
            
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to status text
    /// </summary>
    public class BooleanToStatusConverter : IValueConverter
    {
        public static readonly BooleanToStatusConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid ? "Ready" : "Invalid Data";
            }
            
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverts boolean to visibility converter
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public static readonly InverseBooleanToVisibilityConverter Default = new();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            
            return false;
        }
    }
}
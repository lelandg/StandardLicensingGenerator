using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StandardLicensingGenerator;

public class BoolToVisibilityConverter : IValueConverter
{
    // Converts bool to Visibility
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b)
        {
            return Visibility.Visible; // true -> Visible
        }
        return Visibility.Collapsed; // false -> Collapsed
    }

    // Converts back Visibility to bool (optional, used in two-way bindings)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v)
        {
            return v == Visibility.Visible;
        }
        return false;
    }
}
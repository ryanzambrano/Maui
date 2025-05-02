using System.Globalization;
using Microsoft.Maui.Controls;

namespace Amazon.Converters
{
    public class IsNotNullConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    public class TaxRateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal taxRate)
            {
                // Convert tax rate (0.07 for 7%) to slider value (7)
                return (double)(taxRate * 100);
            }
            return 0.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double sliderValue)
            {
                // Convert slider value (7) to tax rate (0.07 for 7%)
                return (decimal)(sliderValue / 100);
            }
            return 0.07m; // Default to 7%
        }
    }

    public class CartTabColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string cartName)
            {
                // The main cart has a special color
                return cartName == "Shopping Cart" ? "#0056B3" : "#6c757d";
            }
            return "#6c757d"; // Default to gray
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotMainCartConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string cartName)
            {
                // Return true if this is NOT the main cart (so delete button is visible)
                return cartName != "Shopping Cart";
            }
            return true; // Default to visible
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
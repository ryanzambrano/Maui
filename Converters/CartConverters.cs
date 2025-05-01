using System.Globalization;
using Amazon.Models;
using System.Collections.ObjectModel;

namespace Amazon.Converters
{
    public class CartSubtotalConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<CartItem> cart)
            {
                return cart.Sum(item => item.Subtotal);
            }
            return 0m;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CartTaxConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<CartItem> cart)
            {
                return cart.Sum(item => item.Subtotal) * 0.07m;
            }
            return 0m;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CartTotalConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<CartItem> cart)
            {
                var subtotal = cart.Sum(item => item.Subtotal);
                return subtotal + (subtotal * 0.07m);
            }
            return 0m;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
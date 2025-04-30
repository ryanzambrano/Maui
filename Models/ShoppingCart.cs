using System.Collections.ObjectModel;
using System.Linq;

namespace Amazon.Models
{
    public class ShoppingCart
    {
        public ObservableCollection<CartItem> Items { get; } = new();
        public decimal Subtotal => Items.Sum(item => item.Subtotal);
        public decimal Tax => Subtotal * 0.07m;
        public decimal Total => Subtotal + Tax;
    }
} 
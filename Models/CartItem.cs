using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amazon.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private Product _product;
        private int _quantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        public required Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => Product.Price * Quantity;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
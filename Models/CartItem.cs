using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Amazon.Models;

public sealed class CartItem : INotifyPropertyChanged
{
    public required Product Product { get; init; }      // C# 11 'required' keeps CS8618 quiet

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value == _quantity) return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Subtotal));
        }
    }

    public decimal Subtotal => Product.Price * Quantity;

    private PropertyChangedEventHandler? _propertyChanged;
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add { _propertyChanged += value; }
        remove { _propertyChanged -= value; }
    }

    private void OnPropertyChanged([CallerMemberName] string? n = null)
    {
        Debug.WriteLine($"[CartItem ID:{Product?.Id}] OnPropertyChanged called for: {n}");
        _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

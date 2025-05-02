using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Amazon.Models
{
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private decimal _price;
        private string _category = string.Empty;
        private string _imageUrl = string.Empty;
        private int _stockQuantity;
        private DateTime _createdAt;

        public int Id 
        { 
            get => _id; 
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public required string Name 
        { 
            get => _name; 
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public required string Description 
        { 
            get => _description; 
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public decimal Price 
        { 
            get => _price; 
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public required string Category 
        { 
            get => _category; 
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public required string ImageUrl 
        { 
            get => _imageUrl; 
            set
            {
                if (_imageUrl != value)
                {
                    _imageUrl = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int StockQuantity 
        { 
            get => _stockQuantity; 
            set
            {
                if (_stockQuantity != value)
                {
                    _stockQuantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }
        
        public DateTime CreatedAt 
        { 
            get => _createdAt; 
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsAvailable => StockQuantity > 0;

        // Optional: Method to apply discount
        public decimal GetDiscountedPrice(decimal discountPercentage)
        {
            return Price * (1 - (discountPercentage / 100));
        }
        
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
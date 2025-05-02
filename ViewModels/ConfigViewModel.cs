using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Amazon.ViewModels
{
    public class ConfigViewModel : INotifyPropertyChanged
    {
        private decimal _taxRate = 0.07m; // Default 7%
        
        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                if (_taxRate != value)
                {
                    _taxRate = value;
                    OnPropertyChanged();
                    
                    // Save the tax rate when it changes
                    SaveTaxRate();
                    
                    // Update display percentage
                    TaxRatePercentage = $"{_taxRate * 100:F1}%";
                }
            }
        }
        
        private string _taxRatePercentage = "7.0%";
        public string TaxRatePercentage
        {
            get => _taxRatePercentage;
            set
            {
                if (_taxRatePercentage != value)
                {
                    _taxRatePercentage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ICommand SaveCommand { get; }
        public ICommand GoBackCommand { get; }
        
        public ConfigViewModel()
        {
            // Load saved tax rate
            LoadTaxRate();
            
            SaveCommand = new Command(Save);
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }
        
        private void Save()
        {
            SaveTaxRate();
            Shell.Current.DisplayAlert("Success", "Tax rate saved successfully", "OK");
        }
        
        private void SaveTaxRate()
        {
            Preferences.Set("TaxRate", (double)_taxRate);
        }
        
        private void LoadTaxRate()
        {
            if (Preferences.ContainsKey("TaxRate"))
            {
                _taxRate = (decimal)Preferences.Get("TaxRate", 0.07);
                TaxRatePercentage = $"{_taxRate * 100:F1}%";
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
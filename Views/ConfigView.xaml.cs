using Amazon.ViewModels;

namespace Amazon.Views
{
    public partial class ConfigView : ContentPage
    {
        public ConfigView(ConfigViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 
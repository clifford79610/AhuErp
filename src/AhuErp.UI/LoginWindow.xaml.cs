using System.Windows;
using AhuErp.UI.ViewModels;

namespace AhuErp.UI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.IsAuthenticated) &&
                    viewModel.IsAuthenticated)
                {
                    DialogResult = true;
                    Close();
                }
            };
            Loaded += (_, __) => FullNameBox.Focus();
        }
    }
}

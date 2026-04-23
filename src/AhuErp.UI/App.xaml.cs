using System.Windows;
using AhuErp.UI.Infrastructure;
using AhuErp.UI.ViewModels;

namespace AhuErp.UI
{
    /// <summary>
    /// Корневой App. Инициализирует DI-контейнер, показывает окно входа и,
    /// в случае успешной аутентификации, открывает <see cref="MainWindow"/>
    /// с уже разрешённой <see cref="MainViewModel"/> (зависимости приходят через DI).
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppServices.Initialize();

            var loginVm = AppServices.GetRequiredService<LoginViewModel>();
            var login = new LoginWindow(loginVm);

            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var mainVm = AppServices.GetRequiredService<MainViewModel>();
            var main = new MainWindow { DataContext = mainVm };
            MainWindow = main;
            main.Show();
        }
    }
}

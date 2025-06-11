using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace demo_14_4
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var loginWindow = new LoginWindow();

                loginWindow.LoginSucceeded += (_, isAdmin) =>
                {
                    var mainWindow = new MainWindow(isAdmin);
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();

                    loginWindow.Close();
                };

                loginWindow.LoginCancelled += (_, _) =>
                {
                    desktop.Shutdown();
                };

                desktop.MainWindow = loginWindow;
                loginWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }


    }
}

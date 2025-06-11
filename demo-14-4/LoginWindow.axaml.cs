using Avalonia.Controls;
using Avalonia.Interactivity;
using demoModels;
using System;
using System.Linq;

namespace demo_14_4
{
    public partial class LoginWindow : Window
    {
        public event EventHandler<bool> LoginSucceeded;
        public event EventHandler LoginCancelled;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            using var context = new MydatabaseContext();
            var user = context.Users.FirstOrDefault(u => u.Login == UsernameTextBox.Text && u.Password == PasswordTextBox.Text);

            if (user != null)
            {
                LoginSucceeded?.Invoke(this, user.Admin);
            }
            else
            {
                var errorText = this.FindControl<TextBlock>("ErrorText");
                errorText.Text = "Неверный логин или пароль";
                errorText.IsVisible = true;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog(this);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LoginCancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}

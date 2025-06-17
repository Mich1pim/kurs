using Avalonia.Controls;
using Avalonia.Interactivity;
using demoModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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

        private bool ValidateCredentials(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Ћогин и пароль не могут быть пустыми";
                return false;
            }

            if (!Regex.IsMatch(login, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ||
                (!login.EndsWith(".com", StringComparison.OrdinalIgnoreCase) &&
                 !login.EndsWith(".ru", StringComparison.OrdinalIgnoreCase)))
            {
                ErrorText.Text = "Ћогин должен быть email (содержать @ и заканчиватьс€ на .com или .ru)";
                return false;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[A-Z]).+$"))
            {
                ErrorText.Text = "ѕароль должен содержать хот€ бы одну заглавную букву";
                return false;
            }

            return true;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = UsernameTextBox.Text;
            var password = PasswordTextBox.Text;

            if (!ValidateCredentials(login, password))
            {
                ErrorText.IsVisible = true;
                return;
            }

            using var context = new MydatabaseContext();
            var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                LoginSucceeded?.Invoke(this, user.Admin);
                this.Close();
            }
            else
            {
                ErrorText.Text = "Ќеверный логин или пароль";
                ErrorText.IsVisible = true;
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
            this.Close();
        }
    }
}
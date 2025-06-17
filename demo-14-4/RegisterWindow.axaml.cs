using Avalonia.Controls;
using Avalonia.Interactivity;
using demoModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace demo_14_4
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private bool ValidateCredentials(string login, string password)
        {
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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var login = UsernameTextBox.Text.Trim();
            var password = PasswordTextBox.Text;
            var isAdmin = IsAdminCheckBox.IsChecked == true;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "«аполните все пол€";
                ErrorText.IsVisible = true;
                return;
            }

            if (!ValidateCredentials(login, password))
            {
                ErrorText.IsVisible = true;
                return;
            }

            using var context = new MydatabaseContext();

            if (context.Users.Any(u => u.Login == login))
            {
                ErrorText.Text = "ѕользователь с таким логином уже существует";
                ErrorText.IsVisible = true;
                return;
            }

            var user = new User
            {
                Login = login,
                Password = password,
                Admin = isAdmin
            };

            context.Users.Add(user);
            context.SaveChanges();

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
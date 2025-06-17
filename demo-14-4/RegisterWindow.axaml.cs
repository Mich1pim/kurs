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
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Все поля должны быть заполнены";
                return false;
            }

            if (!Regex.IsMatch(login, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ||
                (!login.EndsWith(".com", StringComparison.OrdinalIgnoreCase) &&
                 !login.EndsWith(".ru", StringComparison.OrdinalIgnoreCase)))
            {
                ErrorText.Text = "Логин должен быть email (содержать @ и заканчиваться на .com или .ru)";
                return false;
            }

            if (password.Length < 6)
            {
                ErrorText.Text = "Пароль должен содержать минимум 6 символов";
                return false;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[A-Z]).+$"))
            {
                ErrorText.Text = "Пароль должен содержать хотя бы одну заглавную букву";
                return false;
            }

            return true;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var login = UsernameTextBox.Text?.Trim() ?? string.Empty;
            var password = PasswordTextBox.Text ?? string.Empty;
            var isAdmin = IsAdminCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Все поля должны быть заполнены";
                ErrorText.IsVisible = true;
                return;
            }

            if (!ValidateCredentials(login, password))
            {
                ErrorText.IsVisible = true;
                return;
            }

            try
            {
                using var context = new MydatabaseContext();

                if (context.Users.Any(u => u.Login == login))
                {
                    ErrorText.Text = "Пользователь с таким логином уже существует";
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
            catch (Exception ex)
            {
                ErrorText.Text = $"Ошибка регистрации: {ex.Message}";
                ErrorText.IsVisible = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using demoModels;
using System.Linq;

namespace demo_14_4
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var login = UsernameTextBox.Text.Trim();
            var password = PasswordTextBox.Text;
            var isAdmin = IsAdminCheckBox.IsChecked == true;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Заполните все поля";
                ErrorText.IsVisible = true; 
                return;
            }

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


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

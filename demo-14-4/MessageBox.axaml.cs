using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;

namespace demo_14_4
{
    public enum MessageBoxButtons
    {
        OK,
        YesNo
    }

    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
        }

        public MessageBox(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            InitializeComponent();
            this.Title = title;
            var messageBlock = this.FindControl<TextBlock>("MessageText");
            messageBlock.Text = message;

            var buttonPanel = this.FindControl<StackPanel>("ButtonPanel");

            if (buttons == MessageBoxButtons.OK)
            {
                var okButton = new Button { Content = "OK" };
                okButton.Click += (s, e) => Close(true);
                buttonPanel.Children.Add(okButton);
            }
            else if (buttons == MessageBoxButtons.YesNo)
            {
                var yesButton = new Button { Content = "Да" };
                yesButton.Click += (s, e) => Close(true);
                buttonPanel.Children.Add(yesButton);

                var noButton = new Button { Content = "Нет" };
                noButton.Click += (s, e) => Close(false);
                buttonPanel.Children.Add(noButton);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static async Task<bool> Show(Window parent, string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            var dialog = new MessageBox(title, message, buttons);
            return await dialog.ShowDialog<bool>(parent);
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace demo_14_4
{
    public partial class ChangeCostDialog : Window
    {
        public double NewCost { get; private set; }

        public ChangeCostDialog(double averageCost)
        {
            InitializeComponent();
            this.DataContext = this;

            var costBox = this.FindControl<TextBox>("CostBox");
            costBox.Text = averageCost.ToString("0.00");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var costBox = this.FindControl<TextBox>("CostBox");
            if (double.TryParse(costBox.Text, out double newCost))
            {
                NewCost = newCost;
                Close(true);
            }
            else
            {
                var errorText = this.FindControl<TextBlock>("ErrorText");
                errorText.IsVisible = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
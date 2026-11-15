using System.Windows;

namespace CatchTheSquare
{
    public partial class RulesWindow : Window
    {
        public RulesWindow() => InitializeComponent();

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
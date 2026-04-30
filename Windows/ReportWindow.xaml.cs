using System.Windows;

namespace Resonate.Windows
{
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
        }

        private void GenerateReport(object sender, RoutedEventArgs e)
        {
            new InfoWindow("Формирование отчётов пока не реализовано.").Show();
        }

        private void CancelReport(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

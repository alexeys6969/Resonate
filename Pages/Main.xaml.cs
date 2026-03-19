using Newtonsoft.Json.Linq;
using Resonate.Context;
using Resonate.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Resonate.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private static string _token;
        public Main(string token)
        {
            InitializeComponent();
            _token = token;
            LoadUserData();
            LoadPermissions();
        }

        private void ReportForm(object sender, RoutedEventArgs e)
        {
            
        }
        private async void LoadUserData()
        {
            var employee = await EmployeeContext.GetCurrentEmployee(_token);
            if (employee != null)
            {
                string UserName = employee.Full_Name;
                SystemUser.Text = $"Система: {employee.GetShortName(UserName)}";
            }
        }
        private async void LoadPermissions()
        {
            var employee = await EmployeeContext.GetCurrentEmployee(_token);
            if(employee.Position == "Кассир")
            {
                EmployeeBtn.Visibility = Visibility.Collapsed;
                CategoryBtn.Visibility = Visibility.Collapsed;
                SupplierBtn.Visibility = Visibility.Collapsed;
                SupplyBtn.Visibility = Visibility.Collapsed;
            } else if(employee.Position == "Менеджер")
            {
                EmployeeBtn.Visibility = Visibility.Collapsed;
                CategoryBtn.Visibility = Visibility.Collapsed;
                SaleBtn.Visibility = Visibility.Collapsed;
            }


        }

        private void CategoryClick(object sender, RoutedEventArgs e)
        {

        }

        private void EmployeeClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Employees.Main(_token));
        }

        private void ProductClick(object sender, RoutedEventArgs e)
        {

        }

        private void SaleClick(object sender, RoutedEventArgs e)
        {

        }

        private void SupplierClick(object sender, RoutedEventArgs e)
        {

        }

        private void SupplyClick(object sender, RoutedEventArgs e)
        {

        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Authorization());
            MainWindow.Token = null;
        }
    }
}

using Resonate.Context;
using Resonate.Model;
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

namespace Resonate.Pages.Employees
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
            LoadCurrentEmployees();
            LoadEmployees();
        }

        public async Task LoadEmployees() {
            List<Model.Employees> employees = await EmployeeContext.GetEmployees();
            foreach (var item in employees as List<Model.Employees>)
            {
                EmployeeParent.Children.Add(new Elements.Item(item));
            }
        }

        public async Task LoadCurrentEmployees()
        {
            var employee = await EmployeeContext.GetCurrentEmployee(_token);
            SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Main(_token));
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Employees.Add(_token));
        }
    }
}

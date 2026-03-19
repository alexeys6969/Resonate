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
        public Main()
        {
            InitializeComponent();
            LoadEmployees();
        }

        public async Task LoadEmployees() {
            List<Model.Employees> employees = await EmployeeContext.GetEmployees();
            foreach (var item in employees as List<Model.Employees>)
            {
                EmployeeParent.Children.Add(new Elements.Item(item));
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Employees.Add());
        }
    }
}

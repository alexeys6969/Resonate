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

namespace Resonate.Pages.Employees.Elements
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        Model.Employees employee;
        public Item(Model.Employees _employee)
        {
            InitializeComponent();
            employee = _employee;
            LoadItem();
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Employees.Add(MainWindow.Token, employee));
        }

        private void Delete(object sender, RoutedEventArgs e)
        {

        }

        private async void LoadItem()
        {
            var currentEmployee = await EmployeeContext.GetEmployeeById(employee.Id);
            FIO.Text = employee.Full_Name;
            Position.Text = employee.Position;
        }
    }
}

using Newtonsoft.Json.Linq;
using Resonate.Context;
using Resonate.Model;
using Resonate.Windows;
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
            MainWindow.init.frame.Navigate(new Pages.Employees.Add(employee));
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogWindow dialog = new DialogWindow($"Вы точно хотите удалить сотрудника {employee.Full_Name}?");
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    bool result = await EmployeeContext.DeleteEmployee(employee.Id);
                    if (result)
                    {
                        InfoWindow info = new InfoWindow($"Сотрудник {employee.Full_Name} успешно удален");
                        info.Show();
                        MainWindow.init.frame.Navigate(new Pages.Employees.Main());
                    }
                    else
                    {
                        InfoWindow info = new InfoWindow($"При удалении сотрудника {employee.Full_Name} возникла ошибка");
                        info.Show();
                    }
                }
            } catch(Exception ex)
            {
                InfoWindow info = new InfoWindow($"Возникла ошибка {ex.Message}");
                info.Show();
            }            
        }

        private async void LoadItem()
        {
            var currentEmployee = await EmployeeContext.GetEmployeeById(employee.Id);
            FIO.Text = employee.Full_Name;
            Position.Text = employee.Position;
        }
    }
}

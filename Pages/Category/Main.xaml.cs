using Newtonsoft.Json.Linq;
using Resonate.Context;
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

namespace Resonate.Pages.Category
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
            LoadCategories();
            LoadCurrentEmployees();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Main());
        }

        public async Task LoadCategories()
        {
            List<Model.Category> categories = await CategoryContext.GetCategories();
            foreach (var item in categories as List<Model.Category>)
            {
                CategoryParent.Children.Add(new Elements.Item(item));
            }
        }

        public async Task LoadCurrentEmployees()
        {
            var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
            SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Category.Add());
        }
    }
}

using MySql.Data.MySqlClient;
using Resonate_course_project.Models;
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

namespace Resonate_course_project.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public static MySqlConnection currentConnection;
        public List<Classes.Table> currentTables = new List<Classes.Table>();
        public Main(MySqlConnection _currentConnection)
        {
            InitializeComponent();
            currentConnection = _currentConnection;
            currentConnection.Open();
            LoadTables();
        }

        public void LoadTables()
        {
            currentTables.Clear(); // Очистить предыдущие данные
            using (var cmd = new MySqlCommand("SHOW TABLES", currentConnection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    string imagePath = "";
                    switch (tableName)
                    {
                        case "Categories":
                            imagePath = "\\Images\\category.png";
                            break;
                        case "Employees":
                            imagePath = "\\Images\\employeesIcon.png";
                            break;
                        case "Products":
                            imagePath = "\\Images\\product.png";
                            break;
                        case "Sales":
                            imagePath = "\\Images\\sales.png";
                            break;
                        case "Suppliers":
                            imagePath = "\\Images\\suppliers.png";
                            break;
                        case "Supplies":
                            imagePath = "\\Images\\supply.png";
                            break;
                    }
                    currentTables.Add(new Classes.Table(tableName, imagePath));
                }
            }

            foreach(var Item in currentTables)
            {
                if (Item.name == "Supply_items" || Item.name == "Sale_items")
                    continue;
                parentTables.Children.Add(new Elements.TableItem(Item));

            }
        }

        private void moveToEmployees(object sender, RoutedEventArgs e)
        {

        }
    }
}

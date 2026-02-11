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

namespace Resonate_course_project.Elements
{
    /// <summary>
    /// Логика взаимодействия для TableItem.xaml
    /// </summary>
    public partial class TableItem : UserControl
    {
        Classes.Table table;
        public TableItem(Classes.Table _table)
        {
            InitializeComponent();
            table = _table;
            string tableNameRuss = "";
            switch (table.name)
            {
                case "Categories":
                    tableNameRuss = "Категории";
                    break;
                case "Employees":
                    tableNameRuss = "Сотрудники";
                    break;
                case "Products":
                    tableNameRuss = "Товары";
                    break;
                case "Sales":
                    tableNameRuss = "Продажи";
                    break;
                case "Suppliers":
                    tableNameRuss = "Поставщики";
                    break;
                case "Supplies":
                    tableNameRuss = "Поставки";
                    break;
            }
            TableIcon.Source = new BitmapImage(new Uri(table.image, UriKind.Relative));
            TableName.Text = tableNameRuss;
        }

        private void OpenTable(object sender, RoutedEventArgs e)
        {

        }
    }
}

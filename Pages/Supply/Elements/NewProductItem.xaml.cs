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

namespace Resonate.Pages.Supply.Elements
{
    /// <summary>
    /// Логика взаимодействия для NewProductItem.xaml
    /// </summary>
    public partial class NewProductItem : UserControl
    {
        Pages.Supply.Add add;
        public NewProductItem(Add add)
        {
            InitializeComponent();
            this.add = add;
        }

        private void DeleteProduct(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

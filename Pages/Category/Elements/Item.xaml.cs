using Resonate.Context;
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

namespace Resonate.Pages.Category.Elements
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        Model.Category category;
        public Item(Model.Category _category)
        {
            InitializeComponent();
            category = _category;
            LoadItem();
        }

        private void EditInfo(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Category.Add(category));
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogWindow dialog = new DialogWindow($"Вы точно хотите удалить категорию {category.Name}?");
                dialog.ShowDialog();
                if (dialog.DialogResult == true)
                {
                    bool result = await CategoryContext.DeleteCategory(category.Id);
                    if (result)
                    {
                        InfoWindow info = new InfoWindow($"Категория {category.Name} успешно удален");
                        info.Show();
                        MainWindow.init.frame.Navigate(new Pages.Category.Main());
                    }
                    else
                    {
                        InfoWindow info = new InfoWindow($"При удалении категории {category.Name} возникла ошибка");
                        info.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                InfoWindow info = new InfoWindow($"Возникла ошибка {ex.Message}");
                info.Show();
            }
        }
        private async void LoadItem()
        {
            Name.Text = category.Name;
            Description.Text = category.Description;
        }
    }
}

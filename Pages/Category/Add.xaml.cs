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

namespace Resonate.Pages.Category
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Page
    {
        Model.Category category;
        public Add(Model.Category _category = null)
        {
            InitializeComponent();
            category = _category;
            LoadCurrentEmployees();
            if(category != null )
            {
                LoadCategoryDataInField(category);
            }
        }

        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Name.Text))
            {
                InfoWindow incorrectName = new InfoWindow("Введите название");
                incorrectName.Show();
                return;
            }
            if (String.IsNullOrWhiteSpace(Description.Text))
            {
                InfoWindow incorrectDescription = new InfoWindow("Введите описание");
                incorrectDescription.Show();
                return;
            }

            if (category != null)
            {
                try
                {
                    AddEdit.IsEnabled = false;
                    var updatedCategory = new Model.Category
                    {
                        Id = category.Id,
                        Name = Name.Text,
                        Description = Description.Text
                    };
                    bool result = await CategoryContext.UpdateCategory(updatedCategory.Id, updatedCategory);

                    if (result)
                    {
                        InfoWindow info = new InfoWindow("Информация о категории успешно обновлена");
                        info.Show();
                        MainWindow.init.frame.Navigate(new Pages.Category.Main());
                    }
                    else
                    {
                        InfoWindow info = new InfoWindow("Не удалось обновить информацию о категории");
                        info.Show();
                    }
                }
                catch (Exception ex)
                {
                    InfoWindow info = new InfoWindow($"Ошибка при обновлении: {ex.Message}");
                    info.Show();
                }
                finally
                {
                    AddEdit.IsEnabled = true;
                }
            }
            else
            {
                try
                {
                    AddEdit.IsEnabled = false;
                    var newCategory = new Model.Category
                    {
                        Name = Name.Text,
                        Description = Description.Text
                    };
                    Model.Category createdCategory = await CategoryContext.CreateCategory(newCategory);

                    if (createdCategory != null)
                    {
                        InfoWindow info = new InfoWindow($"Категория {createdCategory.Name} успешно создана!");
                        info.Show();
                        MainWindow.init.frame.Navigate(new Pages.Category.Main());
                    }
                    else
                    {
                        InfoWindow info = new InfoWindow("Не удалось создать категорию. Сервер вернул пустой ответ.");
                        info.Show();
                    }
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Ошибка при создании: {ex.Message}";
                    InfoWindow info = new InfoWindow(errorMessage);
                    info.Show();
                }
                finally
                {
                    AddEdit.IsEnabled = true;
                }
            }
        }
        private async void LoadCategoryDataInField(Model.Category category)
        {
            List<Model.Category> categories = await CategoryContext.GetCategories();
            if (category != null)
            {
                Name.Text = category.Name;
                Description.Text = category.Description;
                AddEdit.Content = "Изменить";
            }
        }
        public async Task LoadCurrentEmployees()
        {
            var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
            SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Category.Main());
        }
    }
}

using Newtonsoft.Json.Linq;
using Resonate.Context;
using Resonate.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Page
    {
        Model.Employees employee;
        public Add(Model.Employees _employee = null)
        {
            InitializeComponent();
            employee = _employee;
            LoadEmployeesDataInField(employee);
            LoadCurrentEmployees();
        }

        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrWhiteSpace(FIO.Text) || !Regex.IsMatch(FIO.Text, @"^[А-ЯЁ][а-яё]*(?:-[А-ЯЁ][а-яё]*)?\s[А-ЯЁ][а-яё]*(?:-[А-ЯЁ][а-яё]*)?\s[А-ЯЁ][а-яё]*(?:-[А-ЯЁ][а-яё]*)?$"))
            {
                InfoWindow incorrectFIO = new InfoWindow("Некорректное ФИО");
                incorrectFIO.Show();
                return;
            }
            if (Position.SelectedIndex == -1)
            {
                InfoWindow incorrectPosition = new InfoWindow("Выберите должность");
                incorrectPosition.Show();
                return;
            }
            if (String.IsNullOrWhiteSpace(Login.Text))
            {
                InfoWindow incorrectLogin = new InfoWindow("Введите логин");
                incorrectLogin.Show();
                return;
            }
            if (String.IsNullOrWhiteSpace(Pass.Password))
            {
                InfoWindow incorrectPassword = new InfoWindow("Введите пароль");
                incorrectPassword.Show();
                return;
            }
            if (employee != null)
            {
                try
                {
                    AddEdit.IsEnabled = false;
                    var updatedEmployee = new Model.Employees
                    {
                        Id = employee.Id,
                        Full_Name = FIO.Text,
                        Login = Login.Text,
                        Password = Pass.Password,
                        Position = Position.SelectedItem?.ToString() ?? employee.Position
                    };
                    bool result = await EmployeeContext.UpdateEmployee(updatedEmployee.Id, updatedEmployee);

                    if (result)
                    {
                        InfoWindow info = new InfoWindow("Информация о сотруднике успешно обновлена");
                        info.Show();
                        MainWindow.init.frame.Navigate(new Pages.Employees.Main());
                    }
                    else
                    {
                        InfoWindow info = new InfoWindow("Не удалось обновить информацию о сотруднике");
                        info.Show();
                    }
                }
                catch (Exception ex)
                {
                    InfoWindow info = new InfoWindow($"Ошибка при обновлении: { ex.Message }");
                    info.Show();
                }
                finally
                {
                    AddEdit.IsEnabled = true;
                }
            } else
            {
                try
                {
                    AddEdit.IsEnabled = false;
                    var newEmployee = new Model.Employees
                    {
                        Full_Name = FIO.Text,
                        Login = Login.Text,
                        Password = Pass.Password,
                        Position = Position.SelectedItem.ToString()
                    };
                    Model.Employees createdEmployee = await EmployeeContext.CreateEmployee(newEmployee);

                    if (createdEmployee != null)
                    {
                        InfoWindow info = new InfoWindow($"Сотрудник {createdEmployee.Full_Name} успешно создан!");
                        info.Show();
                    }
                    else
                    {
                        InfoWindow info = new InfoWindow("Не удалось создать сотрудника. Сервер вернул пустой ответ.");
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
        public async Task LoadCurrentEmployees()
        {
            var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
            SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Employees.Main());
        }

        private async void LoadEmployeesDataInField(Model.Employees employee)
        {
            List<Model.Employees> employees = await EmployeeContext.GetEmployees();
            var positions = employees
            .Select(e => e.Position)
            .Distinct()
            .OrderBy(p => p)
            .ToList();

            Position.ItemsSource = positions;
            if (employee != null)
            {
                FIO.Text = employee.Full_Name;
                Position.SelectedItem = employee.Position;
                Login.Text = employee.Login;
                Pass.Password = employee.Password;
                AddEdit.Content = "Изменить";
            }
        }
    }
}

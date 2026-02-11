using MySql.Data.MySqlClient;
using Resonate_course_project.Contexts;
using Resonate_course_project.DB;
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
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void AuthClk(object sender, RoutedEventArgs e)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("server=127.0.0.1;port=3307;database=resonate;uid=readonly_user;pwd=1111");
                conn.Open();

                // 2. Параметризованный запрос для поиска сотрудника
                string sql = "SELECT `full_name`, `password`, `position` FROM `Employees` WHERE `login` = @login";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login.Text);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // Нашли пользователя
                        {
                            // 3. Сравниваем хеш пароля
                            string storedHash = reader.GetString("password");
                            string enteredPassword = password.Password; // passwordBox - ваш элемент ввода пароля

                            if (BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash))
                            {
                                // Успешная авторизация
                                string position = reader.GetString("position");
                                try
                                {
                                    MySqlConnection roleConn = new MySqlConnection($"server=127.0.0.1;port=3307;database=resonate;uid={login.Text};pwd={enteredPassword}");
                                    MainWindow.init.frame.Navigate(new Pages.Main(roleConn));
                                }
                                catch (Exception)
                                {
                                    MessageBox.Show($"Подключение не открыто");
                                }

                            }
                            else
                            {
                                MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

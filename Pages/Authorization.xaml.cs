using Resonate.Services;
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

namespace Resonate.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        private readonly ApiService _apiService;
        public Authorization()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void Auth(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = await _apiService.LoginAsync(EmployeeLogin.Text, EmployeePassword.Password);

                if (response != null)
                {
                    // Сохраняем токен
                    TokenStorage.Instance.SaveToken(response.Token, response.Expiration, response.Employee);

                    // Переходим на главную страницу
                    NavigationService.Navigate(new Main());
                }
                else
                {
                    MessageBox.Show("Седня не \n На неделе го");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Седня не \n На неделе го");
            }
        }
    }
}

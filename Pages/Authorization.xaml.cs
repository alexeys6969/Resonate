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
using System.Net.Http;
using Resonate.Windows;

namespace Resonate.Pages
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

        public async Task Auth(string login, string password)
        {
            var Token = await EmployeeContext.Login(login, password);
            if (Token == null)
            {
                InfoWindow info = new InfoWindow("Пользователь не найден");
                info.Show();

            } else
            {
                MainWindow.Token = Token;
                MainWindow.init.frame.Navigate(new Pages.Main(Token));
            }
        }

        private void AuthBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmployeeLogin.Text))
            {
                InfoWindow info = new InfoWindow("Необходимо указать логин пользователя");
                info.Show();
                return;
            }
            if (string.IsNullOrEmpty(EmployeePassword.Password))
            {
                InfoWindow info = new InfoWindow("Необходимо указать пароль");
                info.Show();
                return;
            }
            Auth(EmployeeLogin.Text, EmployeePassword.Password);
        }
    }
}

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
                MessageBox.Show("Логин и пароль указаны неверно");
            } else
            {
                MainWindow.Token = Token;
                MainWindow.init.frame.Navigate(new Pages.Main());
            }
        }

        private void AuthBtn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmployeeLogin.Text))
            {
                MessageBox.Show("Необходимо указать логин пользователя");
                return;
            }
            if (string.IsNullOrEmpty(EmployeePassword.Password))
            {
                MessageBox.Show("Необходимо указать пароль");
                return;
            }
            Auth(EmployeeLogin.Text, EmployeePassword.Password);
        }
    }
}

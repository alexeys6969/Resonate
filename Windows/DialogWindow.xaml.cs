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
using System.Windows.Shapes;

namespace Resonate.Windows
{
    /// <summary>
    /// Логика взаимодействия для DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public bool? DialogResult { get; private set; }
        public DialogWindow(string question)
        {
            InitializeComponent();
            questionText.Text = question;
        }

        private void Yes(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void No(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

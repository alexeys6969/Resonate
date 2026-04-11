using Resonate.Model;
using Resonate.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Suppliers.Elements
{
    public partial class Item : UserControl
    {
        private Supplier supplier;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(58, 58, 58));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));

        public Item() 
        { 
            InitializeComponent(); 
        }

        public void LoadData(Supplier _supplier)
        {
            supplier = _supplier;
            RenderSupplier();
            AnimateEntrance();
        }

        private void AnimateEntrance()
        {
            this.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void RenderSupplier()
        {
            if (supplier == null) return;
            SupplierName.Text = supplier.Name ?? "Без названия";
            ContactInfo.Text = !string.IsNullOrWhiteSpace(supplier.Contact) ? supplier.Contact : "Контакты не указаны";
        }

        private void Edit(object sender, RoutedEventArgs e) => NavigateToAdd();
        private void Delete(object sender, RoutedEventArgs e)
        {
            var dialog = new DialogWindow($"Удалить поставщика \"{supplier?.Name}\"?");
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                NavigateToMain();
            }
        }

        private void NavigateToAdd()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.init.frame.Navigate(new Pages.Suppliers.Add(supplier));
            });
        }

        private void NavigateToMain()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.init.frame.Navigate(new Pages.Suppliers.Main());
            });
        }
    }
}
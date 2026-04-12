using Newtonsoft.Json.Linq;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Resonate.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private readonly SolidColorBrush _accentBrush = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _accentAltBrush = new SolidColorBrush(Color.FromRgb(36, 227, 237));
        public Main()
        {
            InitializeComponent();
            Loaded += Main_Loaded;
        }
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем данные пользователя
            LoadUserData();

            // Запускаем анимацию появления карточек
            AnimateCardsEntrance();
        }
        private void AnimateCardsEntrance()
        {
            if (MainWrapPanel?.Children == null) return;

            int delay = 0;
            foreach (UIElement child in MainWrapPanel.Children)
            {
                if (child is Button card)
                {
                    // Начальное состояние
                    card.Opacity = 0;
                    card.RenderTransform = new TranslateTransform(0, 30);

                    // Анимация появления
                    var fadeIn = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(400),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    var slideUp = new DoubleAnimation
                    {
                        From = 30,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(400),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    // Запуск с задержкой для эффекта "лесенки"
                    fadeIn.BeginTime = TimeSpan.FromMilliseconds(delay);
                    slideUp.BeginTime = TimeSpan.FromMilliseconds(delay);

                    card.BeginAnimation(OpacityProperty, fadeIn);
                    card.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);

                    delay += 80; // Задержка между карточками
                }
            }
        }

        /// <summary>
        /// Эффект нажатия на карточку (визуальный отклик)
        /// </summary>
        private void AnimateCardPress(Button card)
        {
            var scaleDown = new DoubleAnimation(0.98, TimeSpan.FromMilliseconds(100))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                AutoReverse = true
            };

            card.RenderTransform = new ScaleTransform(1, 1);
            card.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDown);
            card.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDown);
        }

        private void ReportForm(object sender, RoutedEventArgs e)
        {
            
        }
        private async void LoadUserData()
        {
            try
            {
                var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);

                if (employee != null)
                {
                    // Обновляем имя пользователя в UI-потоке
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string userName = employee.Full_Name;
                        SystemUser.Text = $"Система: {employee.GetShortName(userName)}";

                        // Настройка видимости кнопок по роли
                        ApplyRolePermissions(employee.Position);
                    });
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки (можно добавить свой логгер)
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных пользователя: {ex.Message}");
            }
        }
        private void ApplyRolePermissions(string position)
        {
            switch (position)
            {
                case "Кассир":
                    EmployeeBtn.Visibility = Visibility.Collapsed;
                    CategoryBtn.Visibility = Visibility.Collapsed;
                    SupplierBtn.Visibility = Visibility.Collapsed;
                    SupplyBtn.Visibility = Visibility.Collapsed;
                    break;

                case "Менеджер":
                    EmployeeBtn.Visibility = Visibility.Collapsed;
                    CategoryBtn.Visibility = Visibility.Collapsed;
                    SaleBtn.Visibility = Visibility.Collapsed;
                    break;

                    // Для администратора все кнопки доступны по умолчанию
            }
        }

        private void CategoryClick(object sender, RoutedEventArgs e)
        {
            AnimateCardPress(sender as Button);
            NavigateWithDelay(new Pages.Category.Main());
        }

        private void EmployeeClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Employees.Main());
        }

        private void ProductClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Products.Main());
        }

        private void SaleClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Sales.Main());
        }

        private void SupplierClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Suppliers.Main());
        }

        private void SupplyClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Supply.Main());
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            if (sender is Button exitBtn)
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                exitBtn.BeginAnimation(OpacityProperty, fadeOut);
            }

            // Задержка для анимации перед выходом
            _ = Task.Delay(200).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Authorization());
                    MainWindow.Token = null;
                });
            });
        }
        private async void NavigateWithDelay(Page targetPage)
        {
            await Task.Delay(150);
            MainWindow.init.frame.Navigate(targetPage);
        }
    }
}

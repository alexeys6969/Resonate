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
using System.Windows.Media.Animation;

namespace Resonate.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _focusBorderAlt = new SolidColorBrush(Color.FromRgb(36, 227, 237));
        public Authorization()
        {
            InitializeComponent();
            Loaded += Authorization_Loaded;
        }
        private void Authorization_Loaded(object sender, RoutedEventArgs e)
        {
            // Плавное появление карточки
            AnimateCardEntrance();

            // Автофокус на поле логина
            EmployeeLogin.Focus();
        }

        private void AnimateCardEntrance()
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideUp = new ThicknessAnimation
            {
                From = new Thickness(0, 30, 0, 0),
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            AuthCard.BeginAnimation(OpacityProperty, fadeIn);
            AuthCard.BeginAnimation(MarginProperty, slideUp);
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement input && input.Parent is Border border)
            {
                // Анимация смены цвета рамки
                var colorAnim = new ColorAnimation
                {
                    From = ((SolidColorBrush)border.BorderBrush).Color,
                    To = _focusBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                // Лёгкое свечение (увеличение толщины)
                var thicknessAnim = new ThicknessAnimation
                {
                    To = new Thickness(2),
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                border.BeginAnimation(Border.BorderThicknessProperty, thicknessAnim);
            }
        }

        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement input && input.Parent is Border border)
            {
                // Возврат к обычному состоянию
                var colorAnim = new ColorAnimation
                {
                    To = _defaultBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                var thicknessAnim = new ThicknessAnimation
                {
                    To = new Thickness(1.5),
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                border.BeginAnimation(Border.BorderThicknessProperty, thicknessAnim);
            }
        }

        public async Task Auth(string login, string password)
        {
            try
            {
                var token = await EmployeeContext.Login(login, password);

                if (token == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowInputError(PasswordBorder, "Неверный логин или пароль");
                    });
                }
                else
                {
                    MainWindow.Token = token;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.init.frame.Navigate(new Pages.Main());
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowInputError(LoginBorder, $"Ошибка соединения: {ex.Message}");
                });
                throw;
            }
        }

        private async void AuthBtn(object sender, RoutedEventArgs e)
        {
            // 🔒 Блокируем кнопку сразу, чтобы избежать повторных нажатий
            AuthButton.IsEnabled = false;
            var originalContent = AuthButton.Content;
            AuthButton.Content = "⏳ Проверка...";

            try
            {
                // 🔹 Валидация полей
                if (string.IsNullOrWhiteSpace(EmployeeLogin.Text))
                {
                    ShowInputError(LoginBorder, "Введите логин");
                    return;
                }
                if (string.IsNullOrWhiteSpace(EmployeePassword.Password))
                {
                    ShowInputError(PasswordBorder, "Введите пароль");
                    return;
                }

                // 🔹 Асинхронный вызов авторизации с await!
                await Auth(EmployeeLogin.Text, EmployeePassword.Password);
            }
            catch (Exception ex)
            {
                // 🔹 Ловим реальные ошибки (сеть, сервер, исключение в коде)
                ShowInputError(PasswordBorder, $"Ошибка: {ex.Message}");
            }
            finally
            {
                // 🔹 Возвращаем кнопку в исходное состояние (всегда!)
                AuthButton.IsEnabled = true;
                AuthButton.Content = originalContent;
            }
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb)
            {
                var pulse = new DoubleAnimation(0.8, 1, TimeSpan.FromMilliseconds(150))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                tb.BeginAnimation(OpacityProperty, pulse);
            }
        }
        private void ShowInputError(Border border, string message)
        {
            // Красная рамка на 1.5 секунды
            var errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));
            var originalBrush = border.BorderBrush;

            border.BorderBrush = errorBrush;

            Task.Delay(1500).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    border.BorderBrush = originalBrush;
                });
            });

            // Показать подсказку (можно заменить на Toast)
            InfoWindow incorrectAuthData = new InfoWindow(message);
            incorrectAuthData.Show();
        }
    }
}

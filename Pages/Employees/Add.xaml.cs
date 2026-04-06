using Resonate.Context;
using Resonate.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Employees
{
    public partial class Add : Page
    {
        private Model.Employees employee;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));

        // Регулярка для валидации ФИО (три слова, каждое с заглавной буквы)
        private readonly Regex _fioRegex = new Regex(@"^[А-ЯЁ][а-яё]*(?:-[А-ЯЁ][а-яё]*)?\s[А-ЯЁ][а-яё]*(?:-[А-ЯЁ][а-яё]*)?\s[А-ЯЁ][а-яё]*(?:-[А-ЯЁ][а-яё]*)?$", RegexOptions.Compiled);

        public Add(Model.Employees _employee = null)
        {
            InitializeComponent();
            employee = _employee;
            Loaded += Add_Loaded;
        }

        private async void Add_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadEmployeesDataInField(employee);
            await LoadCurrentEmployees();

            if (employee != null)
            {
                FormTitle.Text = "Редактирование сотрудника";
            }

            AnimateFormEntrance();
            FIO.Focus();
        }

        private void AnimateFormEntrance()
        {
            this.Opacity = 0;
            this.RenderTransform = new TranslateTransform(0, 20);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var slideUp = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(OpacityProperty, fadeIn);
            this.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control input && input.Parent is Border border)
            {
                var colorAnim = new ColorAnimation
                {
                    To = _focusBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
            // Скрыть ошибку при фокусе на поле
            ClearFieldError(sender);
        }

        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control input && input.Parent is Border border)
            {
                var colorAnim = new ColorAnimation
                {
                    To = _defaultBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
        }

        private void ClearFieldError(object sender)
        {
            if (sender == FIO) { FIOError.Visibility = Visibility.Collapsed; return; }
            if (sender == Position) { PositionError.Visibility = Visibility.Collapsed; return; }
            if (sender == Login) { LoginError.Visibility = Visibility.Collapsed; return; }
            if (sender == Pass) { PassError.Visibility = Visibility.Collapsed; return; }
        }

        private void FIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FIO.Text))
                FIOError.Visibility = Visibility.Collapsed;
        }

        private void Pass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Pass.Password))
                PassError.Visibility = Visibility.Collapsed;
        }

        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            // 🔹 Валидация полей
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(FIO.Text) || !_fioRegex.IsMatch(FIO.Text.Trim()))
            {
                ShowFieldError(FIOBorder, FIOError, "Введите ФИО в формате: Иванов Иван Иванович");
                FIO.Focus();
                isValid = false;
            }

            if (Position.SelectedItem == null)
            {
                ShowFieldError(PositionBorder, PositionError, "Выберите должность из списка");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Login.Text) || Login.Text.Length < 3)
            {
                ShowFieldError(LoginBorder, LoginError, "Логин должен содержать минимум 3 символа");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Pass.Password) || Pass.Password.Length < 6)
            {
                ShowFieldError(PassBorder, PassError, "Пароль должен содержать минимум 6 символов");
                isValid = false;
            }

            if (!isValid) return;

            // 🔹 Блокировка кнопки
            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = employee != null ? "💾 Сохранение..." : "✨ Создание...";

            try
            {
                if (employee != null)
                {
                    // 🔹 РЕДАКТИРОВАНИЕ
                    var updatedEmployee = new Model.Employees
                    {
                        Id = employee.Id,
                        Full_Name = FIO.Text.Trim(),
                        Login = Login.Text.Trim(),
                        Password = Pass.Password,
                        Position = Position.SelectedItem?.ToString() ?? employee.Position
                    };

                    bool result = await EmployeeContext.UpdateEmployee(updatedEmployee.Id, updatedEmployee);

                    if (result)
                    {
                        ShowSuccess("Данные сотрудника обновлены");
                        NavigateBack();
                    }
                    else
                    {
                        ShowError("Не удалось обновить данные");
                    }
                }
                else
                {
                    // 🔹 СОЗДАНИЕ
                    var newEmployee = new Model.Employees
                    {
                        Full_Name = FIO.Text.Trim(),
                        Login = Login.Text.Trim(),
                        Password = Pass.Password,
                        Position = Position.SelectedItem.ToString()
                    };

                    Model.Employees createdEmployee = await EmployeeContext.CreateEmployee(newEmployee);

                    if (createdEmployee != null)
                    {
                        ShowSuccess($"Сотрудник \"{createdEmployee.Full_Name}\" создан");
                        NavigateBack();
                    }
                    else
                    {
                        ShowError("Не удалось создать сотрудника");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                AddEdit.IsEnabled = true;
                AddEdit.Content = originalContent;
            }
        }

        private void ShowFieldError(Border border, TextBlock errorText, string message)
        {
            // Анимация красной рамки
            var errorAnim = new ColorAnimation
            {
                To = _errorBrush.Color,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true
            };
            border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, errorAnim);

            // Показать текст ошибки
            if (errorText != null)
            {
                errorText.Text = message;
                errorText.Visibility = Visibility.Visible;

                // Скрыть через 4 секунды
                _ = Task.Delay(4000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (errorText.Text == message)
                            errorText.Visibility = Visibility.Collapsed;
                    });
                });
            }
        }

        private void ShowSuccess(string message)
        {
            var info = new InfoWindow(message);
            info.Show();
        }

        private void ShowError(string message)
        {
            var info = new InfoWindow(message);
            info.Show();

            // Анимация "тряски" кнопки при ошибке
            var shake = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(300) };
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromPercent(0.25)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.FromPercent(0.5)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromPercent(0.75)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));

            AddEdit.RenderTransform = new TranslateTransform();
            AddEdit.RenderTransform.BeginAnimation(TranslateTransform.XProperty, shake);
        }

        private async void NavigateBack()
        {
            await Task.Delay(300);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.init.frame.Navigate(new Pages.Employees.Main());
            });
        }

        public async Task LoadCurrentEmployees()
        {
            try
            {
                var emp = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
                if (emp != null)
                {
                    SystemUser.Text = $"Система: {emp.GetShortName(emp.Full_Name)}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
                btn.BeginAnimation(OpacityProperty, fadeOut);
            }

            _ = Task.Delay(150).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Employees.Main());
                });
            });
        }

        public async Task LoadEmployeesDataInField(Model.Employees emp)
        {
            try
            {
                var employees = await EmployeeContext.GetEmployees(MainWindow.Token);
                var positions = employees
                    .Where(e => !string.IsNullOrWhiteSpace(e.Position))
                    .Select(e => e.Position)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();

                Position.ItemsSource = positions;

                if (emp != null)
                {
                    FIO.Text = emp.Full_Name;
                    Login.Text = emp.Login;
                    Pass.Password = emp.Password;

                    // Выбор должности в ComboBox
                    if (positions.Contains(emp.Position))
                        Position.SelectedItem = emp.Position;

                    AddEdit.Content = "💾 Сохранить изменения";
                    FormTitle.Text = "Редактирование сотрудника";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки списка сотрудников: {ex.Message}");
            }
        }
    }
}
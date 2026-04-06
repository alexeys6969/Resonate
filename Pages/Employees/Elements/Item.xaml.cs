using Resonate.Context;
using Resonate.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Employees.Elements
{
    public partial class Item : UserControl
    {
        private Model.Employees employee;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(58, 58, 58));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));

        public Item(Model.Employees _employee)
        {
            InitializeComponent();
            employee = _employee;
            Loaded += Item_Loaded;
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItem();
            AnimateEntrance();
        }

        /// <summary>
        /// Анимация появления элемента
        /// </summary>
        private void AnimateEntrance()
        {
            this.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Анимация нажатия на кнопку
        /// </summary>
        private void AnimateButtonClick(Button button)
        {
            var scaleDown = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            button.RenderTransform = new ScaleTransform(1, 1);
            button.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDown);
            button.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDown);
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Employees.Add(employee));
                });
            });
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            try
            {
                var dialog = new DialogWindow($"Вы точно хотите удалить сотрудника \"{employee.Full_Name}\"?");
                dialog.ShowDialog();

                if (dialog.DialogResult == true)
                {
                    // Анимация удаления (исчезновение)
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    fadeOut.Completed += async (s, args) =>
                    {
                        bool result = await EmployeeContext.DeleteEmployee(employee.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (result)
                            {
                                var info = new InfoWindow($"Сотрудник \"{employee.Full_Name}\" успешно удалён");
                                info.Show();

                                // Перезагрузка списка сотрудников
                                MainWindow.init.frame.Navigate(new Pages.Employees.Main());
                            }
                            else
                            {
                                var info = new InfoWindow($"При удалении сотрудника \"{employee.Full_Name}\" возникла ошибка");
                                info.Show();

                                // Возвращаем видимость при ошибке
                                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                                this.BeginAnimation(OpacityProperty, fadeIn);
                            }
                        });
                    };

                    this.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
            catch (Exception ex)
            {
                var info = new InfoWindow($"Возникла ошибка: {ex.Message}");
                info.Show();
            }
        }

        private async void LoadItem()
        {
            try
            {
                // Загружаем актуальные данные с сервера
                var currentEmployee = await EmployeeContext.GetEmployeeById(employee.Id);

                if (currentEmployee != null)
                {
                    employee = currentEmployee; // Обновляем локальную ссылку
                }

                FIO.Text = employee?.Full_Name ?? "Неизвестный сотрудник";
                Position.Text = employee?.Position ?? "Должность не указана";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сотрудника: {ex.Message}");
                FIO.Text = employee?.Full_Name ?? "Ошибка загрузки";
                Position.Text = "Не удалось получить данные";
            }
        }
    }
}
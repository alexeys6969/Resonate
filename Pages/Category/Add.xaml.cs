using Resonate.Context;
using Resonate.Windows;
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

namespace Resonate.Pages.Category
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Page
    {
        private Model.Category category;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));
        public Add(Model.Category _category = null)
        {
            InitializeComponent();
            category = _category;
            Loaded += Add_Loaded;
        }
        private async void Add_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrentEmployees();

            if (category != null)
            {
                LoadCategoryDataInField(category);
                FormTitle.Text = "Редактирование категории";
            }

            AnimateFormEntrance();
            Name.Focus();
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
            if (sender is TextBox tb && tb.Parent is Border border)
            {
                var colorAnim = new ColorAnimation
                {
                    To = _focusBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
            // Скрыть ошибку при фокусе
            if (sender == Name) NameError.Visibility = Visibility.Collapsed;
        }

        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Parent is Border border)
            {
                var colorAnim = new ColorAnimation
                {
                    To = _defaultBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
        }

        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                ShowFieldError(NameBorder, NameError, "Введите название категории");
                Name.Focus();
                return;
            }
            if (Name.Text.Length < 2)
            {
                ShowFieldError(NameBorder, NameError, "Название должно содержать минимум 2 символа");
                return;
            }
            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                // Описание необязательно, но можно предупредить
                // ShowFieldError(DescriptionBorder, null, "Описание поможет лучше организовать каталог");
            }

            // Блокировка кнопки с анимацией
            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = category != null ? "💾 Сохранение..." : "✨ Создание...";

            try
            {
                if (category != null)
                {
                    // 🔹 РЕДАКТИРОВАНИЕ
                    var updatedCategory = new Model.Category
                    {
                        Id = category.Id,
                        Name = Name.Text.Trim(),
                        Description = Description.Text?.Trim()
                    };

                    bool result = await CategoryContext.UpdateCategory(updatedCategory.Id, updatedCategory);

                    if (result)
                    {
                        ShowSuccess("Категория успешно обновлена");
                        NavigateBack();
                    }
                    else
                    {
                        ShowError("Не удалось обновить категорию");
                    }
                }
                else
                {
                    // 🔹 СОЗДАНИЕ
                    var newCategory = new Model.Category
                    {
                        Name = Name.Text.Trim(),
                        Description = Description.Text?.Trim()
                    };

                    Model.Category createdCategory = await CategoryContext.CreateCategory(newCategory);

                    if (createdCategory != null)
                    {
                        ShowSuccess($"Категория \"{createdCategory.Name}\" создана");
                        NavigateBack();
                    }
                    else
                    {
                        ShowError("Не удалось создать категорию");
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
        private async void LoadCategoryDataInField(Model.Category cat)
        {
            if (cat != null)
            {
                Name.Text = cat.Name;
                Description.Text = cat.Description;
                AddEdit.Content = "💾 Сохранить изменения";
                FormTitle.Text = "Редактирование категории";
            }
        }
        public async Task LoadCurrentEmployees()
        {
            try
            {
                var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
                if (employee != null)
                {
                    SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
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
                    MainWindow.init.frame.Navigate(new Pages.Category.Main());
                });
            });
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

                // Скрыть через 3 секунды
                _ = Task.Delay(3000).ContinueWith(_ =>
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

            // Анимация ошибки на кнопке
            var shake = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
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
            await Task.Delay(300); // Небольшая задержка для показа уведомления
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.init.frame.Navigate(new Pages.Category.Main());
            });
        }


        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Name.Text))
                NameError.Visibility = Visibility.Collapsed;
        }
    }
}

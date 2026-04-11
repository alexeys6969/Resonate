using Resonate.Context;
using Resonate.Model;
using Resonate.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Suppliers
{
    public partial class Add : Page
    {
        private Supplier supplier;

        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));

        public Add(Supplier _supplier = null)
        {
            InitializeComponent();
            supplier = _supplier;
            Loaded += Add_Loaded;
        }

        private void Add_Loaded(object sender, RoutedEventArgs e)
        {
            if (supplier != null)
            {
                LoadSupplierData();
                FormTitle.Text = "Редактирование поставщика";
            }

            AnimateFormEntrance();
            Name.Focus();
        }

        /// <summary>
        /// Анимация появления формы
        /// </summary>
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

        /// <summary>
        /// Обработчик фокуса на поле ввода (подсветка рамки)
        /// </summary>
        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control input && input.Parent is Border border)
            {
                var anim = new ColorAnimation
                {
                    To = _focusBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }

            ClearErrors();
        }

        /// <summary>
        /// Обработчик потери фокуса (возврат цвета рамки)
        /// </summary>
        private void Input_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control input && input.Parent is Border border)
            {
                var anim = new ColorAnimation
                {
                    To = _defaultBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        /// <summary>
        /// Скрывает ошибку валидации
        /// </summary>
        private void ClearErrors()
        {
            NameError.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Загружает данные поставщика в поля формы
        /// </summary>
        private void LoadSupplierData()
        {
            if (supplier != null)
            {
                Name.Text = supplier.Name;
                ContactInfo.Text = supplier.Contact;
                AddEdit.Content = "💾 Сохранить изменения";
            }
        }

        /// <summary>
        /// Обработчик кнопки сохранения (создание или редактирование)
        /// </summary>
        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                ShowError(NameBorder, NameError, "Введите название");
                return;
            }

            // Блокировка кнопки
            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = "⏳ Сохранение...";

            try
            {
                var data = new Supplier
                {
                    Id = supplier?.Id ?? 0,
                    Name = Name.Text.Trim(),
                    Contact = ContactInfo.Text?.Trim()
                };

                if (supplier != null)
                {
                    // Редактирование
                    await SupplierContext.UpdateSupplier(data.Id, data);
                    ShowSuccess("Поставщик обновлён");
                }
                else
                {
                    // Создание
                    var created = await SupplierContext.CreateSupplier(data);
                    ShowSuccess("Поставщик создан");
                }

                NavigateBack();
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

        /// <summary>
        /// Показывает ошибку валидации поля (красная рамка + текст)
        /// </summary>
        private void ShowError(Border border, TextBlock errorText, string message)
        {
            var anim = new ColorAnimation
            {
                To = _errorBrush.Color,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true
            };
            border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);

            if (errorText != null)
            {
                errorText.Text = message;
                errorText.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Показывает общую ошибку (информационное окно + тряска кнопки)
        /// </summary>
        private void ShowError(string message)
        {
            var info = new InfoWindow(message);
            info.Show();

            // Анимация тряски кнопки
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

        /// <summary>
        /// Показывает успешное сообщение
        /// </summary>
        private void ShowSuccess(string message)
        {
            var info = new InfoWindow(message);
            info.Show();
        }

        /// <summary>
        /// Возврат на список поставщиков
        /// </summary>
        private void NavigateBack()
        {
            _ = Task.Delay(300).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Suppliers.Main());
                });
            });
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }
    }
}
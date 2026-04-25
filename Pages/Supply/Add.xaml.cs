using Resonate.Context;
using Resonate.Model;
using Resonate.Pages.Supply.Elements;
using Resonate.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Supply
{
    public partial class Add : Page
    {
        private Supply supply;

        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));
        private readonly Regex _dateRegex = new Regex(@"^\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}$", RegexOptions.Compiled);
        private readonly List<SupplyItemData> _cartItems = new List<SupplyItemData>();

        public Add(Supply _supply = null)
        {
            InitializeComponent();
            supply = _supply;
            Loaded += Add_Loaded;
        }

        private void Add_Loaded(object sender, RoutedEventArgs e)
        {
            if (supply != null)
            {
                LoadSupplyData();
                FormTitle.Text = "✏️ Редактирование поставки";
            }

            AnimateFormEntrance();
            Code.Focus();

            if (string.IsNullOrWhiteSpace(Code.Text))
                Code.Text = $"SUPP-{DateTime.Now:yyyyMMdd-HHmm}";
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
        /// Обработчик получения фокуса полем ввода (подсветка рамки)
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
        /// Обработчик потери фокуса полем ввода (возврат цвета рамки)
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
            CodeError.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Загружает данные поставки в поля формы (при редактировании)
        /// </summary>
        private void LoadSupplyData()
        {
            if (supply != null)
            {
                Code.Text = supply.Code;
                DateTimeSale.Text = supply.Supply_Date?.ToString("dd.MM.yyyy HH:mm");
                AddEdit.Content = "💾 Сохранить изменения";
            }
        }

        /// <summary>
        /// Добавляет новую строку товара в чек поставки
        /// </summary>
        private void AddProduct(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            var newItem = new NewProductItem(this);
            newItem.ItemChanged += (s, args) => RecalculateTotal();

            _ = LoadProductsToComboBox(newItem.Product);
            NewProductParent.Children.Add(newItem);

            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    newItem.BringIntoView();
                    newItem.Product.Focus();
                });
            });
        }

        /// <summary>
        /// Загружает список товаров в ComboBox
        /// </summary>
        private async Task LoadProductsToComboBox(ComboBox comboBox)
        {
            try
            {
                var products = await ProductContext.GetProducts();
                comboBox.ItemsSource = products?.OrderBy(p => p.Name).ToList();
                comboBox.DisplayMemberPath = "Name";
                comboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Пересчитывает итоговую сумму поставки
        /// </summary>
        private void RecalculateTotal()
        {
            decimal total = 0;
            _cartItems.Clear();

            foreach (var child in NewProductParent.Children.OfType<NewProductItem>())
            {
                var data = child.GetSupplyItemData();
                if (data != null)
                {
                    total += data.LineTotal;
                    _cartItems.Add(data);
                }
            }

            TotalAmount.Text = $"{total:N2} ₽";
            AnimateTotalChange();
        }

        /// <summary>
        /// Анимация изменения итоговой суммы
        /// </summary>
        private void AnimateTotalChange()
        {
            var scale = new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            TotalAmount.RenderTransform = new ScaleTransform(1, 1);
            TotalAmount.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scale);
            TotalAmount.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale);
        }

        /// <summary>
        /// Анимация нажатия на кнопку
        /// </summary>
        private void AnimateButtonClick(Button button)
        {
            var scale = new DoubleAnimation(0.98, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            button.RenderTransform = new ScaleTransform(1, 1);
            button.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scale);
            button.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale);
        }

        /// <summary>
        /// Обработчик кнопки сохранения (создание или редактирование поставки)
        /// </summary>
        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            // 🔹 Валидация полей
            if (string.IsNullOrWhiteSpace(Code.Text))
            {
                ShowError(CodeBorder, CodeError, "Введите код поставки");
                return;
            }

            if (!DateTime.TryParseExact(DateTimeSale.Text, "dd.MM.yyyy HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime supplyDate))
            {
                ShowError(DateBorder, null, "Дата в формате: дд.мм.гггг чч:мм");
                return;
            }

            if (!_cartItems.Any())
            {
                // ✅ Используем кастомное окно вместо MessageBox
                new InfoWindow("Добавьте хотя бы один товар").Show();
                return;
            }

            // 🔹 Блокировка кнопки
            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = "⏳ Оформление...";

            try
            {
                // TODO: Реальная логика сохранения через SupplyContext
                if (supply != null)
                {
                    // await SupplyContext.UpdateSupply(supply.Id, updateRequest);
                    new InfoWindow("Поставка обновлена").Show();
                }
                else
                {
                    // var created = await SupplyContext.CreateSupply(createRequest);
                    new InfoWindow("Поставка создана").Show();
                }
                NavigateBack();
            }
            catch (Exception ex)
            {
                new InfoWindow($"Ошибка: {ex.Message}").Show();
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
        /// Возврат на список поставок
        /// </summary>
        private void NavigateBack()
        {
            _ = Task.Delay(300).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Supply.Main());
                });
            });
        }

        /// <summary>
        /// Выход на главную страницу
        /// </summary>
        private void Exit(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }
    }
}
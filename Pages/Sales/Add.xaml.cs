using Resonate.Context;
using Resonate.Model;
using Resonate.Pages.Sales.Elements;
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
using Resonate.Model.SaleClasses;

namespace Resonate.Pages.Sales
{
    public partial class Add : Page
    {
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));

        // Регулярка для валидации даты: дд.мм.гггг чч:мм
        private readonly Regex _dateRegex = new Regex(@"^\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}$", RegexOptions.Compiled);

        // Список товаров для отправки на сервер
        private readonly List<SaleItemData> _cartItems = new List<SaleItemData>();

        public Add()
        {
            InitializeComponent();
            Loaded += Add_Loaded;
        }

        private async void Add_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCashiers();
            AnimateFormEntrance();
            Code.Focus();

            // Генерация кода продажи по умолчанию
            if (string.IsNullOrWhiteSpace(Code.Text))
                Code.Text = $"SALE-{DateTime.Now:yyyyMMdd-HHmm}";
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
            if (sender == Code) { CodeError.Visibility = Visibility.Collapsed; return; }
            if (sender == Cashier) { CashierError.Visibility = Visibility.Collapsed; return; }
            if (sender == DateTimeSale) { DateTimeError.Visibility = Visibility.Collapsed; return; }
        }

        private void Code_TextChanged(object sender, TextChangedEventArgs e) => ClearFieldError(sender);
        private void Cashier_SelectionChanged(object sender, SelectionChangedEventArgs e) => ClearFieldError(sender);
        private void DateTimeSale_TextChanged(object sender, TextChangedEventArgs e) => ClearFieldError(sender);

        /// <summary>
        /// Загружает список кассиров (сотрудников)
        /// </summary>
        private async Task LoadCashiers()
        {
            try
            {
                var employees = await EmployeeContext.GetEmployees(MainWindow.Token);
                var cashiers = employees?.Where(e => e.Position == "Кассир" || e.Position == "Администратор").ToList();

                Cashier.Items.Clear();
                if (cashiers != null)
                {
                    foreach (var emp in cashiers.OrderBy(e => e.Full_Name))
                    {
                        Cashier.Items.Add(emp);
                    }
                }

                // Автовыбор текущего пользователя если он кассир
                var current = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
                if (current != null && cashiers?.Any(c => c.Id == current.Id) == true)
                {
                    Cashier.SelectedItem = current;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки кассиров: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавляет новую строку товара в чек
        /// </summary>
        private void AddProduct(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            var newItem = new NewProductItem(this);

            // Подписка на изменение строки для пересчёта итога
            newItem.ItemChanged += (s, args) => RecalculateTotal();

            // Загружаем товары в ComboBox нового элемента
            _ = LoadProductsToComboBox(newItem.Product);

            NewProductParent.Children.Add(newItem);

            // Прокрутка к новому элементу
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
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        /// <summary>
        /// Пересчитывает итоговую сумму чека
        /// </summary>
        private void RecalculateTotal()
        {
            decimal total = 0;
            _cartItems.Clear();

            foreach (var child in NewProductParent.Children.OfType<NewProductItem>())
            {
                var data = child.GetSaleItemData();
                if (data != null)
                {
                    total += data.LineTotal;
                    _cartItems.Add(data);
                }
            }

            TotalAmount.Text = $"{total:N2} ₽";

            // Анимация изменения суммы
            AnimateTotalChange();
        }

        private void AnimateTotalChange()
        {
            var scaleUp = new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            TotalAmount.RenderTransform = new ScaleTransform(1, 1);
            TotalAmount.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            TotalAmount.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
        }

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
        /// Обработчик кнопки сохранения продажи
        /// </summary>
        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            // 🔹 Валидация полей
            bool isValid = true;
            Model.Employees selectedCashier = null; // ✅ Инициализируем переменную

            if (string.IsNullOrWhiteSpace(Code.Text) || Code.Text.Length < 5)
            {
                ShowFieldError(CodeBorder, CodeError, "Введите корректный код продажи");
                isValid = false;
            }

            // ✅ Вместо pattern matching используем传统ный cast
            selectedCashier = Cashier.SelectedItem as Model.Employees;
            if (selectedCashier == null)
            {
                ShowFieldError(CashierBorder, CashierError, "Выберите кассира");
                isValid = false;
            }

            if (!_dateRegex.IsMatch(DateTimeSale.Text?.Trim() ?? ""))
            {
                ShowFieldError(DateTimeBorder, DateTimeError, "Дата в формате: дд.мм.гггг чч:мм");
                isValid = false;
            }

            if (!_cartItems.Any())
            {
                MessageBox.Show("Добавьте хотя бы один товар в чек", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (!isValid) return;

            // 🔹 Блокировка кнопки
            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = "⏳ Оформление...";

            try
            {
                // Парсинг даты
                if (!DateTime.TryParseExact(DateTimeSale.Text, "dd.MM.yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime saleDate))
                {
                    ShowError("Неверный формат даты");
                    return;
                }

                // 🔹 Формирование запроса для API
                var createRequest = new Model.SaleClasses.CreateSaleRequest
                {
                    employee_id = selectedCashier.Id,
                    items = _cartItems.Select(item => new SaleItemRequest
                    {
                        product_id = item.ProductId,
                        quantity = item.Quantity,
                        price_at_sale = item.Price
                    }).ToList()
                };

                // 🔹 Отправка на сервер через SaleContext
                var createdSale = await SaleContext.CreateSale(createRequest);

                if (createdSale != null)
                {
                    ShowSuccess($"Продажа \"{createdSale.Code}\" оформлена на сумму {createdSale.Total_Amount:N2} ₽");
                    NavigateBack();
                }
                else
                {
                    ShowError("Не удалось оформить продажу. Сервер вернул пустой ответ.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при создании продажи: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                AddEdit.IsEnabled = true;
                AddEdit.Content = originalContent;
            }
        }

        private void ShowFieldError(Border border, TextBlock errorText, string message)
        {
            var errorAnim = new ColorAnimation
            {
                To = _errorBrush.Color,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true
            };
            border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, errorAnim);

            if (errorText != null)
            {
                errorText.Text = message;
                errorText.Visibility = Visibility.Visible;

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
                MainWindow.init.frame.Navigate(new Pages.Sales.Main());
            });
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
                    MainWindow.init.frame.Navigate(new Pages.Main());
                });
            });
        }
    }
}
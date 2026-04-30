using Resonate.Context;
using Resonate.Model;
using Resonate.Model.SaleClasses;
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
using EmployeeModel = Resonate.Model.Employees;
using SaleModel = Resonate.Model.SaleClasses.Sale;

namespace Resonate.Pages.Sales
{
    public partial class Add : Page
    {
        private SaleModel sale;
        private int? _currentEmployeeId;

        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));
        private readonly Regex _dateRegex = new Regex(@"^\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}$", RegexOptions.Compiled);
        private readonly List<SaleItemData> _cartItems = new List<SaleItemData>();
        private readonly List<Product> _availableProducts = new List<Product>();
        private readonly List<EmployeeModel> _availableCashiers = new List<EmployeeModel>();

        public Add(SaleModel currentSale = null)
        {
            InitializeComponent();
            sale = currentSale;
            Loaded += Add_Loaded;
        }

        private async void Add_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrentEmployee();
            await LoadCashiers();
            await LoadProducts();

            if (sale != null)
            {
                if (sale.Sale_Items == null || !sale.Sale_Items.Any())
                {
                    var fullSale = await SaleContext.GetSaleById(sale.Id);
                    if (fullSale != null)
                        sale = fullSale;
                }

                LoadSaleData();
                FormTitle.Text = "✏️ Редактирование продажи";
            }
            else
            {
                DateTimeSale.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }

            AnimateFormEntrance();
            Code.Focus();

            if (string.IsNullOrWhiteSpace(Code.Text))
                Code.Text = $"SALE-{DateTime.Now:yyyyMMdd-HHmm}";
        }

        private async Task LoadCurrentEmployee()
        {
            try
            {
                var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
                if (employee != null)
                {
                    _currentEmployeeId = employee.Id;
                    SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private async Task LoadCashiers()
        {
            try
            {
                _availableCashiers.Clear();

                var employees = await EmployeeContext.GetEmployees(MainWindow.Token) ?? new List<EmployeeModel>();
                _availableCashiers.AddRange(employees
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Full_Name))
                    .Where(x => x.Position == "Кассир" || x.Position == "Администратор")
                    .OrderBy(x => x.Full_Name));

                if (sale != null && sale.Employee_id > 0 && _availableCashiers.All(x => x.Id != sale.Employee_id))
                {
                    _availableCashiers.Add(sale.Employee ?? new EmployeeModel
                    {
                        Id = sale.Employee_id,
                        Full_Name = $"Сотрудник #{sale.Employee_id}",
                        Position = "Кассир"
                    });
                }

                Cashier.ItemsSource = null;
                Cashier.DisplayMemberPath = "Full_Name";
                Cashier.SelectedValuePath = "Id";
                Cashier.ItemsSource = _availableCashiers.OrderBy(x => x.Full_Name).ToList();

                if (sale == null && _currentEmployeeId.HasValue)
                {
                    var currentCashier = _availableCashiers.FirstOrDefault(x => x.Id == _currentEmployeeId.Value);
                    if (currentCashier != null)
                        Cashier.SelectedItem = currentCashier;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки кассиров: {ex.Message}");
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                _availableProducts.Clear();
                _availableProducts.AddRange((await ProductContext.GetProducts() ?? new List<Product>()).OrderBy(x => x.Name));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        private void AnimateFormEntrance()
        {
            Opacity = 0;
            RenderTransform = new TranslateTransform(0, 20);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var slideUp = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(400))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            BeginAnimation(OpacityProperty, fadeIn);
            RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

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

            ClearFieldError(sender);
        }

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

        private void Code_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearFieldError(sender);
        }

        private void Cashier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearFieldError(sender);
        }

        private void DateTimeSale_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearFieldError(sender);
        }

        private void ClearFieldError(object sender)
        {
            if (sender == Code)
            {
                CodeError.Visibility = Visibility.Collapsed;
                return;
            }

            if (sender == Cashier)
            {
                CashierError.Visibility = Visibility.Collapsed;
                return;
            }

            if (sender == DateTimeSale)
                DateTimeError.Visibility = Visibility.Collapsed;
        }

        private void LoadSaleData()
        {
            Code.Text = string.IsNullOrWhiteSpace(sale.Code) ? $"SALE-{sale.Id}" : sale.Code;
            DateTimeSale.Text = sale.Sale_Date.ToString("dd.MM.yyyy HH:mm");
            AddEdit.Content = "💾 Сохранить изменения";

            var selectedCashier = _availableCashiers.FirstOrDefault(x => x.Id == sale.Employee_id);
            if (selectedCashier != null)
                Cashier.SelectedItem = selectedCashier;

            LoadSaleItems();
        }

        private void LoadSaleItems()
        {
            NewProductParent.Children.Clear();

            if (sale == null || sale.Sale_Items == null || !sale.Sale_Items.Any())
            {
                RecalculateTotal();
                return;
            }

            foreach (var saleItem in sale.Sale_Items)
            {
                var newItem = CreateProductItem();
                var product = _availableProducts.FirstOrDefault(x => x.Id == saleItem.Product_id)
                    ?? new Product
                    {
                        Id = saleItem.Product_id,
                        Name = saleItem.Product != null ? saleItem.Product.Name : $"Товар #{saleItem.Product_id}",
                        Price = saleItem.Price_At_Sale
                    };

                if (_availableProducts.All(x => x.Id != product.Id))
                {
                    newItem.Product.ItemsSource = _availableProducts.Concat(new[] { product })
                        .GroupBy(x => x.Id)
                        .Select(x => x.First())
                        .OrderBy(x => x.Name)
                        .ToList();
                }

                NewProductParent.Children.Add(newItem);
                newItem.SetItem(saleItem.Id, product, saleItem.Quantity, saleItem.Price_At_Sale);
            }

            RecalculateTotal();
        }

        private NewProductItem CreateProductItem()
        {
            var newItem = new NewProductItem(this);
            newItem.ItemChanged += delegate { RecalculateTotal(); };

            newItem.Product.ItemsSource = _availableProducts.ToList();
            newItem.Product.DisplayMemberPath = "Name";
            newItem.Product.SelectedValuePath = "Id";

            return newItem;
        }

        private void AddProduct(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            var newItem = CreateProductItem();
            NewProductParent.Children.Add(newItem);

            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    newItem.BringIntoView();
                    newItem.FocusProduct();
                });
            });
        }

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
            AnimateTotalChange();
        }

        public void RefreshTotals()
        {
            RecalculateTotal();
        }

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

        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            bool isValid = true;
            var selectedCashier = Cashier.SelectedItem as EmployeeModel;
            DateTime saleDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(Code.Text) || Code.Text.Length < 5)
            {
                ShowFieldError(CodeBorder, CodeError, "Введите корректный код продажи");
                isValid = false;
            }

            if (selectedCashier == null)
            {
                ShowFieldError(CashierBorder, CashierError, "Выберите кассира");
                isValid = false;
            }

            if (!_dateRegex.IsMatch(DateTimeSale.Text ?? string.Empty) ||
                !DateTime.TryParseExact(DateTimeSale.Text, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out saleDate))
            {
                ShowFieldError(DateTimeBorder, DateTimeError, "Дата в формате: дд.мм.гггг чч:мм");
                isValid = false;
            }

            if (!_cartItems.Any())
            {
                new InfoWindow("Добавьте хотя бы один товар").Show();
                isValid = false;
            }

            if (!isValid)
                return;

            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = "⏳ Сохранение...";

            try
            {
                if (sale != null)
                {
                    var updateRequest = BuildUpdateRequest(selectedCashier, saleDate);
                    bool updated = await SaleContext.UpdateSale(sale.Id, updateRequest);
                    if (!updated)
                        throw new Exception("Сервер не подтвердил обновление продажи.");

                    new InfoWindow("Продажа обновлена").Show();
                }
                else
                {
                    var createRequest = new CreateSaleRequest
                    {
                        Code = Code.Text.Trim(),
                        Sale_Date = saleDate,
                        employee_id = selectedCashier.Id,
                        items = _cartItems.Select(item => new SaleItemRequest
                        {
                            product_id = item.ProductId,
                            quantity = item.Quantity,
                            price_at_sale = item.Price
                        }).ToList()
                    };

                    var createdSale = await SaleContext.CreateSale(createRequest);
                    if (createdSale == null)
                        throw new Exception("Сервер вернул пустой ответ.");

                    new InfoWindow("Продажа создана").Show();
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

        private UpdateSaleFullRequest BuildUpdateRequest(EmployeeModel selectedCashier, DateTime saleDate)
        {
            var request = new UpdateSaleFullRequest
            {
                Sale = new SaleUpdateData
                {
                    Code = Code.Text.Trim(),
                    Employee_id = selectedCashier.Id,
                    Sale_Date = saleDate
                },
                Items = new List<SaleItemUpdateRequest>()
            };

            foreach (var item in _cartItems)
            {
                request.Items.Add(new SaleItemUpdateRequest
                {
                    Id = item.ItemId,
                    Product_id = item.ProductId,
                    Quantity = item.Quantity,
                    Price_At_Sale = item.Price,
                    Action = item.ItemId > 0 ? "update" : "add"
                });
            }

            if (sale != null && sale.Sale_Items != null)
            {
                var existingIds = new HashSet<int>(_cartItems.Where(x => x.ItemId > 0).Select(x => x.ItemId));
                foreach (var item in sale.Sale_Items.Where(x => !existingIds.Contains(x.Id)))
                {
                    request.Items.Add(new SaleItemUpdateRequest
                    {
                        Id = item.Id,
                        Product_id = item.Product_id,
                        Quantity = item.Quantity,
                        Price_At_Sale = item.Price_At_Sale,
                        Action = "delete"
                    });
                }
            }

            return request;
        }

        private void ShowFieldError(Border border, TextBlock errorText, string message)
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

        private void ShowError(string message)
        {
            new InfoWindow(message).Show();

            var shake = new DoubleAnimationUsingKeyFrames { Duration = TimeSpan.FromMilliseconds(300) };
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(0)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromPercent(0.25)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.FromPercent(0.5)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromPercent(0.75)));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));

            AddEdit.RenderTransform = new TranslateTransform();
            AddEdit.RenderTransform.BeginAnimation(TranslateTransform.XProperty, shake);
        }

        private void NavigateBack()
        {
            _ = Task.Delay(300).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Sales.Main());
                });
            });
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }
    }
}

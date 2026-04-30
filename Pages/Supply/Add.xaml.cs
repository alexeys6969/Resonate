using Resonate.Context;
using Resonate.Model;
using Resonate.Model.SupplyClasses;
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
using SupplyModel = Resonate.Model.SupplyClasses.Supply;

namespace Resonate.Pages.Supply
{
    public partial class Add : Page
    {
        private SupplyModel supply;

        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));
        private readonly Regex _dateRegex = new Regex(@"^\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}$", RegexOptions.Compiled);
        private readonly List<SupplyItemData> _cartItems = new List<SupplyItemData>();
        private readonly List<Product> _availableProducts = new List<Product>();
        private readonly List<Supplier> _availableSuppliers = new List<Supplier>();

        public Add(SupplyModel _supply = null)
        {
            InitializeComponent();
            supply = _supply;
            Loaded += Add_Loaded;
        }

        private async void Add_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrentEmployee();
            await LoadSuppliers();
            await LoadProducts();

            if (supply != null)
            {
                if (supply.Supply_Items == null || !supply.Supply_Items.Any())
                {
                    var fullSupply = await SupplyContext.GetSupplyById(supply.Id);
                    if (fullSupply != null)
                        supply = fullSupply;
                }

                LoadSupplyData();
                FormTitle.Text = "✏️ Редактирование поставки";
            }
            else
            {
                DateTimeSale.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }

            AnimateFormEntrance();
            Code.Focus();

            if (string.IsNullOrWhiteSpace(Code.Text))
                Code.Text = $"SUPP-{DateTime.Now:yyyyMMdd-HHmm}";
        }

        private async Task LoadCurrentEmployee()
        {
            try
            {
                var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
                if (employee != null)
                    SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private async Task LoadSuppliers()
        {
            try
            {
                _availableSuppliers.Clear();
                _availableSuppliers.AddRange((await SupplierContext.GetSuppliers() ?? new List<Supplier>()).OrderBy(x => x.Name));

                Supplier.ItemsSource = null;
                Supplier.DisplayMemberPath = "Name";
                Supplier.SelectedValuePath = "Id";
                Supplier.ItemsSource = _availableSuppliers.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки поставщиков: {ex.Message}");
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

        private void Supplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            if (sender == Supplier)
            {
                SupplierError.Visibility = Visibility.Collapsed;
                return;
            }

            if (sender == DateTimeSale)
                DateError.Visibility = Visibility.Collapsed;
        }

        private void LoadSupplyData()
        {
            Code.Text = string.IsNullOrWhiteSpace(supply.Code) ? $"SUPP-{supply.Id}" : supply.Code;
            DateTimeSale.Text = supply.Supply_Date.ToString("dd.MM.yyyy HH:mm");
            AddEdit.Content = "💾 Сохранить изменения";

            var selectedSupplier = _availableSuppliers.FirstOrDefault(x => x.Id == supply.Supplier_id);
            if (selectedSupplier != null)
                Supplier.SelectedItem = selectedSupplier;

            LoadSupplyItems();
        }

        private void LoadSupplyItems()
        {
            NewProductParent.Children.Clear();

            if (supply == null || supply.Supply_Items == null || !supply.Supply_Items.Any())
            {
                RecalculateTotal();
                return;
            }

            foreach (var supplyItem in supply.Supply_Items)
            {
                var newItem = CreateProductItem();
                var product = _availableProducts.FirstOrDefault(x => x.Id == supplyItem.Product_id)
                    ?? new Product
                    {
                        Id = supplyItem.Product_id,
                        Name = supplyItem.Product != null ? supplyItem.Product.Name : $"Товар #{supplyItem.Product_id}",
                        Price = supplyItem.Purchase_Price
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
                newItem.SetItem(supplyItem.Id, product, supplyItem.Quantity, supplyItem.Purchase_Price);
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
            var selectedSupplier = Supplier.SelectedItem as Supplier;
            DateTime supplyDate = DateTime.Now;

            if (string.IsNullOrWhiteSpace(Code.Text) || Code.Text.Length < 5)
            {
                ShowFieldError(CodeBorder, CodeError, "Введите корректный код поставки");
                isValid = false;
            }

            if (selectedSupplier == null)
            {
                ShowFieldError(SupplierBorder, SupplierError, "Выберите поставщика");
                isValid = false;
            }

            if (!_dateRegex.IsMatch(DateTimeSale.Text ?? string.Empty) ||
                !DateTime.TryParseExact(DateTimeSale.Text, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out supplyDate))
            {
                ShowFieldError(DateBorder, DateError, "Дата в формате: дд.мм.гггг чч:мм");
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
            AddEdit.Content = "⏳ Оформление...";

            try
            {
                if (supply != null)
                {
                    var updateRequest = BuildUpdateRequest(selectedSupplier, supplyDate);
                    bool updated = await SupplyContext.UpdateSupply(supply.Id, updateRequest);
                    if (!updated)
                        throw new Exception("Сервер не подтвердил обновление поставки.");

                    new InfoWindow("Поставка обновлена").Show();
                }
                else
                {
                    var createRequest = new CreateSupplyRequest
                    {
                        supplier_id = selectedSupplier.Id,
                        items = _cartItems.Select(item => new SupplyItemRequest
                        {
                            product_id = item.ProductId,
                            quantity = item.Quantity
                        }).ToList()
                    };

                    var createdSupply = await SupplyContext.CreateSupply(createRequest);
                    if (createdSupply == null)
                        throw new Exception("Сервер вернул пустой ответ.");

                    new InfoWindow("Поставка создана").Show();
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

        private UpdateSupplyFullRequest BuildUpdateRequest(Supplier selectedSupplier, DateTime supplyDate)
        {
            var request = new UpdateSupplyFullRequest
            {
                Supply = new UpdateSupplyInfoRequest
                {
                    Supplier_id = selectedSupplier.Id,
                    Supply_Date = supplyDate
                },
                Items = new List<UpdateSupplyItemFullRequest>()
            };

            foreach (var item in _cartItems)
            {
                request.Items.Add(new UpdateSupplyItemFullRequest
                {
                    Id = item.ItemId,
                    Product_id = item.ProductId,
                    Quantity = item.Quantity,
                    Purchase_Price = item.PurchasePrice,
                    Action = item.ItemId > 0 ? "update" : "add"
                });
            }

            if (supply != null && supply.Supply_Items != null)
            {
                var existingIds = new HashSet<int>(_cartItems.Where(x => x.ItemId > 0).Select(x => x.ItemId));
                foreach (var item in supply.Supply_Items.Where(x => !existingIds.Contains(x.Id)))
                {
                    request.Items.Add(new UpdateSupplyItemFullRequest
                    {
                        Id = item.Id,
                        Product_id = item.Product_id,
                        Quantity = item.Quantity,
                        Purchase_Price = item.Purchase_Price,
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
                    MainWindow.init.frame.Navigate(new Pages.Supply.Main());
                });
            });
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }
    }
}

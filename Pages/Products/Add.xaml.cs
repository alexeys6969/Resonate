using Resonate.Context;
using Resonate.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Products
{
    public partial class Add : Page
    {
        private Model.Product product;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _errorBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));

        // Регулярки для валидации числовых полей
        private readonly Regex _priceRegex = new Regex(@"^\d*\.?\d{0,2}$", RegexOptions.Compiled);
        private readonly Regex _intRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

        public Add(Model.Product _product = null)
        {
            InitializeComponent();
            product = _product;
            Loaded += Add_Loaded;
        }

        private async void Add_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProductsDataInField(product);
            await LoadCurrentEmployees();

            if (product != null)
            {
                FormTitle.Text = "Редактирование товара";
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
            if (sender == Name) { NameError.Visibility = Visibility.Collapsed; return; }
            if (sender == Article) { ArticleError.Visibility = Visibility.Collapsed; return; }
            if (sender == Description) return;
            if (sender == Category) { CategoryError.Visibility = Visibility.Collapsed; return; }
            if (sender == Price) { PriceError.Visibility = Visibility.Collapsed; return; }
            if (sender == Stock) { StockError.Visibility = Visibility.Collapsed; return; }
        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e) => ClearFieldError(sender);
        private void Article_TextChanged(object sender, TextChangedEventArgs e) => ClearFieldError(sender);
        private void Price_TextChanged(object sender, TextChangedEventArgs e) => ClearFieldError(sender);
        private void Stock_TextChanged(object sender, TextChangedEventArgs e) => ClearFieldError(sender);
        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e) => ClearFieldError(sender);

        // 🔹 Только цифры и точка для цены
        private void Price_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string text = (sender as TextBox)?.Text + e.Text;
            e.Handled = !_priceRegex.IsMatch(text);
        }

        // 🔹 Только цифры для остатка
        private void Stock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_intRegex.IsMatch(e.Text);
        }

        private async void EditInfo(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            // Валидация названия
            if (string.IsNullOrWhiteSpace(Name.Text) || Name.Text.Length < 2)
            {
                ShowFieldError(NameBorder, NameError, "Введите название (мин. 2 символа)");
                Name.Focus();
                isValid = false;
            }

            // Валидация артикула
            if (string.IsNullOrWhiteSpace(Article.Text) || Article.Text.Length < 3)
            {
                ShowFieldError(ArticleBorder, ArticleError, "Введите артикул (мин. 3 символа)");
                isValid = false;
            }

            // Валидация описания
            if (string.IsNullOrWhiteSpace(Description.Text))
            {
                ShowFieldError(DescriptionBorder, null, "Введите описание товара");
                isValid = false;
            }

            // Валидация категории
            if (Category.SelectedItem == null)
            {
                ShowFieldError(CategoryBorder, CategoryError, "Выберите категорию");
                isValid = false;
            }

            // Валидация цены
            if (!decimal.TryParse(Price.Text, out decimal price) || price <= 0)
            {
                ShowFieldError(PriceBorder, PriceError, "Введите корректную цену (> 0)");
                isValid = false;
            }

            // Валидация остатка
            if (!int.TryParse(Stock.Text, out int stock) || stock < 0)
            {
                ShowFieldError(StockBorder, StockError, "Введите корректный остаток (≥ 0)");
                isValid = false;
            }

            if (!isValid) return;

            // Блокировка кнопки
            AddEdit.IsEnabled = false;
            var originalContent = AddEdit.Content;
            AddEdit.Content = product != null ? "💾 Сохранение..." : "✨ Создание...";

            try
            {
                var selectedCategory = Category.SelectedItem as Model.Category;
                int categoryId = selectedCategory?.Id ?? 0;

                if (product != null)
                {
                    // 🔹 РЕДАКТИРОВАНИЕ
                    var updatedProduct = new Model.Product
                    {
                        Id = product.Id,
                        Article = Article.Text.Trim(),
                        Name = Name.Text.Trim(),
                        Description = Description.Text?.Trim(),
                        Category_Id = categoryId,
                        Price = price,
                        Stock_Quantity = stock
                    };

                    bool result = await ProductContext.UpdateProduct(updatedProduct.Id, updatedProduct);

                    if (result)
                    {
                        ShowSuccess("Товар обновлён");
                        NavigateBack();
                    }
                    else
                    {
                        ShowError("Не удалось обновить товар");
                    }
                }
                else
                {
                    // 🔹 СОЗДАНИЕ
                    var newProduct = new Model.Product
                    {
                        Article = Article.Text.Trim(),
                        Name = Name.Text.Trim(),
                        Description = Description.Text?.Trim(),
                        Category_Id = categoryId,
                        Price = price,
                        Stock_Quantity = stock
                    };

                    Model.Product createdProduct = await ProductContext.CreateProduct(newProduct);

                    if (createdProduct != null)
                    {
                        ShowSuccess($"Товар \"{createdProduct.Name}\" создан");
                        NavigateBack();
                    }
                    else
                    {
                        ShowError("Не удалось создать товар");
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
                MainWindow.init.frame.Navigate(new Pages.Products.Main());
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
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
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
                    MainWindow.init.frame.Navigate(new Pages.Products.Main());
                });
            });
        }

        public async Task LoadProductsDataInField(Model.Product prod)
        {
            try
            {
                var categories = await CategoryContext.GetCategories();

                Category.DisplayMemberPath = "Name";
                Category.SelectedValuePath = "Id";
                Category.ItemsSource = categories;

                if (prod != null)
                {
                    var fullProduct = await ProductContext.GetProductById(prod.Id) ?? prod;

                    Article.Text = fullProduct.Article;
                    Name.Text = fullProduct.Name;
                    Description.Text = fullProduct.Description;

                    int categoryId = fullProduct.Category_Id;
                    if (categoryId <= 0 && fullProduct.Category != null)
                        categoryId = fullProduct.Category.Id;

                    if (categoryId > 0)
                        Category.SelectedValue = categoryId;
                    else
                        Category.SelectedItem = null;

                    Price.Text = fullProduct.Price.ToString("0.00", CultureInfo.InvariantCulture);
                    Stock.Text = fullProduct.Stock_Quantity.ToString();

                    AddEdit.Content = "💾 Сохранить изменения";
                    FormTitle.Text = "Редактирование товара";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки категорий: {ex.Message}");
            }
        }
    }
}

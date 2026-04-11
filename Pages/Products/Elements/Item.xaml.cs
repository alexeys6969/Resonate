using Resonate.Context;
using Resonate.Model;
using Resonate.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Products.Elements
{
    public partial class Item : UserControl
    {
        private Product product;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(58, 58, 58));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly SolidColorBrush _lowStockBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
        private readonly SolidColorBrush _outOfStockBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82));

        public Item(Product _product)
        {
            InitializeComponent();
            product = _product;
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

        private async void LoadItem()
        {
            try
            {
                // Загружаем актуальные данные с сервера
                var currentProduct = await ProductContext.GetProductById(product.Id);

                if (currentProduct != null)
                {
                    product = currentProduct;
                }

                // Заполняем поля с защитой от null
                Name.Text = product?.Name ?? "Без названия";
                Article.Text = $"Артикул: {product?.Article ?? "—"}";
                Category.Text = $"Категория: {product?.Category?.Name ?? "Не указана"}";

                // Форматируем цену с разделителями тысяч
                if (product?.Price != null)
                    Price.Text = $"{product.Price:N2}₽";
                else
                    Price.Text = "0.00₽";

                // Описание с обрезкой если слишком длинное
                string desc = product?.Description ?? "Описание отсутствует";
                Description.Text = $"Описание: {(desc.Length > 80 ? desc.Substring(0, 80) + "..." : desc)}";

                // Остаток с цветовой индикацией
                int stock = product?.Stock_Quantity ?? 0;
                Stock.Text = $"📦 Остаток: {stock}";

                // Цветовая индикация остатка
                if (stock == 0)
                {
                    Stock.Foreground = _outOfStockBrush;
                    Stock.FontWeight = FontWeights.Bold;
                }
                else if (stock <= 5)
                {
                    Stock.Foreground = _lowStockBrush;
                    Stock.FontWeight = FontWeights.Medium;
                }
                else
                {
                    Stock.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    Stock.FontWeight = FontWeights.Normal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки товара: {ex.Message}");
                Name.Text = product?.Name ?? "Ошибка загрузки";
                Price.Text = "—";
                Stock.Text = "📦 Остаток: ?";
            }
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Products.Add(product));
                });
            });
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            // Показываем диалог подтверждения
            var dialog = new DialogWindow($"Вы точно хотите удалить товар \"{product?.Name}\"?");
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                _ = PerformDelete();
            }
        }

        private async Task PerformDelete()
        {
            try
            {
                // Анимация исчезновения перед удалением
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += async (s, args) =>
                {
                    bool result = await ProductContext.DeleteProduct(product?.Id ?? 0);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (result)
                        {
                            var info = new InfoWindow($"Товар \"{product?.Name}\" успешно удалён");
                            info.Show();

                            // Перезагрузка списка товаров
                            MainWindow.init.frame.Navigate(new Pages.Products.Main());
                        }
                        else
                        {
                            var info = new InfoWindow($"При удалении товара \"{product?.Name}\" возникла ошибка. Возможно он участвует в поставке или продаже.");
                            info.Show();

                            // Возвращаем видимость при ошибке
                            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                            this.BeginAnimation(OpacityProperty, fadeIn);
                        }
                    });
                };

                this.BeginAnimation(OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                var info = new InfoWindow($"Возникла ошибка: {ex.Message}");
                info.Show();
            }
        }
    }
}
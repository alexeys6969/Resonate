using Resonate.Model;
using Resonate.Model.SaleClasses;
using Resonate.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Sales.Elements
{
    public partial class Item : UserControl
    {
        private Sale sale;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(58, 58, 58));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));

        public Item()
        {
            InitializeComponent();
            Loaded += Item_Loaded;
        }

        /// <summary>
        /// Загружает данные продажи в элемент
        /// </summary>
        public void LoadData(Sale _sale)
        {
            sale = _sale;
            RenderSale();
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
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

        /// <summary>
        /// Отображает данные продажи в интерфейсе
        /// </summary>
        private void RenderSale()
        {
            if (sale == null) return;

            // Код продажи
            SaleCode.Text = $"🧾 {sale.Code ?? $"SALE-{sale.Id}"}";

            // Кассир
            string cashierName = "—";
            if (sale.Employee != null && !string.IsNullOrWhiteSpace(sale.Employee.Full_Name))
                cashierName = sale.Employee.Full_Name;
            else if (sale.Employee_id > 0)
                cashierName = $"ID: {sale.Employee_id}";
            Cashier.Text = $"👤 Кассир: {cashierName}";

            // Дата
            if (sale.Sale_Date != null && sale.Sale_Date != default(DateTime))
                SaleDate.Text = $"📅 {sale.Sale_Date:dd MMMM yyyy HH:mm}";
            else
                SaleDate.Text = "📅 Дата не указана";

            // Сумма с форматированием
            TotalAmount.Text = $"{sale.Total_Amount:N2} ₽";

            // Список товаров
            if (sale.Sale_Items != null && sale.Sale_Items.Count > 0)
            {
                var displayItems = new System.Collections.Generic.List<object>();
                foreach (var item in sale.Sale_Items)
                {
                    string productName = item.Product?.Name ?? $"Товар #{item.Product_id}";
                    decimal lineTotal = item.Quantity * item.Price_At_Sale;

                    displayItems.Add(new
                    {
                        Name = $"{productName} × {item.Quantity}",
                        LineTotal = lineTotal
                    });
                }
                ProductsList.ItemsSource = displayItems;
            }
            else
            {
                ProductsList.ItemsSource = new System.Collections.Generic.List<object>
                {
                    new { Name = "Список товаров пуст", LineTotal = 0m }
                };
            }
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            if (sale?.Id != null && sale.Id > 0)
            {
                // TODO: Переход на страницу редактирования
                MessageBox.Show("Редактирование продаж пока недоступно", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            if (sale?.Id == null || sale.Id <= 0) return;

            var dialog = new DialogWindow($"Вы точно хотите удалить продажу #{sale.Code ?? sale.Id.ToString()}?");
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
                // Анимация исчезновения
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += async (s, args) =>
                {
                    bool result = await Resonate.Context.SaleContext.DeleteSale(sale.Id);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (result)
                        {
                            var info = new InfoWindow($"Продажа #{sale.Code ?? sale.Id.ToString()} удалена");
                            info.Show();

                            // Перезагрузка списка
                            // MainWindow.init.frame.Navigate(new Pages.Sales.Main());
                        }
                        else
                        {
                            var info = new InfoWindow($"Не удалось удалить продажу");
                            info.Show();

                            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                            this.BeginAnimation(OpacityProperty, fadeIn);
                        }
                    });
                };

                this.BeginAnimation(OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                var info = new InfoWindow($"Ошибка: {ex.Message}");
                info.Show();
            }
        }
    }
}
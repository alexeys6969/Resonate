using Resonate.Model;
using Resonate.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Supply.Elements
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        private Supply supply;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(58, 58, 58));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));

        public Item()
        {
            InitializeComponent();
            Loaded += Item_Loaded;
        }

        /// <summary>
        /// Загружает данные поставки в элемент
        /// </summary>
        /// <param name="_supply">Объект поставки для отображения</param>
        public void LoadData(Supply _supply)
        {
            supply = _supply;
            RenderSupply();
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
        /// Отображает данные поставки в интерфейсе
        /// </summary>
        private void RenderSupply()
        {
            if (supply == null) return;

            // Код поставки
            SupplyCode.Text = $"📥 {supply.Code ?? $"SUPP-{supply.Id}"}";

            // Дата
            if (supply.Supply_Date != null && supply.Supply_Date != default(DateTime))
                SupplyDate.Text = $"📅 {supply.Supply_Date:dd MMMM yyyy HH:mm}";
            else
                SupplyDate.Text = "📅 Дата не указана";

            // Сумма с форматированием
            TotalAmount.Text = $"{supply.Total_Amount:N2} ₽";

            // Список товаров
            if (supply.Supply_Items != null && supply.Supply_Items.Count > 0)
            {
                var displayItems = new List<object>();
                foreach (var item in supply.Supply_Items)
                {
                    string productName = item.Product?.Name ?? $"Товар #{item.Product_id}";
                    decimal lineTotal = item.Quantity * item.Purchase_Price;

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
                ProductsList.ItemsSource = new List<object>
                {
                    new { Name = "Список товаров пуст", LineTotal = 0m }
                };
            }
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            if (supply?.Id != null && supply.Id > 0)
            {
                _ = Task.Delay(100).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainWindow.init.frame.Navigate(new Pages.Supply.Add(supply));
                    });
                });
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            if (supply?.Id == null || supply.Id <= 0) return;

            var dialog = new DialogWindow($"Вы точно хотите удалить поставку #{supply.Code ?? supply.Id.ToString()}?");
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
                    // 🔹 Здесь будет вызов API для удаления
                    // bool result = await SupplyContext.DeleteSupply(supply.Id);

                    // Заглушка для демонстрации
                    bool result = true;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (result)
                        {
                            var info = new InfoWindow($"Поставка #{supply.Code ?? supply.Id} удалена");
                            info.Show();

                            // Перезагрузка списка
                            MainWindow.init.frame.Navigate(new Pages.Supply.Main());
                        }
                        else
                        {
                            var info = new InfoWindow($"Не удалось удалить поставку");
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
                var info = new InfoWindow($"Ошибка: {ex.Message}");
                info.Show();
            }
        }
    }
}
using Resonate.Context;
using Resonate.Model.SupplyClasses;
using Resonate.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using SupplyModel = Resonate.Model.SupplyClasses.Supply;

namespace Resonate.Pages.Supply.Elements
{
    public partial class Item : UserControl
    {
        private SupplyModel supply;

        public Item()
        {
            InitializeComponent();
            Loaded += Item_Loaded;
        }

        public void LoadData(SupplyModel _supply)
        {
            supply = _supply;
            RenderSupply();
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateEntrance();
        }

        private void AnimateEntrance()
        {
            Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, fadeIn);
        }

        private void AnimateButtonClick(Button button)
        {
            var scaleDown = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            button.RenderTransform = new System.Windows.Media.ScaleTransform(1, 1);
            button.RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleDown);
            button.RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleDown);
        }

        private void RenderSupply()
        {
            if (supply == null)
                return;

            SupplyCode.Text = $"📥 {supply.Code ?? $"SUPP-{supply.Id}"}";
            SupplyDate.Text = supply.Supply_Date != default(DateTime)
                ? $"📅 {supply.Supply_Date:dd MMMM yyyy HH:mm}"
                : "📅 Дата не указана";
            SupplierName.Text = $"Поставщик: {supply.Supplier?.Name ?? $"#{supply.Supplier_id}"}";
            TotalAmount.Text = $"{supply.Total_Amount:N2} ₽";

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

        private async void Edit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                AnimateButtonClick(btn);
                btn.IsEnabled = false;
            }

            try
            {
                if (supply == null || supply.Id <= 0)
                    return;

                var fullSupply = await SupplyContext.GetSupplyById(supply.Id) ?? supply;
                MainWindow.init.frame.Navigate(new Pages.Supply.Add(fullSupply));
            }
            catch (Exception ex)
            {
                new InfoWindow($"Ошибка при загрузке поставки: {ex.Message}").Show();
            }
            finally
            {
                if (sender is Button sourceButton)
                    sourceButton.IsEnabled = true;
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            if (supply == null || supply.Id <= 0)
                return;

            var dialog = new DialogWindow($"Вы точно хотите удалить поставку #{supply.Code ?? supply.Id.ToString()}?");
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
                _ = PerformDelete();
        }

        private Task PerformDelete()
        {
            try
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += async (s, args) =>
                {
                    bool result = await SupplyContext.DeleteSupply(supply.Id);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (result)
                        {
                            new InfoWindow($"Поставка #{supply.Code ?? supply.Id.ToString()} удалена").Show();
                            MainWindow.init.frame.Navigate(new Pages.Supply.Main());
                        }
                        else
                        {
                            new InfoWindow("Не удалось удалить поставку").Show();

                            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                            BeginAnimation(OpacityProperty, fadeIn);
                        }
                    });
                };

                BeginAnimation(OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                new InfoWindow($"Ошибка: {ex.Message}").Show();
            }

            return Task.CompletedTask;
        }
    }
}

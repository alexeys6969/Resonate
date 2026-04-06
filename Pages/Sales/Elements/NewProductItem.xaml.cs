using Resonate.Model;
using Resonate.Windows;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Sales.Elements
{
    public partial class NewProductItem : UserControl
    {
        private Pages.Sales.Add add;
        private readonly Regex _intRegex = new Regex(@"^\d*$", RegexOptions.Compiled);

        // Событие для уведомления об изменении строки (для пересчёта итога)
        public event EventHandler<ItemChangedEventArgs> ItemChanged;

        public NewProductItem(Pages.Sales.Add add)
        {
            InitializeComponent();
            this.add = add;
            Loaded += NewProductItem_Loaded;
        }

        private void NewProductItem_Loaded(object sender, RoutedEventArgs e)
        {
            // Анимация появления
            var storyboard = (Storyboard)FindResource("SlideIn");
            storyboard?.Begin(this);

            // Фокус на ComboBox после появления
            Product.Focus();
        }

        /// <summary>
        /// Только цифры для поля количества
        /// </summary>
        private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_intRegex.IsMatch(e.Text);
        }

        /// <summary>
        /// Пересчёт суммы строки при изменении количества
        /// </summary>
        private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateLineTotal();
        }

        /// <summary>
        /// Пересчёт при выборе другого товара
        /// </summary>
        private void Product_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateLineTotal();
        }

        /// <summary>
        /// Пересчитывает сумму строки: Цена × Количество
        /// </summary>
        private void RecalculateLineTotal()
        {
            if (Product.SelectedItem is Model.Product product &&
                int.TryParse(Quantity.Text, out int quantity) &&
                quantity > 0)
            {
                decimal lineTotal = product.Price * quantity;
                LineTotal.Text = $"{lineTotal:N2} ₽";

                // Уведомляем родительскую страницу об изменении
                ItemChanged?.Invoke(this, new ItemChangedEventArgs
                {
                    Product = product,
                    Quantity = quantity,
                    LineTotal = lineTotal
                });
            }
            else
            {
                LineTotal.Text = "0.00 ₽";
                ItemChanged?.Invoke(this, new ItemChangedEventArgs
                {
                    Product = Product.SelectedItem as Model.Product,
                    Quantity = 0,
                    LineTotal = 0
                });
            }
        }

        /// <summary>
        /// Увеличить количество на 1
        /// </summary>
        private void IncrementQuantity(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Quantity.Text, out int qty))
            {
                Quantity.Text = (qty + 1).ToString();
                Quantity.CaretIndex = Quantity.Text.Length;
            }
        }

        /// <summary>
        /// Уменьшить количество на 1 (минимум 1)
        /// </summary>
        private void DecrementQuantity(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Quantity.Text, out int qty) && qty > 1)
            {
                Quantity.Text = (qty - 1).ToString();
                Quantity.CaretIndex = Quantity.Text.Length;
            }
        }

        /// <summary>
        /// Удаление товара из чека с анимацией
        /// </summary>
        private void DeleteProduct(object sender, RoutedEventArgs e)
        {
            // Анимация исчезновения
            var storyboard = (Storyboard)FindResource("SlideOut");

            if (storyboard != null)
            {
                storyboard.Completed += (s, args) =>
                {
                    add?.NewProductParent?.Children.Remove(this);
                };
                storyboard.Begin(this);
            }
            else
            {
                // Если анимация не найдена — удаляем сразу
                add?.NewProductParent?.Children.Remove(this);
            }
        }

        /// <summary>
        /// Получает данные выбранного товара
        /// </summary>
        public SaleItemData GetSaleItemData()
        {
            if (Product.SelectedItem is Model.Product product &&
                int.TryParse(Quantity.Text, out int quantity) &&
                quantity > 0)
            {
                return new SaleItemData
                {
                    Product = product,
                    ProductId = product.Id,
                    Quantity = quantity,
                    Price = product.Price
                };
            }
            return null;
        }

        /// <summary>
        /// Устанавливает товар и количество программно
        /// </summary>
        public void SetItem(Model.Product product, int quantity = 1)
        {
            Product.SelectedItem = product;
            Quantity.Text = quantity.ToString();
            RecalculateLineTotal();
        }
    }

    /// <summary>
    /// Данные для передачи в родительский контрол
    /// </summary>
    public class ItemChangedEventArgs : EventArgs
    {
        public Model.Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// Данные строки продажи для сохранения
    /// </summary>
    public class SaleItemData
    {
        public int ProductId { get; set; }
        public Model.Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
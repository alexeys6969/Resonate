using Resonate.Model;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Supply.Elements
{
    public partial class NewProductItem : UserControl
    {
        private Pages.Supply.Add add;
        private readonly Regex _intRegex = new Regex(@"^\d*$", RegexOptions.Compiled);
        public event EventHandler<ItemChangedEventArgs> ItemChanged;

        public NewProductItem(Pages.Supply.Add add)
        {
            InitializeComponent();
            this.add = add;
            Loaded += NewProductItem_Loaded;
        }

        private void NewProductItem_Loaded(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)FindResource("SlideIn");
            sb?.Begin(this);
            Product.Focus();
        }

        private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !_intRegex.IsMatch(e.Text);

        private void Quantity_TextChanged(object sender, TextChangedEventArgs e) => RecalculateLineTotal();
        private void Product_SelectionChanged(object sender, SelectionChangedEventArgs e) => RecalculateLineTotal();

        private void RecalculateLineTotal()
        {
            if (Product.SelectedItem is Product product && int.TryParse(Quantity.Text, out int qty) && qty > 0)
            {
                decimal total = product.Price * qty;
                LineTotal.Text = $"{total:N2} ₽";
                ItemChanged?.Invoke(this, new ItemChangedEventArgs { Product = product, Quantity = qty, LineTotal = total });
            }
            else
            {
                LineTotal.Text = "0.00 ₽";
                ItemChanged?.Invoke(this, new ItemChangedEventArgs { Product = Product.SelectedItem as Product, Quantity = 0, LineTotal = 0 });
            }
        }

        private void IncrementQuantity(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Quantity.Text, out int qty))
            {
                Quantity.Text = (qty + 1).ToString();
                Quantity.CaretIndex = Quantity.Text.Length;
            }
        }

        private void DecrementQuantity(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Quantity.Text, out int qty) && qty > 1)
            {
                Quantity.Text = (qty - 1).ToString();
                Quantity.CaretIndex = Quantity.Text.Length;
            }
        }

        private void DeleteProduct(object sender, RoutedEventArgs e)
        {
            var sb = (Storyboard)FindResource("SlideOut");
            if (sb != null)
            {
                sb.Completed += (s, args) => add?.NewProductParent?.Children.Remove(this);
                sb.Begin(this);
            }
            else add?.NewProductParent?.Children.Remove(this);
        }

        public SupplyItemData GetSupplyItemData()
        {
            if (Product.SelectedItem is Product product && int.TryParse(Quantity.Text, out int qty) && qty > 0)
            {
                return new SupplyItemData
                {
                    Product = product,
                    ProductId = product.Id,
                    Quantity = qty,
                    Price = product.Price,
                    LineTotal = product.Price * qty
                };
            }
            return null;
        }

        public void SetItem(Product product, int quantity = 1)
        {
            Product.SelectedItem = product;
            Quantity.Text = quantity.ToString();
            RecalculateLineTotal();
        }
    }

    public class ItemChangedEventArgs : EventArgs
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class SupplyItemData
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
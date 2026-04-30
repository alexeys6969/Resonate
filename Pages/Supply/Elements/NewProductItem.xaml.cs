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
        private bool _isApplyingState;
        private int _itemId;
        private decimal _purchasePrice;

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
        {
            e.Handled = !_intRegex.IsMatch(e.Text);
        }

        private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecalculateLineTotal();
        }

        private void Product_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isApplyingState && Product.SelectedItem is Product product)
            {
                _itemId = 0;
                _purchasePrice = product.Price;
            }

            RecalculateLineTotal();
        }

        private void RecalculateLineTotal()
        {
            if (Product == null || Quantity == null)
                return;

            if (Product.SelectedItem is Product product && int.TryParse(Quantity.Text, out int qty) && qty > 0)
            {
                decimal unitPrice = _purchasePrice > 0 ? _purchasePrice : product.Price;
                decimal total = unitPrice * qty;

                SetPriceLabels($"{unitPrice:N2} ₽/шт", $"{total:N2} ₽");

                ItemChanged?.Invoke(this, new ItemChangedEventArgs
                {
                    Product = product,
                    Quantity = qty,
                    LineTotal = total
                });
            }
            else
            {
                SetPriceLabels("0.00 ₽/шт", "0.00 ₽");
                ItemChanged?.Invoke(this, new ItemChangedEventArgs
                {
                    Product = Product.SelectedItem as Product,
                    Quantity = 0,
                    LineTotal = 0
                });
            }
        }

        private void SetPriceLabels(string unitPriceText, string lineTotalText)
        {
            if (UnitPrice != null)
                UnitPrice.Text = unitPriceText;

            if (LineTotal != null)
                LineTotal.Text = lineTotalText;
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
                sb.Completed += (s, args) =>
                {
                    if (add?.NewProductParent != null)
                    {
                        add.NewProductParent.Children.Remove(this);
                        add.RefreshTotals();
                    }
                };
                sb.Begin(this);
            }
            else if (add?.NewProductParent != null)
            {
                add.NewProductParent.Children.Remove(this);
                add.RefreshTotals();
            }
        }

        public SupplyItemData GetSupplyItemData()
        {
            if (Product.SelectedItem is Product product && int.TryParse(Quantity.Text, out int qty) && qty > 0)
            {
                return new SupplyItemData
                {
                    ItemId = _itemId,
                    Product = product,
                    ProductId = product.Id,
                    Quantity = qty,
                    PurchasePrice = _purchasePrice > 0 ? _purchasePrice : product.Price
                };
            }

            return null;
        }

        public void SetItem(int itemId, Product product, int quantity, decimal purchasePrice)
        {
            if (product == null)
                return;

            _isApplyingState = true;
            _itemId = itemId;
            _purchasePrice = purchasePrice > 0 ? purchasePrice : product.Price;
            Product.SelectedItem = product;
            Quantity.Text = quantity.ToString();
            _isApplyingState = false;
            RecalculateLineTotal();
        }

        public void FocusProduct()
        {
            Product.Focus();
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
        public int ItemId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal LineTotal => PurchasePrice * Quantity;
    }
}

using Resonate.Context;
using Resonate.Model;
using Resonate.Model.SupplyClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SupplyModel = Resonate.Model.SupplyClasses.Supply;

namespace Resonate.Pages.Supply
{
    public partial class Main : Page
    {
        private List<SupplyModel> _allSupplies = new List<SupplyModel>();
        private List<Supplier> _allSuppliers = new List<Supplier>();

        private readonly FontFamily _interFont = new FontFamily(
            new Uri("pack://application:,,,/Fonts/"),
            "./#Inter");

        public Main()
        {
            InitializeComponent();
            Loaded += Main_Loaded;
        }

        private async void Main_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCurrentEmployee();
            await LoadSupplies();
            await LoadSuppliersForFilter();
        }

        private void AnimateListEntrance()
        {
            if (SupplyParent?.Children == null)
                return;

            int delay = 0;

            foreach (UIElement child in SupplyParent.Children)
            {
                child.Opacity = 0;
                child.RenderTransform = new TranslateTransform(-20, 0);

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                    BeginTime = TimeSpan.FromMilliseconds(delay)
                };

                var slideIn = new DoubleAnimation(-20, 0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                    BeginTime = TimeSpan.FromMilliseconds(delay)
                };

                child.BeginAnimation(OpacityProperty, fadeIn);

                if (child.RenderTransform != null)
                    child.RenderTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);

                delay += 60;
            }
        }

        public async Task LoadSupplies()
        {
            try
            {
                _allSupplies = await SupplyContext.GetSupplies() ?? new List<SupplyModel>();
                RenderSupplies(_allSupplies.OrderByDescending(x => x.Supply_Date), true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                ShowErrorState("Не удалось загрузить поставки");
            }
        }

        private async Task LoadSuppliersForFilter()
        {
            try
            {
                _allSuppliers = await SupplierContext.GetSuppliers() ?? new List<Supplier>();

                SupplierFilter.Items.Clear();
                SupplierFilter.Items.Add(new ComboBoxItem { Content = "Все поставщики", Tag = 0 });

                foreach (var supplier in _allSuppliers.OrderBy(s => s.Name))
                {
                    SupplierFilter.Items.Add(new ComboBoxItem
                    {
                        Content = supplier.Name,
                        Tag = supplier.Id
                    });
                }

                SupplierFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox != null)
                FilterSupplies();
        }

        private void SupplierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterSupplies();
        }

        private void FilterSupplies()
        {
            if (SearchBox == null || SupplierFilter == null)
                return;

            string query = SearchBox.Text?.Trim().ToLower() ?? "";

            int? supplierId = null;
            if (SupplierFilter.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is int id && id > 0)
            {
                supplierId = id;
            }

            var filtered = _allSupplies.Where(s =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(query) ||
                    s.Code?.ToLower().Contains(query) == true ||
                    s.Supplier?.Name?.ToLower().Contains(query) == true ||
                    s.Supply_Items?.Any(i => i.Product?.Name?.ToLower().Contains(query) == true) == true;

                bool matchesSupplier = !supplierId.HasValue || s.Supplier_id == supplierId.Value;
                return matchesSearch && matchesSupplier;
            });

            RenderSupplies(filtered.OrderByDescending(x => x.Supply_Date), true);
        }

        private void RenderSupplies(IEnumerable<SupplyModel> supplies, bool animate)
        {
            SupplyParent.Children.Clear();

            var renderedSupplies = supplies.ToList();
            if (!renderedSupplies.Any())
            {
                ShowEmptyState();
                return;
            }

            foreach (var supply in renderedSupplies)
            {
                var item = new Elements.Item();
                item.LoadData(supply);
                SupplyParent.Children.Add(item);
            }

            if (animate)
                AnimateListEntrance();
        }

        private void ShowEmptyState()
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                BorderThickness = new Thickness(1, 1, 1, 1),
                Padding = new Thickness(30, 40, 30, 40),
                Margin = new Thickness(0, 10, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stack.Children.Add(new TextBlock
            {
                Text = "📥",
                FontSize = 48,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Поставки не найдены",
                FontFamily = _interFont,
                FontSize = 18,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Margin = new Thickness(0, 15, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Нажмите «Новая поставка»",
                FontFamily = _interFont,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            card.Child = stack;
            SupplyParent.Children.Add(card);
        }

        private void ShowErrorState(string message)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(50, 30, 30)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82)),
                BorderThickness = new Thickness(1, 1, 1, 1),
                Padding = new Thickness(25, 30, 25, 30),
                Margin = new Thickness(0, 10, 0, 10)
            };

            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = "⚠️ Ошибка",
                FontFamily = _interFont,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 120, 120))
            });

            stack.Children.Add(new TextBlock
            {
                Text = message,
                FontFamily = _interFont,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });

            card.Child = stack;
            SupplyParent.Children.Add(card);
        }

        public async Task LoadCurrentEmployee()
        {
            try
            {
                var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);

                if (employee != null)
                    SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Supply.Add());
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow.init.frame.Navigate(new Pages.Main());
        }
    }
}

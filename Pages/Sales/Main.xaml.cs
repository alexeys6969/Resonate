using Resonate.Context;
using Resonate.Model;
using Resonate.Model.SaleClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Sales
{
    public partial class Main : Page
    {
        private List<Sale> _allSales = new List<Sale>();
        private List<Model.Employees> _allCashiers = new List<Model.Employees>();
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(68, 68, 68));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        private readonly FontFamily _interFont = new FontFamily(new Uri("pack://application:,,,/Fonts/"), "./#Inter");

        public Main()
        {
            InitializeComponent();
            Loaded += Main_Loaded;
        }

        private async void Main_Loaded(object sender, RoutedEventArgs e)
        {
            // ✅ Сначала загружаем данные, ПОТОМ подписываемся на события
            await LoadCurrentEmployee();
            await LoadSales();
            await LoadCashiersForFilter();

            // Анимацию запускаем в конце
            AnimateListEntrance();
        }

        /// <summary>
        /// Анимация появления элементов списка
        /// </summary>
        private void AnimateListEntrance()
        {
            if (SaleParent?.Children == null) return;

            int delay = 0;
            foreach (UIElement child in SaleParent.Children)
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

        /// <summary>
        /// Загружает список продаж
        /// </summary>
        public async Task LoadSales()
        {
            try
            {
                SaleParent.Children.Clear();
                _allSales = await SaleContext.GetSales();

                if (_allSales == null || _allSales.Count == 0)
                {
                    ShowEmptyState();
                    return;
                }

                // Сортировка: новые сверху
                var sorted = _allSales.OrderByDescending(s => s.Sale_Date).ToList();
                DisplaySales(sorted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки продаж: {ex.Message}");
                ShowErrorState("Не удалось загрузить историю продаж");
            }
        }

        /// <summary>
        /// Отображает продажи в списке
        /// </summary>
        private void DisplaySales(List<Sale> sales)
        {
            SaleParent.Children.Clear();

            foreach (var sale in sales)
            {
                var saleElement = new Elements.Item();
                saleElement.LoadData(sale);

                if (saleElement is FrameworkElement fe)
                {
                    fe.MouseEnter += SaleElement_MouseEnter;
                    fe.MouseLeave += SaleElement_MouseLeave;
                }

                SaleParent.Children.Add(saleElement);
            }
        }

        private void SaleElement_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var border = FindChild<Border>(element);
                if (border != null && border.BorderBrush is SolidColorBrush)
                {
                    var colorAnim = new ColorAnimation
                    {
                        To = _focusBorder.Color,
                        Duration = TimeSpan.FromMilliseconds(150)
                    };
                    border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
                }
            }
        }

        private void SaleElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var border = FindChild<Border>(element);
                if (border != null && border.BorderBrush is SolidColorBrush)
                {
                    var colorAnim = new ColorAnimation
                    {
                        To = _defaultBorder.Color,
                        Duration = TimeSpan.FromMilliseconds(150)
                    };
                    border.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
                }
            }
        }

        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        /// <summary>
        /// Загружает кассиров для фильтра
        /// </summary>
        private async Task LoadCashiersForFilter()
        {
            try
            {
                var employees = await EmployeeContext.GetEmployees(MainWindow.Token);
                _allCashiers = employees?.Where(e => e.Position == "Кассир" || e.Position == "Администратор")
                    .OrderBy(e => e.Full_Name).ToList() ?? new List<Model.Employees>();

                // ✅ Проверка на null
                if (CashierFilter == null) return;

                CashierFilter.Items.Clear();
                CashierFilter.Items.Add(new ComboBoxItem { Content = "Все кассиры", Tag = 0 });

                foreach (var cashier in _allCashiers)
                {
                    CashierFilter.Items.Add(new ComboBoxItem
                    {
                        Content = cashier.Full_Name,
                        Tag = cashier.Id
                    });
                }
                CashierFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки кассиров: {ex.Message}");
            }
        }

        /// <summary>
        /// Поиск по продажам
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox == null) return;
            FilterSales();
        }

        private void DateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateFilter == null) return;
            FilterSales();
        }

        private void CashierFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CashierFilter == null) return;
            FilterSales();
        }

        /// <summary>
        /// Применяет все фильтры
        /// </summary>
        private void FilterSales()
        {
            // ✅ Проверка на null для всех ComboBox
            if (SearchBox == null || DateFilter == null || CashierFilter == null)
                return;

            string query = SearchBox.Text?.Trim().ToLower() ?? "";

            // Фильтр по дате
            DateTime? dateFrom = null;
            if (DateFilter.SelectedItem is ComboBoxItem dateItem)
            {
                string tag = dateItem.Tag?.ToString();
                if (tag == "today")
                    dateFrom = DateTime.Today;
                else if (tag == "week")
                    dateFrom = DateTime.Today.AddDays(-7);
                else if (tag == "month")
                    dateFrom = DateTime.Today.AddDays(-30);
            }

            // Фильтр по кассиру
            int? cashierId = null;
            if (CashierFilter.SelectedItem is ComboBoxItem cashierItem)
            {
                if (cashierItem.Tag is int id && id > 0)
                    cashierId = id;
            }

            var filtered = _allSales.Where(s =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(query) ||
                    s.Code?.ToLower().Contains(query) == true ||
                    s.Sale_Items?.Any(i => i.Product?.Name?.ToLower().Contains(query) == true) == true;

                bool matchesDate = !dateFrom.HasValue || s.Sale_Date >= dateFrom.Value;
                bool matchesCashier = !cashierId.HasValue || s.Employee_id == cashierId.Value;

                return matchesSearch && matchesDate && matchesCashier;
            });

            DisplaySales(filtered.OrderByDescending(s => s.Sale_Date).ToList());
        }

        /// <summary>
        /// Показ заглушки, если продаж нет
        /// </summary>
        private void ShowEmptyState()
        {
            var emptyCard = new Border
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
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            stack.Children.Add(new TextBlock
            {
                Text = "🧾",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136))
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Продажи не найдены",
                FontFamily = _interFont,
                FontSize = 18,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Margin = new Thickness(0, 15, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Нажмите «Новая продажа», чтобы создать первую запись",
                FontFamily = _interFont,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            emptyCard.Child = stack;
            SaleParent.Children.Add(emptyCard);
        }

        /// <summary>
        /// Показ ошибки загрузки
        /// </summary>
        private void ShowErrorState(string message)
        {
            var errorCard = new Border
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

            errorCard.Child = stack;
            SaleParent.Children.Add(errorCard);
        }

        /// <summary>
        /// Загружает данные текущего пользователя
        /// </summary>
        public async Task LoadCurrentEmployee()
        {
            try
            {
                var employee = await EmployeeContext.GetCurrentEmployee(MainWindow.Token);
                if (employee != null)
                {
                    SystemUser.Text = $"Система: {employee.GetShortName(employee.Full_Name)}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var scale = new DoubleAnimation(0.98, TimeSpan.FromMilliseconds(100))
                {
                    AutoReverse = true,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                btn.RenderTransform = new ScaleTransform(1, 1);
                btn.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scale);
                btn.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale);
            }

            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Sales.Add());
                });
            });
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            if (sender is Button exitBtn)
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                exitBtn.BeginAnimation(OpacityProperty, fadeOut);
            }

            _ = Task.Delay(200).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Main());
                });
            });
        }
    }
}
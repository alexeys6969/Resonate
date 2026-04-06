using Resonate.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Products
{
    public partial class Main : Page
    {
        private List<Model.Product> _allProducts = new List<Model.Product>();
        private List<Model.Category> _allCategories = new List<Model.Category>();
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
            await LoadCurrentEmployees();
            await LoadProducts();
            AnimateListEntrance();
        }

        /// <summary>
        /// Анимация появления элементов списка
        /// </summary>
        private void AnimateListEntrance()
        {
            if (ProductParent?.Children == null) return;

            int delay = 0;
            foreach (UIElement child in ProductParent.Children)
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

        public async Task LoadProducts()
        {
            try
            {
                ProductParent.Children.Clear();

                // Загружаем товары и категории параллельно
                var productsTask = ProductContext.GetProducts();
                var categoriesTask = CategoryContext.GetCategories();

                await Task.WhenAll(productsTask, categoriesTask);

                _allProducts = productsTask.Result ?? new List<Model.Product>();
                _allCategories = categoriesTask.Result ?? new List<Model.Category>();

                // Заполняем фильтр категорий
                CategoryFilter.Items.Clear();
                CategoryFilter.Items.Add(new ComboBoxItem { Content = "Все категории", Tag = 0 });
                foreach (var cat in _allCategories.OrderBy(c => c.Name))
                {
                    CategoryFilter.Items.Add(new ComboBoxItem
                    {
                        Content = cat.Name,
                        Tag = cat.Id
                    });
                }
                CategoryFilter.SelectedIndex = 0;

                if (_allProducts.Count == 0)
                {
                    ShowEmptyState();
                    return;
                }

                DisplayProducts(_allProducts.OrderBy(p => p.Name).ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки товаров: {ex.Message}");
                ShowErrorState("Не удалось загрузить каталог товаров");
            }
        }

        /// <summary>
        /// Отображает товары в списке
        /// </summary>
        private void DisplayProducts(List<Model.Product> products)
        {
            ProductParent.Children.Clear();

            foreach (var item in products)
            {
                var productElement = new Elements.Item(item);

                if (productElement is FrameworkElement fe)
                {
                    fe.MouseEnter += ProductElement_MouseEnter;
                    fe.MouseLeave += ProductElement_MouseLeave;
                }

                ProductParent.Children.Add(productElement);
            }
        }

        private void ProductElement_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
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

        private void ProductElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
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
        /// Поиск по товарам (фильтрация)
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        /// <summary>
        /// Фильтрация по категории
        /// </summary>
        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProducts();
        }

        /// <summary>
        /// Применяет фильтры поиска и категории
        /// </summary>
        private void FilterProducts()
        {
            string query = SearchBox.Text?.Trim().ToLower() ?? "";
            int categoryId = 0;

            if (CategoryFilter.SelectedItem is ComboBoxItem selected)
            {
                categoryId = selected.Tag is int id ? id : 0;
            }

            var filtered = _allProducts.Where(p =>
            {
                // Фильтр по поиску
                bool matchesSearch = string.IsNullOrWhiteSpace(query) ||
                    p.Name?.ToLower().Contains(query) == true ||
                    p.Article?.ToLower().Contains(query) == true ||
                    p.Description?.ToLower().Contains(query) == true;

                // Фильтр по категории
                bool matchesCategory = categoryId == 0 || p.Category_Id == categoryId;

                return matchesSearch && matchesCategory;
            });

            DisplayProducts(filtered.OrderBy(p => p.Name).ToList());
        }

        /// <summary>
        /// Показ заглушки, если товаров нет
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
                Text = "📦",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136))
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Товары не найдены",
                FontFamily = _interFont,
                FontSize = 18,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Margin = new Thickness(0, 15, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Нажмите «Добавить товар», чтобы создать первую запись",
                FontFamily = _interFont,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            emptyCard.Child = stack;
            ProductParent.Children.Add(emptyCard);
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
            ProductParent.Children.Add(errorCard);
        }

        public async Task LoadCurrentEmployees()
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
                    MainWindow.init.frame.Navigate(new Pages.Products.Add(null));
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
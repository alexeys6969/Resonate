using Resonate.Context;
using Resonate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Pages.Suppliers
{
    public partial class Main : Page
    {
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
            await LoadSuppliers();
            AnimateListEntrance();
        }

        /// <summary>
        /// Анимация появления элементов списка
        /// </summary>
        private void AnimateListEntrance()
        {
            if (SupplierParent?.Children == null)
                return;

            int delay = 0;

            foreach (UIElement child in SupplierParent.Children)
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
                {
                    child.RenderTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);
                }

                delay += 60;
            }
        }

        /// <summary>
        /// Загружает список поставщиков
        /// </summary>
        public async Task LoadSuppliers()
        {
            try
            {
                SupplierParent.Children.Clear();
                _allSuppliers = await SupplierContext.GetSuppliers() ?? new List<Supplier>();

                if (!_allSuppliers.Any())
                {
                    ShowEmptyState();
                    return;
                }

                foreach (var supplier in _allSuppliers.OrderBy(x => x.Name))
                {
                    var item = new Elements.Item();
                    item.LoadData(supplier);
                    SupplierParent.Children.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                ShowErrorState("Не удалось загрузить поставщиков");
            }
        }

        /// <summary>
        /// Поиск по поставщикам
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox == null)
                return;

            var query = SearchBox.Text?.Trim().ToLower() ?? "";

            SupplierParent.Children.Clear();

            var filtered = _allSuppliers
                .Where(x =>
                    x.Name?.ToLower().Contains(query) == true ||
                    x.Contact?.ToLower().Contains(query) == true)
                .OrderBy(x => x.Name);

            foreach (var supplier in filtered)
            {
                var item = new Elements.Item();
                item.LoadData(supplier);
                SupplierParent.Children.Add(item);
            }
        }

        /// <summary>
        /// Показ заглушки, если поставщиков нет
        /// </summary>
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
                Text = "🏢",
                FontSize = 48,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136)),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Поставщики не найдены",
                FontFamily = _interFont,
                FontSize = 18,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Margin = new Thickness(0, 15, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Нажмите «Добавить поставщика»",
                FontFamily = _interFont,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            card.Child = stack;
            SupplierParent.Children.Add(card);
        }

        /// <summary>
        /// Показ ошибки загрузки
        /// </summary>
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
            SupplierParent.Children.Add(card);
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
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
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
                    MainWindow.init.frame.Navigate(new Pages.Suppliers.Add());
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
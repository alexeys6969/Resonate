using Newtonsoft.Json.Linq;
using Resonate.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Resonate.Pages.Category
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
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
            await LoadCategories();
            AnimateListEntrance();
        }
        private void AnimateListEntrance()
        {
            if (CategoryParent?.Children == null) return;

            int delay = 0;
            foreach (UIElement child in CategoryParent.Children)
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
                child.RenderTransform?.BeginAnimation(TranslateTransform.XProperty, slideIn);

                delay += 60;
            }
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

        public async Task LoadCategories()
        {
            try
            {
                CategoryParent.Children.Clear();
                _allCategories = await CategoryContext.GetCategories();

                if (_allCategories == null || _allCategories.Count == 0)
                {
                    ShowEmptyState();
                    return;
                }

                // Сортировка по названию
                var sorted = _allCategories.OrderBy(c => c.Name).ToList();

                foreach (var item in sorted)
                {
                    var categoryElement = new Elements.Item(item);

                    // Добавляем анимацию при наведении на элемент
                    if (categoryElement is FrameworkElement fe)
                    {
                        fe.MouseEnter += CategoryElement_MouseEnter;
                        fe.MouseLeave += CategoryElement_MouseLeave;
                    }

                    CategoryParent.Children.Add(categoryElement);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки категорий: {ex.Message}");
                ShowErrorState("Не удалось загрузить категории");
            }
        }
        private void CategoryElement_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.FindName("RootBorder") is Border border)
            {
                var colorAnim = new ColorAnimation
                {
                    To = _focusBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush?.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
        }

        private void CategoryElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is FrameworkElement element && element.FindName("RootBorder") is Border border)
            {
                var colorAnim = new ColorAnimation
                {
                    To = _defaultBorder.Color,
                    Duration = TimeSpan.FromMilliseconds(150)
                };
                border.BorderBrush?.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
            }
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

            // Навигация с небольшой задержкой
            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Category.Add());
                });
            });
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text?.Trim().ToLower() ?? "";

            // Очищаем и перерисовываем с фильтром
            CategoryParent.Children.Clear();

            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allCategories
                : _allCategories.Where(c => c.Name?.ToLower().Contains(query) == true
                                         || c.Description?.ToLower().Contains(query) == true);

            foreach (var item in filtered.OrderBy(c => c.Name))
            {
                CategoryParent.Children.Add(new Elements.Item(item));
            }
        }
        private void ShowEmptyState()
        {
            var emptyCard = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                BorderThickness = new Thickness(1),
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
                Text = "📁",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(136, 136, 136))
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Категории не найдены",
                FontFamily = _interFont,
                FontSize = 18,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Margin = new Thickness(0, 15, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Нажмите «Добавить категорию», чтобы создать первую",
                FontFamily = _interFont,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            emptyCard.Child = stack;
            CategoryParent.Children.Add(emptyCard);
        }
        private void ShowErrorState(string message)
        {
            var errorCard = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(50, 30, 30)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 82, 82)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
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
            CategoryParent.Children.Add(errorCard);
        }
    }
}

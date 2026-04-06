using Resonate.Context;
using Resonate.Windows;
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

namespace Resonate.Pages.Category.Elements
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        private Model.Category category;
        private readonly SolidColorBrush _defaultBorder = new SolidColorBrush(Color.FromRgb(58, 58, 58));
        private readonly SolidColorBrush _focusBorder = new SolidColorBrush(Color.FromRgb(142, 237, 69));
        public Item(Model.Category _category)
        {
            InitializeComponent();
            category = _category;
            Loaded += Item_Loaded;
        }
        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItem();
            AnimateEntrance();
        }
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


        private void EditInfo(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            _ = Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.init.frame.Navigate(new Pages.Category.Add(category));
                });
            });
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);

            try
            {
                var dialog = new DialogWindow($"Вы точно хотите удалить категорию \"{category.Name}\"?");
                dialog.ShowDialog();

                if (dialog.DialogResult == true)
                {
                    // Анимация удаления (исчезновение)
                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    fadeOut.Completed += async (s, args) =>
                    {
                        bool result = await CategoryContext.DeleteCategory(category.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (result)
                            {
                                var info = new InfoWindow($"Категория \"{category.Name}\" успешно удалена");
                                info.Show();

                                // Перезагрузка списка категорий
                                MainWindow.init.frame.Navigate(new Pages.Category.Main());
                            }
                            else
                            {
                                var info = new InfoWindow($"При удалении категории \"{category.Name}\" возникла ошибка");
                                info.Show();

                                // Возвращаем видимость при ошибке
                                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                                this.BeginAnimation(OpacityProperty, fadeIn);
                            }
                        });
                    };

                    this.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
            catch (Exception ex)
            {
                var info = new InfoWindow($"Возникла ошибка: {ex.Message}");
                info.Show();
            }
        }
        private async void LoadItem()
        {
            if (category != null)
            {
                Name.Text = category.Name ?? "Без названия";
                Description.Text = !string.IsNullOrWhiteSpace(category.Description)
                    ? category.Description
                    : "Описание отсутствует";

                // Если описание слишком длинное, обрезаем
                if (Description.Text.Length > 120)
                {
                    Description.Text = Description.Text.Substring(0, 120) + "...";
                }
            }
        }
    }
}

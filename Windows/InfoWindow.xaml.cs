using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Windows
{
    public partial class InfoWindow : Window
    {
        public InfoWindow(string message)
        {
            InitializeComponent();
            windowInfo.Text = message;
            Loaded += InfoWindow_Loaded;
        }

        private void InfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Анимация появления
            this.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(OpacityProperty, fadeIn);

            // Авто-закрытие через 3 секунды для коротких сообщений (опционально)
            if (windowInfo.Text.Length < 100)
            {
                _ = System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (this.IsLoaded) CloseWithAnimation();
                    });
                });
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                AnimateButtonClick(btn);
            CloseWithAnimation();
        }

        private void AnimateButtonClick(Button button)
        {
            var scale = new DoubleAnimation(0.95, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            button.RenderTransform = new ScaleTransform(1, 1);
            button.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scale);
            button.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale);
        }

        private void CloseWithAnimation()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fadeOut.Completed += (s, args) => Close();
            this.BeginAnimation(OpacityProperty, fadeOut);
        }

        // Закрытие по ESC или Enter
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                CloseWithAnimation();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
    }
}
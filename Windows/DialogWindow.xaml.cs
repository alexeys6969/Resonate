using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resonate.Windows
{
    public partial class DialogWindow : Window
    {
        public new bool? DialogResult { get; private set; }
        private readonly SolidColorBrush _hoverBrush = new SolidColorBrush(Color.FromRgb(142, 237, 69));

        public DialogWindow(string question)
        {
            InitializeComponent();
            questionText.Text = question;
            Loaded += DialogWindow_Loaded;
        }

        private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Анимация появления
            this.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(OpacityProperty, fadeIn);

            // Фокус на кнопку "Нет" по умолчанию (безопасный выбор)
            NoButton.Focus();
        }

        private void Yes(object sender, RoutedEventArgs e)
        {
            AnimateButtonClick(YesButton);
            DialogResult = true;
            CloseWithAnimation();
        }

        private void No(object sender, RoutedEventArgs e)
        {
            AnimateButtonClick(NoButton);
            DialogResult = false;
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

        // Закрытие по ESC
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                CloseWithAnimation();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
    }
}
using System;
using System.Windows;
using System.Windows.Input;

namespace Resonate
{
    public partial class MainWindow : Window
    {
        public static MainWindow init;
        public static string Token;
        public MainWindow()
        {
            InitializeComponent();
            init = this;
            frame.Navigate(new Pages.Authorization());
        }

        // Перетаскивание окна
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Двойной клик — развернуть/восстановить
                Maximize_Click(null, null);
            }
            else
            {
                DragMove();
            }
        }

        // Свернуть
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            var anim = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
            anim.Completed += (s, a) => WindowState = WindowState.Minimized;
            BeginAnimation(OpacityProperty, anim);
        }

        // Развернуть / Восстановить
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        // Плавное закрытие с анимацией (по желанию)
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // Можно добавить плавное затухание
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(180))
            };

            fadeOut.Completed += (s, a) => Close();

            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
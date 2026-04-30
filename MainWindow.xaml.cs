using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

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
            StateChanged += MainWindow_StateChanged;
            Activated += MainWindow_Activated;
            frame.Navigate(new Pages.Authorization());
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Maximize_Click(null, null);
            }
            else
            {
                DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            anim.Completed += (s, a) =>
            {
                BeginAnimation(OpacityProperty, null);
                Opacity = 1;
                WindowState = WindowState.Minimized;
            };

            BeginAnimation(OpacityProperty, anim);
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(180))
            };

            fadeOut.Completed += (s, a) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized && Opacity != 1)
                Opacity = 1;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (Opacity != 1)
                Opacity = 1;
        }
    }
}

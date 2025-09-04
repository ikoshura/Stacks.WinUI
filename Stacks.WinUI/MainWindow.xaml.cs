using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Stacks.WinUI;
using System;

namespace Stacks
{
    public sealed partial class MainWindow : UserControl
    {
        // Event untuk memberitahu App.xaml.cs bahwa widget telah diklik
        public event Action WidgetClicked;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void WidgetButton_Click(object sender, RoutedEventArgs e)
        {
            WidgetClicked?.Invoke();
        }

        private void WidgetButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var flyout = WidgetButton.ContextFlyout;
            if (flyout != null)
            {
                flyout.ShowAt(WidgetButton, new FlyoutShowOptions { Position = e.GetPosition(WidgetButton) });
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Kirim pesan untuk menutup aplikasi
            App.Current.Exit();
        }
    }
}
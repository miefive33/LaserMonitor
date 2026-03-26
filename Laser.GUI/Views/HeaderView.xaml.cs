using System;
using System.Windows;
using System.Windows.Controls;

namespace Laser.GUI.Views
{
    public partial class HeaderView : UserControl
    {
        public event Action? OnUpdateRequested;
        public event Action? OnReloadRequested;

        public HeaderView()
        {
            InitializeComponent();
        }

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            OnUpdateRequested?.Invoke();
        }

        private void ReloadClicked(object sender, RoutedEventArgs e)
        {
            OnReloadRequested?.Invoke();
        }
    }
}
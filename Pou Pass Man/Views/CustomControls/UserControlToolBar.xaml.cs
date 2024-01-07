using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pou_Pass_Man.Views.CustomControls
{
    public partial class UserControlToolBar : UserControl
    {
        public UserControlToolBar()
        {
            InitializeComponent();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (e.LeftButton == MouseButtonState.Pressed) window.DragMove();
        }
    }
}

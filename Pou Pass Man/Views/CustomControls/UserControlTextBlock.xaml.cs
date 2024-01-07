using FontAwesome.WPF;
using System.Windows;
using System.Windows.Controls;

namespace Pou_Pass_Man.Views.CustomControls
{
    public partial class UserControlTextBlock : UserControl
    {
        public event RoutedEventHandler? TextChanged;
        public UserControlTextBlock()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public object HintText
        {
            get { return (object)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(object), typeof(UserControlTextBlock), new PropertyMetadata());

        public object InputText
        {
            get { return (object)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputTextProperty =
            DependencyProperty.Register("InputText", typeof(object), typeof(UserControlTextBlock), new PropertyMetadata());

        public FontAwesomeIcon? Icon { get; set; }

        public static readonly RoutedEvent TextChangedEvent =
            EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UserControlTextBlock));

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(this, e);
            }
            _ = string.IsNullOrEmpty(tbxInput.Text) ? tbkHint.Visibility = Visibility.Visible : tbkHint.Visibility = Visibility.Hidden;
        }
    }
}

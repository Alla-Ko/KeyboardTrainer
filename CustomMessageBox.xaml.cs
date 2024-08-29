using System.Windows;

namespace KeybordTrainer
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public string Message { get; set; }
        public bool IsConfirmed { get; private set; } = false;

        public CustomMessageBox(string message, string okButtonText = "OK", string cancelButtonText = null)
        {
            InitializeComponent();
            DataContext = this;

            Message = message;
            OkButton.Content = okButtonText;

            if (!string.IsNullOrEmpty(cancelButtonText))
            {
                CancelButton.Content = cancelButtonText;
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}

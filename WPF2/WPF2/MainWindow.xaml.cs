using System.Collections.ObjectModel;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //User User = new User();
        public ObservableCollection<Message> Messages { get; private set; } = new ObservableCollection<Message>();
        //ICommand SendMessageCommand = new SendMessageCommand();
        //ICommand NewLineCommand = new NewLineCommand();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Messages.Add(new Message("New message"));
        }

        private void User_Connect(object sender, RoutedEventArgs e)
        {
            //User.Status = true;
            ConnectMenuItem.IsEnabled = false;
            DisconnectMenuItem.IsEnabled = true;
        }

        private void Show_MessageBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Group Chat Client!", "Group Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void User_Disconnect(object sender, RoutedEventArgs e)
        {
            //User.Status = false;
            DisconnectMenuItem.IsEnabled = false;
            ConnectMenuItem.IsEnabled = true;
        }

        private void App_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (ModifierKeys.Shift & e.KeyboardDevice.Modifiers) != 0)
            {
                e.Handled = true;
                InputTextBox.Text += "\n";
                InputTextBox.CaretIndex = InputTextBox.Text.Length;
            }
            else if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                Messages.Add(new Message(InputTextBox.Text));
                InputTextBox.Text = string.Empty;
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {

        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void ExecuteSendMessage(object? parameter)
        {

        }

        public void AddNewLine(object? parameter)
        { 
        }
    }
}
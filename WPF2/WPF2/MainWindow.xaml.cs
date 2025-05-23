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
        User User = new User();
        ObservableCollection<Message> Messages = new ObservableCollection<Message>();
        SendCommand sendCommand = new SendCommand();
        NewLineCommand newLineCommand = new NewLineCommand();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Messages.Add(new Message("New message", User));
        }

        private void User_Connect(object sender, RoutedEventArgs e)
        {
            User.Status = true;
            ConnectMenuItem.IsEnabled = false;
            DisconnectMenuItem.IsEnabled = true;
        }

        private void Show_MessageBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Group Chat Client!", "Group Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void User_Disconnect(object sender, RoutedEventArgs e)
        {
            User.Status = false;
            DisconnectMenuItem.IsEnabled = false;
            ConnectMenuItem.IsEnabled = true;
        }

        private void App_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
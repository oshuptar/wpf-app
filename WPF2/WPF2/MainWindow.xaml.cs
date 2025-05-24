using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPF2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool MessageTurn { get; set; } = false;
        public ClientConnection ClientConnection { get; private set; } = new ClientConnection();

        public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public User User1 { get; private set; } = new User(false, "User1");
        public User Client { get; private set; } = new User(false, "Client");
        public ObservableCollection<Message> Messages { get; private set; } = new ObservableCollection<Message>();
        //ICommand SendMessageCommand = new SendMessageCommand();
        //ICommand NewLineCommand = new NewLineCommand();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Messages.Add(new Message(User1, User1.Equals(Client),false ,"Hello! How are you?"));
            Messages.Add(new Message(Client, Client.Equals(Client), false, "Hello, good! What a nice day!"));
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMinutes(1);
            dispatcherTimer.Start();
        }
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var message in Messages)
            {
                message.UpdateTimestampDisplay();
            }
        }

        private void User_Connect(object sender, RoutedEventArgs e)
        {
            //Client.Status = true;
            //ConnectMenuItem.IsEnabled = false;
            //DisconnectMenuItem.IsEnabled = true;
            //Messages.Add(new Message(Client, Client.Equals(Client), true, ""));
            Window ConnectDialogWindow = new ConnectDialogWindow(this);
            bool? result = ConnectDialogWindow.ShowDialog();
            //if(Resul)
        }

        private void Show_MessageBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Group Chat Client!", "Group Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void User_Disconnect(object sender, RoutedEventArgs e)
        {
            Client.Status = false;
            DisconnectMenuItem.IsEnabled = false;
            ConnectMenuItem.IsEnabled = true;
            Messages.Add(new Message(null, Client.Equals(Client), true, ""));
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
                SendMessage(this, new RoutedEventArgs());
            }
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            if (!MessageTurn)
                Messages.Add(new Message(User1, User1.Equals(Client), false, InputTextBox.Text));
            else
                Messages.Add(new Message(Client, Client.Equals(Client), false, InputTextBox.Text));
            MessageTurn = !MessageTurn;
            InputTextBox.Text = string.Empty;
        }
    }
}
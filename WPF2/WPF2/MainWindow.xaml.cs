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
using WPF2_Shared;

namespace WPF2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ClientConnection ClientConnection { get; private set; }
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public ObservableCollection<Message> Messages { get; private set; } = new ObservableCollection<Message>();
        public MainWindow()
        {
            InitializeComponent();
            ClientConnection = new ClientConnection(this);
            DataContext = this;
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
        private async void User_Connect(object sender, RoutedEventArgs e)
        {
            ConnectDialogWindow ConnectDialogWindow = new ConnectDialogWindow(this);
            bool? result = ConnectDialogWindow.ShowDialog();
            if(result != null && result.Value)
            {
                User_Connect();
                try
                {
                    await ClientConnection.ClientStartListen();
                }
                catch (IOException)
                {
                    //MessageBox.Show("Error occurred: the connection is closed", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred:" + ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        public void User_Connect()
        {
            Dispatcher.Invoke(() => {
                ConnectMenuItem.IsEnabled = false;
                DisconnectMenuItem.IsEnabled = true;
            } );
        }
        public void User_Disconnect()
        {
            Dispatcher.Invoke(() => {
                DisconnectMenuItem.IsEnabled = false;
                ConnectMenuItem.IsEnabled = true;
            });
        }
        private async void User_Disconnect(object sender, RoutedEventArgs e)
        {
            await ClientConnection.ClientDisconnect();
        }
        private async void App_Exit(object sender, RoutedEventArgs e)
        {
            await ClientConnection.ClientDisconnect();
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
        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = InputTextBox.Text;
                if (!string.IsNullOrEmpty(message))
                    await ClientConnection.SendMessage(message);
                InputTextBox.Text = string.Empty;
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error occurred:" + ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddMessage(Message message)
        {
            Dispatcher.Invoke(() => {
                if (!message.IsSystemMessage && message.Sender?.Username == ClientConnection.Client.Username)
                    message.IsFromClient = true;
                else
                    message.IsFromClient = false;
                Messages.Add(message);
            });
        }
        private void Show_MessageBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a Group Chat Client!", "Group Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
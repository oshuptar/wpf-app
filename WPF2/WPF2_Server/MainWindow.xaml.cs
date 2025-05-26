using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
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
using WPF2_Shared;

namespace WPF2_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ServerLog> ServerLogs { get; private set; } = new ObservableCollection<ServerLog>();
        public ObservableCollection<User> Users { get; private set; } = new ObservableCollection<User>();
        public ServerConnection ServerConnection { get; private set; }
        public MainWindow()
        {
            ServerConnection = new ServerConnection(this);
            DataContext = this;
            InitializeComponent();
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
                {
                    await ServerConnection.BroadcastResponse(ServerConnection.Cts.Token, new SendMessageResponse(null, message, true), null);
                    ServerLogs.Add(new ServerLog($"{message}", "Server", DateTime.Now));
                }
                InputTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred:" + ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddServerLog(ServerLog log)
        {
            Dispatcher.Invoke(() => ServerLogs.Add(log));
        }
        private async void StartServer(object sender, RoutedEventArgs e)
        {
            if (ServerConnection.IsRunning)
            {
                await ServerConnection.ServerStop();
                AddServerLog(new ServerLog("Server stopped", "Server", DateTime.Now));
            }
            else
            {
                try
                {
                    if (!int.TryParse(PortTextBox.Text, out int portNumber) || !IPAddress.TryParse(AddressTextBox.Text, out IPAddress? ipAddress))
                        throw new Exception("Invalid IP address or port number.");

                    if (portNumber < 1300 || portNumber > 65535)
                        throw new Exception("Port number must be between 1300 and 65535.");

                    await ServerConnection.StartServer(ipAddress, portNumber);
                    AddServerLog(new ServerLog($"Server started listening {ipAddress.ToString()}:{portNumber}", "Server", DateTime.Now));
                }
                catch (SocketException ex)
                {
                    MessageBox.Show($"Socket error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AddServerLog(new ServerLog($"Exception occurred: {ex.Message}", "Server", DateTime.Now));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AddServerLog(new ServerLog($"Exception occurred: {ex.Message}", "Server", DateTime.Now));
                }
            }
        }
        public void AddUser(User user)
        {
            Dispatcher.Invoke(() =>
            {
                if (Users.Where(u => u.Username == user.Username).Count() == 0)
                    Users.Add(user);
            });
        }

        public void RemoveUser(string username)
        {
            Dispatcher.Invoke(() =>
            {
                var user = Users.FirstOrDefault(u => u.Username == username);
                if (user != null)
                    Users.Remove(user);
            });
        }

        private async void KickEventHandler(object sender, RoutedEventArgs e)
        {
            var selectedUsers = ClientListView.SelectedItems.Cast<User>().ToList();
            List<Task> tasks = new List<Task>();
            foreach (var user in selectedUsers)
            {
                tasks.Add(ServerConnection.RemoveClient(user.Username));
            }
            await Task.WhenAll(tasks);
            AddServerLog(new ServerLog($"Kicked {selectedUsers.Count} user(s)", "Server", DateTime.Now));
            ClientListView.SelectedItems.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;

namespace WPF2
{
    /// <summary>
    /// Interaction logic for ConnectDialogWindow.xaml
    /// </summary>
    public partial class ConnectDialogWindow : Window
    {
        private MainWindow OwnerWindow { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private IPAddress IpAddress { get; set; }
        private int Port { get; set; }
        public ConnectDialogWindow(MainWindow ownerWindow)
        {
            OwnerWindow = ownerWindow;
            this.Owner = OwnerWindow;
            InitializeComponent();
        }

        private void ConnectHandler(object sender, RoutedEventArgs e)
        {
            ConnectProgressBar.Visibility = Visibility.Visible;
            ConnectButton.IsEnabled = false;

            Username = UsernameTextBox.Text;
            Password = PasswordBox.Password;
            try
            {
                if (!int.TryParse(PortTextBox.Text, out int port) || !IPAddress.TryParse(AddressTextBox.Text, out var ipAddress))
                    throw new Exception("Invalid IP address or port number.");

                Port = port;
                this.IpAddress = ipAddress;

                if (OwnerWindow.ClientConnection.tcpClient.Connected || OwnerWindow.Client.Status)
                    throw new Exception(Username + " is already connected to the server." +
                        " Please disconnect first before trying to connect again.");

                OwnerWindow.ClientConnection.Connect(ipAddress, port);
                NetworkStream stream = OwnerWindow.ClientConnection.tcpClient.GetStream();

                Request connectRequest = new ConnectRequest(Username, Password);
                OwnerWindow.ClientConnection.SendRequest(connectRequest);

                Response? connectResponse = OwnerWindow.ClientConnection.ReadResponse();
                if(connectResponse == null || connectResponse.ResponseType != ResponseType.Connect)
                {
                    throw new Exception("Invalid response from server.");
                }

                ConnectResponse connectResponseData = (ConnectResponse)connectResponse;
                if (!connectResponseData.IsSuccess)
                {
                    throw new Exception("Connection failed: " + connectResponseData.ErrorMessage);
                }

                MessageBoxResult boxResult = MessageBox.Show("Connection successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                if (boxResult == MessageBoxResult.OK)
                {
                    // set values
                    this.Close();
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(IOException ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ConnectProgressBar.Visibility = Visibility.Hidden;
                ConnectButton.IsEnabled = true;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WPF2_Shared;

namespace WPF2;

public class ClientConnection
{
    private object lockObject = new object();
    public User Client { get; set; } = new User(false, "");
    public CancellationTokenSource Cts { get; private set; } = new CancellationTokenSource();
    public TcpClient tcpClient { get; private set; } = new TcpClient();
    public MainWindow MainWindow { get; private set; }
    public ClientConnection(MainWindow mainWindow)
    {
        MainWindow = mainWindow;
    }
    public async Task ClientStartListen()
    {
        try
        {
            while (!Cts.Token.IsCancellationRequested)
            {
                Response? response = await ReadResponseAsync();
                if (response == null) return;
                HandleResponse(response);
            }
        }
        catch (SocketException) { CloseClient(); throw; }
        catch (IOException) { CloseClient(); throw; }
        catch (Exception) { CloseClient(); throw; }
    }
    public void HandleResponse(Response response)
    {
        MainWindow.AddMessage(response.CreateMessage());
    }
    public async Task ClientDisconnect()
    {
        lock(lockObject)
        {
            if (!Client.IsAuthorised) return;
        }
        await SendRequestAsync(new DisconnectRequest(Client.Username));
        this.CloseClient();
    }
    public async Task SendMessage(string message)
    {
        try
        {
            if (!Client.IsAuthorised) return;
            Request request = new SendMessageRequest(Client!.Username, message);
            await SendRequestAsync(request);
            //MainWindow.AddMessage(new Message(Client, false, message, DateTime.Now)); // the message is assumingly rendered on the screen, even if not delivered yet
        }
        catch (Exception) { CloseClient(); }
    }
    public async Task SendRequestAsync(Request request)
    {
        try
        {
            byte[] requestMessage = CreateRequestMessage(request);
            await tcpClient.GetStream().WriteAsync(requestMessage, 0, requestMessage.Length, Cts.Token);
        }
        catch (OperationCanceledException) { CloseClient(); }
        catch (ObjectDisposedException) { CloseClient(); }
    }

    private byte[] CreateRequestMessage(Request request)
    {
        string json = JsonSerializer.Serialize(request);

        int payloadLength = Encoding.UTF8.GetByteCount(json);
        payloadLength = IPAddress.HostToNetworkOrder(payloadLength);

        byte[] payloadLengthBytes = BitConverter.GetBytes(payloadLength);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        return payloadLengthBytes.Concat(jsonBytes).ToArray();
    }

    public async Task<Response?> ReadResponseAsync()
    {
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            byte[] headerLength = new byte[4];
            await stream.ReadExactlyAsync(headerLength, 0, headerLength.Length, Cts.Token);

            int payloadLength = BitConverter.ToInt32(headerLength, 0);
            payloadLength = IPAddress.NetworkToHostOrder(payloadLength);
            byte[] payload = new byte[payloadLength];

            await stream.ReadExactlyAsync(payload, 0, payloadLength, Cts.Token);
            string json = Encoding.UTF8.GetString(payload);

            return JsonSerializer.Deserialize<Response>(json);
        }
        catch (OperationCanceledException) { CloseClient(); }
        catch (ObjectDisposedException) { CloseClient(); }
        return null;
    }

    public async Task ConnectAsync(IPAddress ipAddress, int port)
    {
        try
        {
            if (tcpClient.Connected)
                throw new Exception("User is already connected to the server." +
                        " Please disconnect first before trying to connect again.");
            await tcpClient.ConnectAsync(ipAddress, port, Cts.Token);
        }
        catch (OperationCanceledException) { CloseClient(); throw; }
        catch (SocketException) { CloseClient(); throw; }
        catch (Exception) { CloseClient();throw; }
    }

    private void CloseClient()
    {
        lock(lockObject)
        {
            if (!Client.IsAuthorised) return; 
        }
        CloseRoutine();
    }
    public void CloseRoutine()
    {
        lock(lockObject)
        {
            Client.IsAuthorised = false;
        }
        Cts.Cancel();
        tcpClient.Dispose();
        MainWindow.User_Disconnect();
        ResetClient();
    }
    public void ResetClient()
    {
        tcpClient = new TcpClient();
        Cts = new CancellationTokenSource();
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WPF2_Shared;

namespace WPF2_Server;

public class Channel
{
    public TcpClient TcpClient { get; set; } = new TcpClient();
    public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
    public Channel(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
    }
}

public class ClientContext
{
    public Channel Channel { get; set; }
    public User User { get; set; } = new User(false, "");
    public ClientContext(TcpClient tcpClient, User user)
    {
        Channel = new Channel(tcpClient);
        User = user;
    }
    public ClientContext(Channel channel, User user)
    {
        Channel = channel;
        User = user;
    }
}

public class ServerConnection : INotifyPropertyChanged
{
    public ConcurrentDictionary<string, ClientContext> Clients { get; set; } = new ConcurrentDictionary<string, ClientContext>();
    public CancellationTokenSource? Cts { get; private set; } = new CancellationTokenSource();

    private bool _isRunning = false;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChangedEventHandler(nameof(IsRunning));
        }
    }
    public TcpListener Listener { get; private set; }
    public int Port { get; private set; }
    public IPAddress IpAddress { get; private set; }
    public MainWindow MainWindow { get; private set; }
    public ServerConnection(MainWindow mainWindow)
    {
        MainWindow = mainWindow;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChangedEventHandler(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task StartServer(IPAddress ipAddress, int port)
    {
        try
        {
            if (IsRunning) return;
            Cts = new CancellationTokenSource();
            Listener = new TcpListener(ipAddress, port);
            Listener.Start();
            IsRunning = true;

            await AcceptConnectionsAsync(Cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Graceful cancellation
            throw;
        }
        catch (SocketException)
        {
            throw;
        }
        catch (IOException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await ServerStop();
        }
    }

    public async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                TcpClient tcpClient = await Listener.AcceptTcpClientAsync(cancellationToken);
                Channel channel = new Channel(tcpClient);
                Task.Run(async () => await HandleNewClientSession(channel, cancellationToken));
            }
            catch (OperationCanceledException)
            {
                MainWindow.AddServerLog(new ServerLog("Operation is cancelled", "Server", DateTime.Now));
                break;
            }
            catch (Exception) { throw; }
        }

    }

    private async Task HandleNewClientSession(Channel tcpClient, CancellationToken cancellationToken)
    {
        try
        {
            var (success, context) = await HandleAuthorization(cancellationToken, tcpClient);
            if (success)
            {
                await HandleClientConnection(cancellationToken, context!);
            }
        }
        catch (Exception) { throw; }
    }

    public async Task HandleClientConnection(CancellationToken cancellationToken, ClientContext context)
    {
        if (context.User == null || !context.User.IsAuthorised)
            return;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Request? request = await ReadRequestAsync(cancellationToken, context.Channel);
                if (request == null)
                {
                    // Perform logging
                    throw new Exception();
                }
                await HandleRequest(cancellationToken, request);
            }
        }
        catch (EndOfStreamException)
        {
            await RemoveClient(context.User?.Username);
            throw;
        }
        catch (ObjectDisposedException) { await RemoveClient(context.User.Username); }
        catch (Exception)
        {
            // Perform logging
            await RemoveClient(context.User?.Username);
            throw;
        }
    }

    public async Task HandleRequest(CancellationToken cancellationToken, Request request)
    {
        switch (request.RequestType)
        {
            case RequestType.Disconnect:
                {
                    DisconnectRequest disconnectRequest = (DisconnectRequest)request;
                    if (!Clients.TryGetValue(disconnectRequest.Username, out var context)) return;
                    await RemoveClient(disconnectRequest.Username);
                    break;
                }
            case RequestType.SendMessage:
                {
                    SendMessageRequest sendMessageRequest = (SendMessageRequest)request;
                    if (!Clients.TryGetValue(request.Username, out var context)) return;
                    SendMessageResponse sendMessageResponse = new SendMessageResponse(context.User, sendMessageRequest.Message, false);
                    await BroadcastResponse(cancellationToken, sendMessageResponse, sendMessageRequest.Username);
                    await SendResponseAsync(cancellationToken, context.Channel, sendMessageResponse);
                    MainWindow.AddServerLog(new ServerLog(sendMessageRequest.Username + $": {sendMessageRequest.Message}", "Server", DateTime.Now));
                    break;
                }
        }
    }

    public async Task<Request?> ReadRequestAsync(CancellationToken cancellationToken, Channel channel)
    {
        NetworkStream stream = channel.TcpClient.GetStream();

        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, 0, header.Length, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return null;

        int payloadLength = BitConverter.ToInt32(header, 0);
        payloadLength = IPAddress.NetworkToHostOrder(payloadLength);
        byte[] payload = new byte[payloadLength];

        await stream.ReadExactlyAsync(payload, 0, payloadLength, cancellationToken);
        if (cancellationToken.IsCancellationRequested) return null;

        string json = Encoding.UTF8.GetString(payload);

        return JsonSerializer.Deserialize<Request>(json);
    }

    public async Task SendResponseAsync(CancellationToken cancellationToken, Channel channel, Response response)
    {
        await channel.Semaphore.WaitAsync(cancellationToken);
        try
        {
            NetworkStream stream = channel.TcpClient.GetStream();

            byte[] header = new byte[4];
            string json = JsonSerializer.Serialize(response);
            int payloadLength = Encoding.UTF8.GetByteCount(json);

            payloadLength = IPAddress.HostToNetworkOrder(payloadLength);
            header = BitConverter.GetBytes(payloadLength);

            byte[] payload = Encoding.UTF8.GetBytes(json);
            byte[] responseMessage = header.Concat(payload).ToArray();

            await stream.WriteAsync(responseMessage, 0, responseMessage.Length, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
        }
        finally
        {
            channel.Semaphore.Release();
        }
    }

    public async Task BroadcastResponse(CancellationToken cancellationToken, Response response, string? senderUsername)
    {
        List<Task> tasks = new List<Task>();
        var clients = Clients.Values.Where(c => c.User.Username != senderUsername).ToList();
        //var clients = Clients.Values.ToList();
        foreach (var client in clients) 
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendResponseAsync(cancellationToken, client.Channel, response);
                }
                catch (SocketException)
                {
                    await RemoveClient(client.User.Username);
                    // Perform logging
                }
                catch (IOException)
                {
                    await RemoveClient(client.User.Username);
                    // Perform logging
                }
                catch (ObjectDisposedException) { }
            }));
        }
        await Task.WhenAll(tasks);
    }

    public async Task<(bool, ClientContext?)> HandleAuthorization(CancellationToken cancellationToken, Channel channel)
    {
        try
        {
            ConnectRequest? connectRequest = (await ReadRequestAsync(cancellationToken, channel)) as ConnectRequest;
            if (connectRequest == null)
            {
                // Perform logging
                return (false, null);
            }

            string username = string.Empty;
            string password = string.Empty;
            MainWindow.Dispatcher.Invoke(() =>
            {
                username = MainWindow.UsernameTextBox.Text;
                password = MainWindow.PasswordBox.Password;
            });

            if (String.Equals(connectRequest.Username, username) && String.Equals(connectRequest.Password, password))
            {
                User user = new User(true, username);
                ClientContext clientContext = new ClientContext(channel, user);
                bool result = Clients.TryAdd(user.Username, clientContext); // remember that tuples are immutable
                if (!result)
                {
                    await SendResponseAsync(cancellationToken, channel, new ConnectResponse(null, false, "User with the same username already exists"));
                    MainWindow.AddServerLog(new ServerLog(connectRequest.Username + " failed to log in: the username already exists", "Server", DateTime.Now));
                    channel.TcpClient.Close();
                    channel.TcpClient.Dispose();
                    return (false, null);
                }
                MainWindow.AddUser(user);
                ConnectResponse connectResponse = new ConnectResponse(user, true, "");
                await SendResponseAsync(cancellationToken, channel, connectResponse);
                await BroadcastResponse(cancellationToken, connectResponse, user.Username);
                MainWindow.AddServerLog(new ServerLog(connectRequest.Username + " logged in successfully", "Server", DateTime.Now));
                return (true, clientContext);
            }
            else
            {
                await SendResponseAsync(cancellationToken, channel, new ConnectResponse(null, false, "Invalid credentials"));
                MainWindow.AddServerLog(new ServerLog(connectRequest.Username + " failed to log in: invalid credentials", "Server", DateTime.Now));
                return (false, null);
            }
        }
        catch (SocketException)
        {
            channel.TcpClient.Close();
            channel.TcpClient.Dispose();
            // Perform logging
        }
        catch (EndOfStreamException)
        {
            channel.TcpClient.Close();
            channel.TcpClient.Dispose();
        }
        catch (IOException)
        {
            channel.TcpClient.Close();
            channel.TcpClient.Dispose();
            // Perform logging
        }
        catch (Exception)
        {
            channel.TcpClient.Close();
            channel.TcpClient.Dispose();
            // Perform logging
        }
        MainWindow.AddServerLog(new ServerLog("Exception occurred during authorization", "Server", DateTime.Now));
        return (false, null);
    }

    public async Task ServerStop()
    {
        if (!IsRunning) return;

        Cts.Cancel();
        IsRunning = false;

        List<Task> tasks = new List<Task>();
        foreach (var client in Clients.Values)
        {
            tasks.Add(RemoveClient(client.User.Username));
        }
        await Task.WhenAll(tasks);

        Listener.Stop();
        Listener.Dispose();
    }

    public async Task RemoveClient(string? username)
    {
        if (username == null) return;

        if (Clients.TryRemove(username, out var client))
        {
            await client.Channel.Semaphore.WaitAsync();
            try
            {
                client.Channel.TcpClient.Close();
                client.Channel.TcpClient.Dispose();
                MainWindow.RemoveUser(username);
                await BroadcastResponse(Cts.Token, new DisconnectResponse(client.User), client.User.Username);
                MainWindow.AddServerLog(new ServerLog(username + " disconnected", "Server", DateTime.Now));
            }
            finally
            {
                client.Channel.Semaphore.Release();
            }
        }
    }
}

// But you still must use Dispatcher.Invoke when reading them from a background thread — otherwise, you’ll crash at runtime.

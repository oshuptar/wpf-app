using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WPF2;

public class ClientConnection
{
    public TcpClient tcpClient { get; private set; } = new TcpClient();
    public void SendRequest(Request request)
    {
        try
        {
            byte[] requestMessage = CreateRequestMessage(request);
            tcpClient.GetStream().Write(requestMessage, 0, requestMessage.Length);
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

    public Response? ReadResponse()
    {
        NetworkStream stream = tcpClient.GetStream();
        byte[] headerLength = new byte[4];
        stream.ReadExactly(headerLength, 0, headerLength.Length);

        int payloadLength = BitConverter.ToInt32(headerLength, 0);
        payloadLength = IPAddress.NetworkToHostOrder(payloadLength);
        byte[] payload = new byte[payloadLength];

        stream.ReadExactly(payload, 0, payloadLength);
        string json = Encoding.UTF8.GetString(payload);

        return JsonSerializer.Deserialize<Response>(json);
    }

    public void Connect(IPAddress ipAddress, int port)
    {
        try
        {
            tcpClient.Connect(ipAddress, port);
        }catch(SocketException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }
}

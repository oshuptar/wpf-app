using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WPF2;

public enum RequestType
{
    Connect,
    Disconnect,
    ReceiveMessage
};

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$discriminator")]
[JsonDerivedType(typeof(DisconnectRequest), "DisconnectRequest")]
[JsonDerivedType(typeof(ConnectRequest), "ConnectRequest")]
[JsonDerivedType(typeof(SendMessageRequest), "SendMessageRequest")]
public class Request
{
    [JsonInclude]
    public RequestType RequestType {get; protected set;}
    [JsonInclude]
    public string Username { get; protected set; }

    [JsonConstructor]
    public Request(string username)
    {
        Username = username;
    }
}

public class DisconnectRequest : Request
{
    [JsonConstructor]
    public DisconnectRequest(string username): base(username)
    {
        RequestType = RequestType.Disconnect;
    }
}

public class ConnectRequest : Request
{
    [JsonInclude]
    public string Password { get; private set; }
    [JsonConstructor]
    public ConnectRequest(string username, string password) : base(username)
    {
        RequestType = RequestType.Connect;
        Password = password;
    }
}

public class SendMessageRequest : Request
{
    [JsonInclude]
    public string Message { get; private set; }
    public SendMessageRequest(string username, string message) : base(username)
    {
        RequestType = RequestType.ReceiveMessage;
        Message = message;
    }
}

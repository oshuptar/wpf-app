using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WPF2;

public enum ResponseType
{
    Connect,
    SendMessage,
}
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ConnectResponse), "Connect Response")]
[JsonDerivedType(typeof(SendMessageResponse), "Message Response")]
public class Response
{
    [JsonInclude]
    public string Username { get; protected set; }
    [JsonInclude]
    public ResponseType ResponseType { get; protected set; }
    [JsonConstructor]
    public Response(string username)
    {
        Username = username;
    }
}

public class ConnectResponse : Response
{
    [JsonInclude]
    public bool IsSuccess { get; private set; }
    [JsonInclude]
    public string? ErrorMessage { get; private set; }
    [JsonConstructor]
    public ConnectResponse(string username, bool isSuccess, string message) : base(username)
    {
        ResponseType = ResponseType.Connect;
        IsSuccess = isSuccess;
        ErrorMessage = message;
    }
}

public class SendMessageResponse : Response
{
    [JsonInclude]
    public string Message { get; protected set; }
    [JsonConstructor]
    public SendMessageResponse(string username, string message) : base(username)
    {
        ResponseType = ResponseType.SendMessage;
        Message = message;
    }
}

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace WPF2_Shared;

public class ServerLog
{
    public string Message { get; }
    public string MessageContent { get; set; }
    public string Sender { get; set; }
    public DateTime Timestamp { get; set; }
    public ServerLog(string messageContent, string sender, DateTime timeStamp)
    {
        MessageContent = messageContent;
        Sender = sender;
        Timestamp = timeStamp;
        Message = $"[{Timestamp.ToString("dd/MM/yyyy HH:mm:ss")}] - {sender}: {messageContent}";
    }
}

public class User
{
    [JsonInclude]
    public bool IsAuthorised { get; set; } = false;
    [JsonInclude]
    public string Username { get; set; }
    public User() { }
    public User(bool isAuthorised, string username)
    {
        IsAuthorised = isAuthorised;
        Username = username;
    }
}

public enum RequestType
{
    Connect,
    Disconnect,
    SendMessage
};

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$discriminator")]
[JsonDerivedType(typeof(DisconnectRequest), "DisconnectRequest")]
[JsonDerivedType(typeof(ConnectRequest), "ConnectRequest")]
[JsonDerivedType(typeof(SendMessageRequest), "SendMessageRequest")]
public class Request
{
    [JsonInclude]
    public RequestType RequestType { get; protected set; }
    [JsonInclude]
    public string Username { get; protected set; }

    public Request() { }
    public Request(string username)
    {
        Username = username;
    }
}

public class DisconnectRequest : Request
{
    public DisconnectRequest(): base() { }
    public DisconnectRequest(string username) : base(username)
    {
        RequestType = RequestType.Disconnect;
    }
}

public class ConnectRequest : Request
{
    [JsonInclude]
    public string Password { get; private set; }
    public ConnectRequest(): base() { }
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
    public SendMessageRequest() : base() { }
    public SendMessageRequest(string username, string message) : base(username)
    {
        RequestType = RequestType.SendMessage;
        Message = message;
    }
}

public enum ResponseType
{
    Connect,
    Disconnect,
    SendMessage,
}
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ConnectResponse), "Connect Response")]
[JsonDerivedType(typeof(DisconnectResponse), "Disconnect Response")]
[JsonDerivedType(typeof(SendMessageResponse), "Message Response")]
public abstract class Response
{
    [JsonInclude]
    public DateTime Timestamp { get; set; } = DateTime.Now;
    [JsonInclude]
    public User? User { get; protected set; }
    [JsonInclude]
    public ResponseType ResponseType { get; protected set; }
    public Response() { }
    public Response(User? user)
    {
        User = user;
    }
    public abstract Message CreateMessage();
}

public class DisconnectResponse : Response
{
    public DisconnectResponse() : base() { }
    public DisconnectResponse(User? user) : base(user)
    {
        ResponseType = ResponseType.Disconnect;
    }
    public override Message CreateMessage()
    {
        return new Message(User, true, $"User {User?.Username} disconnected", Timestamp);
    }
}


public class ConnectResponse : Response
{
    [JsonInclude]
    public bool IsSuccess { get; private set; }
    [JsonInclude]
    public string? ErrorMessage { get; private set; }
    public ConnectResponse(): base() { }
    public ConnectResponse(User? user, bool isSuccess, string? message) : base(user)
    {
        ResponseType = ResponseType.Connect;
        IsSuccess = isSuccess;
        ErrorMessage = message;
    }
    public override Message CreateMessage()
    {
        return new Message(User, true, $"User {User?.Username} connected", Timestamp);
    }
}

public class SendMessageResponse : Response
{
    [JsonInclude]
    public bool IsSystemMessage { get; set; }
    [JsonInclude]
    public string Message { get; protected set; }
    public SendMessageResponse() : base() { }
    public SendMessageResponse(User? user, string message, bool isSystemMessage) : base(user)
    {
        ResponseType = ResponseType.SendMessage;
        Message = message;
        IsSystemMessage = isSystemMessage;
    }
    public override Message CreateMessage()
    {
        return new Message(User, IsSystemMessage, Message, Timestamp);
    }
}

public class Message : INotifyPropertyChanged
{
    public bool IsFromClient { get; set; }
    public bool IsSystemMessage { get; set; }
    public string MessageContent { get; set; }
    public User? Sender { get; set; }
    public DateTime Timestamp { get; set; }
    private string _stringTimestampDisplay;
    public event PropertyChangedEventHandler? PropertyChanged;
    public string TimestampDisplay
    {
        set
        {
            _stringTimestampDisplay = value; OnPropertyChanged(nameof(TimestampDisplay));
        }
        get
        {
            return _stringTimestampDisplay;
        }
    }
    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateTimestampDisplay()
    {
        TimeSpan timeDifference = DateTime.Now - Timestamp;
        switch (timeDifference)
        {
            case TimeSpan t when t.TotalSeconds < 60:
                TimestampDisplay = $"Now";
                break;
            case TimeSpan t when t.TotalMinutes < 15:
                TimestampDisplay = $"{t.Minutes} minutes ago";
                break;
            case TimeSpan t when t.TotalDays < 1:
                TimestampDisplay = $"{Timestamp.ToString("HH:MM")}";
                break;
            case TimeSpan t when t.TotalDays >= 1:
                TimestampDisplay = $"{Timestamp.ToString("dd/MM/YYYY")}";
                break;
            default:
                TimestampDisplay = string.Empty;
                break;
        }
    }
    public Message(User? sender, bool isClientSender, bool isSystemMessage, string messageContent)
    {
        Timestamp = DateTime.Now;
        UpdateTimestampDisplay();
        IsSystemMessage = isSystemMessage;
        MessageContent = messageContent;
        Sender = sender;
        IsFromClient = isClientSender;
    }
    public Message(User? sender, bool isSystemMessage, string messageContent, DateTime timestamp)
    {
        Timestamp = timestamp;
        UpdateTimestampDisplay();
        IsSystemMessage = isSystemMessage;
        MessageContent = messageContent;
        Sender = sender;
    }
}

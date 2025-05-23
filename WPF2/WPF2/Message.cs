using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WPF2;

public class Message : INotifyPropertyChanged
{
    public bool IsFromClient { get; set; }
    public bool IsSystemMessage { get; set; }
    public string MessageContent {  get; set; }
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
    public Message(User? sender,bool isClientSender, bool isSystemMessage, string messageContent)
    {
        Timestamp = DateTime.Now;
        UpdateTimestampDisplay();
        IsSystemMessage = isSystemMessage;
        MessageContent = messageContent;
        Sender = sender;
        IsFromClient = isClientSender;
    }
}

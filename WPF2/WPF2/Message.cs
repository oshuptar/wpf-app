using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF2;

public class Message
{
    public bool IsSystemMessage { get; set; }
    public string MessageContent {  get; set; }
    public User? Sender { get; set; }
    public DateTime Timestamp { get; set; } 
    public Message(string messageContent, User sender)
    {
        Timestamp = DateTime.Now;
        MessageContent = messageContent;
        Sender = sender;
    }
    public Message(bool isSystemMessage, string messageContent, User? sender)
    {
        IsSystemMessage = isSystemMessage;
        MessageContent = messageContent;
        Sender = sender;
    }
}

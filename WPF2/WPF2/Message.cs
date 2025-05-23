using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF2;

public class Message
{
    public string MessageContent {  get; set; }
    public User? Sender { get; set; }
    public DateTime Timestamp { get; set; } 
    public Message(string messageContent, User sender)
    {
        Timestamp = DateTime.Now;
        MessageContent = messageContent;
        Sender = sender;
    }
    public Message(string messageContent)
    {
        MessageContent = messageContent;
    }
}

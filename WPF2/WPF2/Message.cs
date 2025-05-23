using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF2;

public class Message
{
    public string MessageInfo {  get; set; }
    public User Sender { get; set; }

    public Message(string messageInfo, User sender)
    {
        MessageInfo = messageInfo;
        Sender = sender;
    }
}

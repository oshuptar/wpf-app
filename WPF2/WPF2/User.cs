using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF2;

public class User
{
    public bool Status { get; set; } = false;
    public string Username { get; set; }
    public User(bool status, string username)
    {
        Status = status;
        Username = username;
    }
}

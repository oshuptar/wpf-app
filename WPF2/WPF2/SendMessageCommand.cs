using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPF2;

public class SendMessageCommand : CustomCommand
{
    public SendMessageCommand(Action<object?> executeCommand, Func<object?, bool> canExecuteCommand) : base(executeCommand, canExecuteCommand)
    {

    }
}

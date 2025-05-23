using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPF2;

public class CustomCommand : ICommand
{
    protected Action<object?> _executeCommand;
    protected Func<object?, bool> _canExecuteCommand;
    public CustomCommand(Action<object?> executeCommand, Func<object?, bool> canExecuteCommand)
    {
        _executeCommand = executeCommand;
        _canExecuteCommand = canExecuteCommand;
    }
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    public bool CanExecute(object? parameter)
    {
        return _canExecuteCommand.Invoke(parameter);
    }

    public void Execute(object? parameter)
    {
        _executeCommand.Invoke(parameter);
    }
}

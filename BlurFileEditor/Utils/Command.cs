using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlurFileEditor.Utils;
public class Command : ICommand
{
    Action ExecuteAction { get; }
    Func<bool> CanExecuteAction { get; }

    public Command(Action executeAction)
    {
        ExecuteAction = executeAction;
        CanExecuteAction = () => true;
    }

    public Command(Action executeAction, Func<bool> canExecuteAction)
    {
        ExecuteAction = executeAction;
        CanExecuteAction = canExecuteAction;
    }

    public event EventHandler? CanExecuteChanged;
    public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    public bool CanExecute(object? parameter) => CanExecuteAction();
    public bool CanExecute() => CanExecute(null);

    public void Execute(object? parameter) => ExecuteAction();
    public void Execute() => Execute(null);
}
public class Command<T> : ICommand where T : notnull
{
    Action<T?> ExecuteAction { get; }
    Func<T?, bool> CanExecuteAction { get; }
    public Command(Action<T?> executeAction)
    {
        ExecuteAction = executeAction;
        CanExecuteAction = _ => true;
    }
    public Command(Action executeAction, Func<T?, bool> canExecuteAction)
    {
        ExecuteAction = _ => executeAction();
        CanExecuteAction = canExecuteAction;
    }
    public Command(Action<T?> executeAction, Func<bool> canExecuteAction)
    {
        ExecuteAction = executeAction;
        CanExecuteAction = _ => canExecuteAction();
    }
    public Command(Action<T?> executeAction, Func<T?, bool> canExecuteAction)
    {
        ExecuteAction = executeAction;
        CanExecuteAction = canExecuteAction;
    }

    public event EventHandler? CanExecuteChanged;
    public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    public bool CanExecute(object? parameter) => CanExecuteAction((T?)parameter);
    public bool CanExecute() => CanExecute(null);
    public void Execute(object? parameter) => ExecuteAction((T?)parameter);
    public void Execute() => Execute(null);
}
public class Command<T, U> : ICommand where T : notnull where U : notnull
{
    Action<T?> ExecuteAction { get; }
    Func<U?, bool> CanExecuteAction { get; }
    public Command(Action<T?> executeAction, Func<U?, bool> canExecuteAction)
    {
        ExecuteAction = executeAction;
        CanExecuteAction = canExecuteAction;
    }

    public event EventHandler? CanExecuteChanged;
    public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    public bool CanExecute(object? parameter) => CanExecuteAction((U?)parameter);

    public void Execute(object? parameter) => ExecuteAction((T?)parameter);
}
using System;
using JinToliq.Umvvm.ViewModel.Exceptions;

namespace JinToliq.Umvvm.ViewModel
{
  public interface ICommand : IDisposable
  {
    TCommand As<TCommand>() where TCommand : ICommand;
  }

  public class Command : ICommand
  {
    private Action _action;

    public Command(Action action) => _action = action ?? throw new ArgumentNullException(nameof(action));

    public TCommand As<TCommand>() where TCommand : ICommand
    {
      if (this is TCommand typed)
        return typed;

      throw new InvalidCommandTypeException();
    }

    public void Invoke() => _action();

    public void Dispose()
    {
      if (_action is null)
        throw new ObjectDisposedException("Command is already disposed");

      _action = null;
    }
  }

  public class Command<TArg> : ICommand
  {
    private Action<TArg> _action;

    public Command(Action<TArg> action) => _action = action ?? throw new ArgumentNullException(nameof(action));

    public TCommand As<TCommand>() where TCommand : ICommand
    {
      if (this is TCommand typed)
        return typed;

      throw new InvalidCommandTypeException();
    }

    public void Invoke(TArg arg) =>
      _action(arg);

    public void Dispose()
    {
      if (_action is null)
        throw new ObjectDisposedException("Command is already disposed");

      _action = null;
    }
  }
}
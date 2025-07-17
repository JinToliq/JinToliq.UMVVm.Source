using System;

namespace JinToliq.Umvvm.ViewModel
{
  public interface IInjectedContextState
  {
    IContextWithState GetBaseContext();
    void SetBaseContext(IContextWithState context);
  }

  public interface IInjectedContextState<T> : IInjectedContextState
    where T : IContextWithState
  {
    T Context { get; set; }

    IContextWithState BaseContext
    {
      get => GetBaseContext();
      set => SetBaseContext(value);
    }

    IContextWithState IInjectedContextState.GetBaseContext() => Context;

    void IInjectedContextState.SetBaseContext(IContextWithState context)
    {
      switch (context)
      {
        case null:
          Context = default;
          return;

        case T typedContext:
          Context = typedContext;
          return;

        default:
          throw new InvalidOperationException(
            $"State {GetType().Name} is not compatible with context {typeof(T).Name}. " +
            $"Expected context type: {typeof(T).Name}, actual context type: {context.GetType().Name}.");
      }
    }
  }
}
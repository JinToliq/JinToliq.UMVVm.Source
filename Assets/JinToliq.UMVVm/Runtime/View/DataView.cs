using System;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View
{
  public abstract class DataView<TContext> : DataView where TContext : Context, new()
  {
    protected TContext Context { get; set; }

    protected sealed override Context BaseContext => Context ??= new TContext();
  }

  public abstract class DataView : MonoBehaviour, IDataView
  {
    protected abstract Context BaseContext { get; }

    public Property GetProperty(string property) => BaseContext.GetProperty(property);

    public TProperty GetProperty<TProperty>(string property) where TProperty : Property => BaseContext.GetProperty<TProperty>(property);

    public Command GetCommand(string property) => BaseContext.GetCommand(property);

    public TCommand GetCommand<TCommand>(string property) where TCommand : ICommand => BaseContext.GetCommand<TCommand>(property);

    public void ClearState()
    {
      if (BaseContext is IContextWithState contextWithState)
        contextWithState.SetStateObject(null);
    }

    public void TrySetState(object state)
    {
      if (BaseContext is IContextWithState contextWithState)
        contextWithState.SetStateObject(state);
    }

    public void SetState<TState>(TState state)
    {
      if (BaseContext is IContextWithState contextWithState)
        contextWithState.SetStateObject(state);
      else
        throw new ArgumentException($"Context {BaseContext!.GetType()} does not support states");
    }

    protected virtual void OnEnabled() {}

    protected virtual void OnDisabled() {}

    private void OnEnable()
    {
      BaseContext!.Enable();
      OnEnabled();
    }

    private void OnDisable()
    {
      BaseContext.Disable();
      OnDisabled();
    }

    private void Update() => BaseContext.Update();
  }
}
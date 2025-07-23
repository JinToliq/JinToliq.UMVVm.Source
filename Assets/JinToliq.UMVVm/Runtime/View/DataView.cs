using System;
using System.Diagnostics.CodeAnalysis;
using JinToliq.Umvvm.View.Lifecycle;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View
{
  public interface IDataView<TContext> : IDataView where TContext : Context, new()
  {
    public TContext TargetContext { get; }

    Context IDataView.Context => Context;
  }

  public abstract class DataView : MonoBehaviour, IDataView
  {
    public abstract Context Context { get; }

    public Property GetProperty([NotNull] string property, string masterPath = null) => Context.GetProperty(property, masterPath is null ? ReadOnlySpan<char>.Empty : masterPath.AsSpan());

    public TProperty GetProperty<TProperty>([NotNull] string property, string masterPath = null) where TProperty : Property => Context.GetProperty<TProperty>(property, masterPath is null ? ReadOnlySpan<char>.Empty : masterPath.AsSpan());

    public Command GetCommand([NotNull] string property, string masterPath = null) => Context.GetCommand(property, masterPath is null ? ReadOnlySpan<char>.Empty : masterPath.AsSpan());

    public TCommand GetCommand<TCommand>([NotNull] string property, string masterPath = null) where TCommand : ICommand => Context.GetCommand<TCommand>(property, masterPath is null ? ReadOnlySpan<char>.Empty : masterPath.AsSpan());

    public void ClearState()
    {
      if (Context is IContextWithState contextWithState)
        contextWithState.SetStateObject(null);
    }

    public void TrySetState(object state)
    {
      if (Context is IContextWithState contextWithState)
        contextWithState.SetStateObject(state);
    }

    public void SetState<TState>(TState state)
    {
      if (Context is IContextWithState contextWithState)
        contextWithState.SetStateObject(state);
      else
        throw new ArgumentException($"Context {Context!.GetType()} does not support states");
    }

    protected virtual void OnEnabled() {}

    protected virtual void OnDisabled() {}

    private void OnEnable()
    {
      Context!.Enable();
      OnEnabled();
    }

    private void OnDisable()
    {
      Context.Disable();
      OnDisabled();
    }
  }


  public abstract class DataView<TContext> : DataView, IDataView<TContext>
    where TContext : Context, new()
  {
    public TContext TargetContext { get; private set; }

    public sealed override Context Context => TargetContext ??= new();

    protected virtual void OnAwake() {}

    private void Awake()
    {
      if (Application.isPlaying)
      {
        var contextType = typeof(TContext);
        if (contextType.IsSubclassOf(typeof(IContextUpdatable)))
        {
          if (!gameObject.TryGetComponent<DataViewUpdatable>(out var dataViewUpdatable))
            dataViewUpdatable = gameObject.AddComponent<DataViewUpdatable>();

          dataViewUpdatable.Initialize(this);
        }
      }

      OnAwake();
    }
  }
}
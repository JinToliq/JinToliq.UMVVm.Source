using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JinToliq.Umvvm.ViewModel.Exceptions;

namespace JinToliq.Umvvm.ViewModel
{
  public interface IContext : IDisposable
  {
    bool LifecycleIsManagedByParent { get; }
    void SetParent(IContext parent);
    Property GetProperty(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath);
    TProperty GetProperty<TProperty>(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath) where TProperty : Property;
    Command GetCommand(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath);
    TCommand GetCommand<TCommand>(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath) where TCommand : ICommand;
    void Enable();
    void Disable();
  }

  public interface IContextUpdatable : IContext
  {
    public void Update();
    public void UpdateChildren();
  }

  public interface IContextWithState : IContext
  {
    void SetStateObject(object state);
    object GetStateObject();
  }

  public interface IContextWithState<TState> : IContextWithState
  {
    TState CurrentState { get; set; }
    void Set(TState state);
  }

  public abstract class Context<TState> : Context, IContextWithState<TState>
  {
    private TState _currentState;

    public TState CurrentState
    {
      get => _currentState;
      set
      {
        if (_currentState is not null && _currentState is IInjectedContextState previous)
          previous.SetBaseContext(null);

        if (value is not null && value is IInjectedContextState updated)
          updated.SetBaseContext(this);

        _currentState = value;
        OnStateChanged();
      }
    }

    public void Set(TState state) =>
      CurrentState = state;

    public void SetStateObject(object state)
    {
      if (state is null)
      {
        CurrentState = default;
        return;
      }

      if (state is not TState typedState)
        throw new ArgumentException($"State object has type {state.GetType()} but {typeof(TState)} is required for context {GetType().Name}");

      CurrentState = typedState;
    }

    public object GetStateObject() => _currentState;

    protected virtual void OnStateChanged() {}
  }

  public abstract class Context : IContext
  {
    public const char ParentContextMarker = '#';
    public const char ChildContextMarker = '.';

    private IContext _parent;
    private Dictionary<string, Property> _properties;
    private Dictionary<string, ICommand> _commands;
    private Dictionary<string, IContext> _contexts;

    public virtual bool LifecycleIsManagedByParent => true;

    public void SetParent(IContext parent) => _parent = parent;

    public Property GetProperty(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath)
    {
      AssertNullOrEmpty(name);

      if (IsStartsWithParentMarker(masterPath))
      {
        if (_parent is null)
          throw new ContextHasNoParent(PathToString(name, masterPath));

        return _parent.GetProperty(name, masterPath[1..]);
      }

      if (!masterPath.IsEmpty)
      {
        var childMarkerIndex = masterPath.IndexOf(ChildContextMarker);
        if (childMarkerIndex == 0)
          throw new InvalidPropertyName(PathToString(name, masterPath), GetType());

        if (childMarkerIndex > 0)
        {
          var contextName = new string(masterPath[..childMarkerIndex]);
          AssertRegistered(_contexts, contextName);
          return _contexts[contextName].GetProperty(name, masterPath[(childMarkerIndex + 1)..]);
        }
        else
        {
          var contextName = new string(masterPath);
          AssertRegistered(_contexts, contextName);
          return _contexts[contextName].GetProperty(name, ReadOnlySpan<char>.Empty);
        }
      }

      if (IsStartsWithParentMarker(name))
      {
        if (_parent is null)
          throw new ContextHasNoParent(PathToString(name, masterPath));

        return _parent.GetProperty(name[1..], masterPath);
      }

      {
        var childMarkerIndex = name.IndexOf(ChildContextMarker);
        if (childMarkerIndex == 0)
          throw new InvalidPropertyName(PathToString(name, masterPath), GetType());

        if (childMarkerIndex > 0)
        {
          var contextName = new string(name[..childMarkerIndex]);
          AssertRegistered(_contexts, contextName);
          return _contexts[contextName].GetProperty(name[(childMarkerIndex + 1)..], masterPath);
        }

        var nameString = new string(name);
        AssertRegistered(_properties, nameString);
        return _properties[nameString];
      }
    }

    public TProperty GetProperty<TProperty>(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath) where TProperty : Property
    {
      return GetProperty(name, masterPath).As<TProperty>();
    }

    public Command GetCommand(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath) => GetCommandInternal(name, masterPath).As<Command>();

    public TCommand GetCommand<TCommand>(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath) where TCommand : ICommand => GetCommandInternal(name, masterPath).As<TCommand>();

    public IContext GetChildContext(string name)
    {
      AssertRegistered(_contexts, name);
      return _contexts[name];
    }

    public void Enable()
    {
      OnEnabled();

      if (_contexts is null)
        return;

      foreach (var context in _contexts)
      {
        if (context.Value.LifecycleIsManagedByParent)
          context.Value.Enable();
      }
    }

    public void Disable()
    {
      OnDisabled();

      if (_contexts is null)
        return;

      foreach (var context in _contexts)
      {
        if (context.Value.LifecycleIsManagedByParent)
          context.Value.Disable();
      }
    }

    public void UpdateChildren()
    {
      if (_contexts is null)
        return;

      foreach (var context in _contexts)
      {
        if (context.Value.LifecycleIsManagedByParent && context.Value is IContextUpdatable updatable)
        {
          updatable.UpdateChildren();
          updatable.Update();
        }
      }
    }

    protected virtual void OnEnabled() {}

    protected virtual void OnDisabled() {}

    public virtual void Dispose()
    {
      if (_commands is not null)
      {
        foreach (var command in _commands.Values)
          command.Dispose();
      }

      if (_properties is not null)
      {
        foreach (var property in _properties.Values)
          property.Dispose();
      }

      if (_contexts is not null)
      {
        foreach (var context in _contexts.Values)
          context.Dispose();
      }
    }

    protected void RegisterProperty(Property property)
    {
      _properties ??= new();
      _properties.Add(property.Name, property);
    }

    protected void RegisterProperty(ProperyBatch propertyBatch)
    {
      foreach (var property in propertyBatch.Properties)
        RegisterProperty(property);
    }

    protected void RegisterProperties(params Property[] properties)
    {
      foreach (var item in properties)
        RegisterProperty(item);
    }

    protected void RegisterCommand(string name, Action action)
    {
      _commands ??= new();
      _commands.Add(name, new Command(action));
    }

    protected void RegisterCommand<TArg>(string name, Action<TArg> action)
    {
      _commands ??= new();
      _commands.Add(name, new Command<TArg>(action));
    }

    protected void RegisterContext(string name, IContext context)
    {
      _contexts ??= new();
      _contexts.Add(name, context);
      context.SetParent(this);
    }

    private ICommand GetCommandInternal(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath)
    {
      AssertNullOrEmpty(name);

      if (IsStartsWithParentMarker(masterPath))
      {
        if (_parent is null)
          throw new ContextHasNoParent(PathToString(name, masterPath));

        return _parent.GetCommand(name, masterPath[1..]);
      }

      if (!masterPath.IsEmpty)
      {
        var childMarkerIndex = masterPath.IndexOf(ChildContextMarker);
        if (childMarkerIndex == 0)
          throw new InvalidPropertyName(PathToString(name, masterPath), GetType());

        if (childMarkerIndex > 0)
        {
          var contextName = new string(masterPath[..childMarkerIndex]);
          AssertRegistered(_contexts, contextName);
          return _contexts[contextName].GetCommand(name, masterPath[(childMarkerIndex + 1)..]);
        }
        else
        {
          var contextName = new string(masterPath);
          AssertRegistered(_contexts, contextName);
          return _contexts[contextName].GetCommand(name, ReadOnlySpan<char>.Empty);
        }
      }

      if (IsStartsWithParentMarker(name))
      {
        if (_parent is null)
          throw new ContextHasNoParent(PathToString(name, masterPath));

        return _parent.GetCommand(name[1..], masterPath);
      }

      {
        var childMarkerIndex = name.IndexOf(ChildContextMarker);
        if (childMarkerIndex == 0)
          throw new InvalidPropertyName(PathToString(name, masterPath), GetType());

        if (childMarkerIndex > 0)
        {
          var contextName = new string(name[..childMarkerIndex]);
          AssertRegistered(_contexts, contextName);
          return _contexts[contextName].GetCommand(name[(childMarkerIndex + 1)..], masterPath);
        }

        var nameString = new string(name);
        AssertRegistered(_commands, nameString);
        return _commands[nameString];
      }
    }

    private void AssertNullOrEmpty(ReadOnlySpan<char> name)
    {
      if (name.IsEmpty)
        throw new InvalidPropertyName("<empty>", GetType());
    }

    private void AssertRegistered(IDictionary dictionary, string name)
    {
      if (dictionary is null)
        throw new InvalidPropertyName(name, GetType());
      if (!dictionary.Contains(name))
        throw new InvalidPropertyName(name, GetType());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsStartsWithParentMarker(ReadOnlySpan<char> span) => !span.IsEmpty && span[0] == ParentContextMarker;

    private string PathToString(ReadOnlySpan<char> name, ReadOnlySpan<char> masterPath)
    {
      return masterPath.IsEmpty
        ? new string(name)
        : $"{new string(masterPath)}.{new string(name)}";
    }
  }
}
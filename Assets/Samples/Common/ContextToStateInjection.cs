using System.Linq;
using JinToliq.Umvvm.ViewModel;
using JinToliq.Umvvm.ViewModel.Properties;
using UnityEngine.Assertions;

namespace Samples.Common
{
  public class SampleChildContext : Context<SampleChildContext.State>
  {
    public class State : IInjectedContextState
    {
      public IContextWithState Context { get; set; }
    }
  }

  public class SampleParentContext : Context
  {
    private readonly CollectionProperty<SampleChildContext.State> _children = new("Children");
    private readonly SampleChildContext _childContext = new();

    public SampleParentContext()
    {
      RegisterProperty(_children);
      RegisterContext("Child", _childContext);
    }

    protected override void OnEnabled()
    {
      base.OnEnabled();

      _childContext.Set(new());
      Assert.AreEqual(_childContext.CurrentState.Context, _childContext);
      var oldState = _childContext.CurrentState;
      _childContext.Set(new());
      Assert.AreEqual(_childContext.CurrentState.Context, _childContext);
      Assert.IsNull(oldState.Context);

      _children.Value = Enumerable.Range(0, 1).Select(i => new SampleChildContext.State()).ToArray();
      foreach (var child in _children.Value)
      {
        Assert.IsNotNull(child);
        Assert.IsNotNull(child.Context);
      }
    }
  }
}
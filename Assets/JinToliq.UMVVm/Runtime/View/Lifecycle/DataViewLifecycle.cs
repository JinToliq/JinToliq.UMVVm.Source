using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View.Lifecycle
{
  public abstract class DataViewLifecycle : MonoBehaviour
  {
    private IDataView _dataView;

    protected IContext Context => _dataView.Context;

    public virtual DataViewLifecycle Initialize(IDataView dataView)
    {
      _dataView = dataView ?? throw new System.ArgumentNullException(nameof(dataView), "DataView cannot be null");
      return this;
    }
  }

  public abstract class DataViewLifecycle<TContext> : DataViewLifecycle
    where TContext : IContext
  {
    protected TContext RequiredContext => (TContext)Context;

    public override DataViewLifecycle Initialize(IDataView dataView)
    {
      if (dataView.Context is not TContext)
        throw new($"Context must be {typeof(TContext).Name} to use this lifecycle");

      return base.Initialize(dataView);
    }
  }
}
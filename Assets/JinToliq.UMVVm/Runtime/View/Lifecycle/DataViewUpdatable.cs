using JinToliq.Umvvm.ViewModel;

namespace JinToliq.Umvvm.View.Lifecycle
{
  public class DataViewUpdatable : DataViewLifecycle<IContextUpdatable>
  {
    public void Update()
    {
      RequiredContext.UpdateChildren();
      RequiredContext.Update();
    }
  }
}
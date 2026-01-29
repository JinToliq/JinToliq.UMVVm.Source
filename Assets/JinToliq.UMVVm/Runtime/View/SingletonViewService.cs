using JinToliq.Umvvm.ViewModel;

namespace JinToliq.Umvvm.View
{
  public abstract class SingletonViewService<TStateId> : BaseViewService<TStateId>
  {
    protected virtual void Awake()
    {
      SingletonViewModel<TStateId>.Instance.StateOpened += OpenUi;
      SingletonViewModel<TStateId>.Instance.StateClosed += CloseUi;
    }

    protected virtual void OnDestroy()
    {
      SingletonViewModel<TStateId>.Instance.StateOpened -= OpenUi;
      SingletonViewModel<TStateId>.Instance.StateClosed -= CloseUi;
    }
  }
}
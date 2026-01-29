using System;

namespace JinToliq.Umvvm.ViewModel
{
  public class SingletonViewModel<TStateId> : BaseViewModelService<TStateId>
  {
    public event Action<UiState<TStateId>> StateOpened;
    public event Action<UiState<TStateId>> StateClosed;

    public static SingletonViewModel<TStateId> Instance { get; } = new();

    private SingletonViewModel() {}

    protected override void OnStateOpened(UiState<TStateId> state) =>
      StateOpened?.Invoke(state);

    protected override void OnStateClosed(UiState<TStateId> state) =>
      StateClosed?.Invoke(state);
  }
}
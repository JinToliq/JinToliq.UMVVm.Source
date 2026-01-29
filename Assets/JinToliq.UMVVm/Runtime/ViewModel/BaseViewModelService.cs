using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace JinToliq.Umvvm.ViewModel
{
  [Preserve]
  public abstract class BaseViewModelService<TStateId>
  {
    private readonly List<UiState<TStateId>> _states = new();

    [Preserve]
    public bool IsLastOpenedState(TStateId id) =>
      _states.Count > 0 && _states[^1].Id.Equals(id);

    [Preserve]
    public void ToggleIfLastState(TStateId di)
    {
      if (_states.Count == 0)
      {
        OpenState(di);
        return;
      }

      var last = _states[^1];
      if (!last.Id.Equals(di))
      {
        OpenState(di);
        return;
      }

      CloseState(di);
    }

    [Preserve]
    public void OpenState(TStateId id, object openWithState = null)
    {
      var state = new UiState<TStateId>(id, _states.Count > 0 ? _states.Max(s => s.Index + 1) : 0, openWithState);
      _states.Add(state);
      OnStateOpened(state);
    }

    [Preserve]
    public void CloseState(TStateId id)
    {
      if (_states.Count == 0)
        return;

      var last = _states[^1];
      if (last.Id.Equals(id))
      {
        Back();
        return;
      }

      if (_states.Count == 1)
        return;

      for (var i = _states.Count - 2; i >= 0; i--)
      {
        var state = _states[i];
        if (!state.Id.Equals(id))
          continue;

        _states.RemoveAt(i);
        OnStateClosed(state);
      }
    }

    [Preserve]
    public void Back()
    {
      if (_states.Count == 0)
        return;

      var last = _states[^1];
      _states.RemoveAt(_states.Count - 1);
      OnStateClosed(last);
    }

    protected abstract void OnStateOpened(UiState<TStateId> state);

    protected abstract void OnStateClosed(UiState<TStateId> state);
  }
}
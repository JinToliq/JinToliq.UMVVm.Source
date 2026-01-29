using System.Collections;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View
{
  public interface IUiView
  {
    UiType UiType { get; }

    public void Open();
    public void OpenWithState(object state);

    GameObject GetGameObject();
    RectTransform GetTransform();

    IEnumerator OnOpen();
    IEnumerator OnClose();
    IEnumerator OnShow();
    IEnumerator OnHide();
  }

  public interface IUiView<out TId> : IUiView
  {
    TId Id { get; }
  }

  public abstract class UiView<TId> : DataView, IUiView<TId>
  {
    [SerializeField] private UiType _uiType;

    public abstract TId Id { get; }

    public UiType UiType => _uiType;

    public abstract void Open();

    public abstract void OpenWithState(object state);

    public GameObject GetGameObject() => gameObject;

    public RectTransform GetTransform() => GetComponent<RectTransform>();

    public virtual IEnumerator OnOpen() { yield break; }

    public virtual IEnumerator OnClose() { yield break; }

    public virtual IEnumerator OnShow() { yield break; }

    public virtual IEnumerator OnHide() { yield break; }
  }

  public abstract class UiView<TId, TContext> : DataView<TContext>, IUiView<TId>
    where TContext : Context, new()
  {
    [SerializeField] private UiType _uiType;

    public abstract TId Id { get; }
    public UiType UiType => _uiType;

    public void Open() => gameObject.SetActive(true);

    public void OpenWithState(object state)
    {
      TrySetState(state);
      gameObject.SetActive(true);
    }

    public GameObject GetGameObject() => gameObject;

    public RectTransform GetTransform() => GetComponent<RectTransform>();

    public virtual IEnumerator OnOpen() { yield break; }

    public virtual IEnumerator OnClose() { yield break; }

    public virtual IEnumerator OnShow() { yield break; }

    public virtual IEnumerator OnHide() { yield break; }
  }
}
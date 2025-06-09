using System;
using System.Collections;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View
{
  public interface IUiView
  {
    UiType UiType { get; }
    Enum BaseName { get; }

    public void Open();
    public void OpenWithState(object state);

    GameObject GetGameObject();
    RectTransform GetTransform();

    IEnumerator OnOpen();
    IEnumerator OnClose();
    IEnumerator OnShow();
    IEnumerator OnHide();
  }

  public interface IUiView<out TType> : IUiView where TType : Enum
  {
    TType Name { get; }

    Enum IUiView.BaseName => Name;
  }

  public abstract class UiView<TType> : DataView, IUiView<TType> where TType : Enum
  {
    [SerializeField] private UiType _uiType;

    public abstract TType Name { get; }

    public UiType UiType => _uiType;

    public abstract void Open();

    public abstract void OpenWithState(object state);

    public GameObject GetGameObject() => gameObject;

    public RectTransform GetTransform() => GetComponent<RectTransform>();

    public IEnumerator OnOpen()
    {
      yield break;
    }

    public IEnumerator OnClose()
    {
      yield break;
    }

    public IEnumerator OnShow()
    {
      yield break;
    }

    public IEnumerator OnHide()
    {
      yield break;
    }
  }

  public abstract class UiView<TType, TContext> : DataView<TContext>, IUiView<TType>
    where TType : Enum
    where TContext : Context, new()
  {
    [SerializeField] private UiType _uiType;

    public abstract TType Name { get; }
    public UiType UiType => _uiType;

    public void Open() => gameObject.SetActive(true);

    public void OpenWithState(object state)
    {
      TrySetState(state);
      gameObject.SetActive(true);
    }

    public GameObject GetGameObject() => gameObject;

    public RectTransform GetTransform() => GetComponent<RectTransform>();

    public IEnumerator OnOpen()
    {
      yield break;
    }

    public IEnumerator OnClose()
    {
      yield break;
    }

    public IEnumerator OnShow()
    {
      yield break;
    }

    public IEnumerator OnHide()
    {
      yield break;
    }
  }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View
{
  public abstract class BaseViewManager : MonoBehaviour
  {
    private const string UiViewTypePlaceholder = "{UiViewType}";

    [SerializeField] private string _resourceSearchPattern = Path.Combine("Prefabs", "UI", UiViewTypePlaceholder, UiViewTypePlaceholder);
    [SerializeField] private Transform _uiViewsContainer;

    private readonly List<IUiView> _pool = new();
    private readonly List<(IUiView View, int Index)> _activeUi = new();

    public bool HasOpenedUi => _activeUi.Count > 0;

    public bool IsLastOpenedState(Enum type) => _activeUi.Count > 0 && _activeUi[^1].View.BaseName.Equals(type);

    public IUiView GetLastOpenedUi() => _activeUi.Count > 0 ? _activeUi[^1].View : null;

    protected abstract IUiView GetNewView(Enum type);
    protected virtual void OnUiOpened(IUiView view) {}
    protected virtual void OnUiClosed(IUiView view) {}
    protected virtual void OnUiShown(IUiView view) {}
    protected virtual void OnUiHidden(IUiView view) {}

    protected IUiView GetFromResources(Enum type)
    {
      var path = GetResourcesUiPath(type);
      var template = Resources.Load<GameObject>(path);
      if (template == null)
        throw new($"No UI prefab found by path: {Path.Combine("Resources", path)}");

      // allows to instantiate game object in inactive state and omit auto call OnEnable
      template.SetActive(false);

      var instance = Instantiate(template);
      var view = instance.GetComponent<IUiView>();
      if (view is null)
        throw new($"UI prefab by path {Path.Combine("Resources", path)} does not contain component implementing {nameof(IUiView)} interface");

      return view;
    }

    protected virtual string GetResourcesUiPath(Enum type)
    {
      if (string.IsNullOrEmpty(_resourceSearchPattern))
        throw new("ResourceSearchPattern field should be set");

      if (!_resourceSearchPattern.Contains(UiViewTypePlaceholder))
        throw new($"ResourceSearchPattern field should contain '{UiViewTypePlaceholder}' placeholder");

      var path = _resourceSearchPattern.Replace(UiViewTypePlaceholder, type.ToString(), StringComparison.OrdinalIgnoreCase);
      return Path.Combine(path.Split('\\', '/'));
    }

    public void OpenUi(UiState state)
    {
      IUiView view;
      RectTransform viewTransform;
      var pooledIndex = _pool.FindIndex(p => p.BaseName.Equals(state.Type));
      if (pooledIndex < 0)
      {
        view = GetNewView(state.Type);
        viewTransform = view.GetTransform();
        viewTransform.SetParent(_uiViewsContainer);
      }
      else
      {
        view = _pool[pooledIndex];
        viewTransform = view.GetTransform();
        _pool.RemoveAt(pooledIndex);
      }

      if (view.UiType is UiType.OpenNewWindow && _activeUi.Count > 0)
      {
        foreach (var item in _activeUi)
          StartCoroutine(DoViewRoutine(item.View, item.View.OnHide(), OnHideComplete));
      }

      FullExpandRectTransform(viewTransform);

      if (state.OpenWithState is not null)
        view.OpenWithState(state.OpenWithState);
      else
        view.Open();

      _activeUi.Add(new(view, state.Index));
      StartCoroutine(DoViewRoutine(view, view.OnOpen(), OnUiOpened));
      return;

      void FullExpandRectTransform(RectTransform rectTransform)
      {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
      }
    }

    public void CloseUi(UiState state)
    {
      var index = _activeUi.FindIndex(p => p.Index == state.Index && p.View.BaseName.Equals(state.Type));
      if (index < 0)
        return;

      var view = _activeUi[index].View;
      _activeUi.RemoveAt(index);
      _pool.Add(view);
      StartCoroutine(DoViewRoutine(view, view.OnClose(), OnCloseComplete));

      if (view.UiType is not UiType.OpenNewWindow)
        return;
      if (_activeUi.Count == 0)
        return;

      for (var i = _activeUi.Count - 1; i >= 0; i--)
      {
        var item = _activeUi[i];
        item.View.GetGameObject().SetActive(true);
        StartCoroutine(DoViewRoutine(item.View, item.View.OnShow(), OnUiShown));
        if (item.View.UiType is UiType.OpenNewWindow)
          break;
      }
    }

    private IEnumerator DoViewRoutine(IUiView view, IEnumerator routine, Action<IUiView> onEnd)
    {
      while (routine.MoveNext())
        yield return routine.Current;

      onEnd(view);
    }

    private void OnHideComplete(IUiView view)
    {
      view.GetGameObject().SetActive(false);
      OnUiHidden(view);
    }

    private void OnCloseComplete(IUiView view)
    {
      view.GetGameObject().SetActive(false);
      OnUiClosed(view);
    }
  }
}
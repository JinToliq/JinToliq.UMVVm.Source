﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View.Binding
{
  public abstract class BaseBinding : MonoBehaviour
  {
    private IDataView _view;

    protected virtual bool AlwaysActiveForChange => false;
    protected bool IsBound { get; private set; }
    protected string MasterPath { get; private set; }

    public Property GetProperty([NotNull] string property) => _view.GetProperty(property, MasterPath);

    public TProperty GetProperty<TProperty>([NotNull] string property) where TProperty : Property => _view.GetProperty<TProperty>(property, MasterPath);

    public Command GetCommand([NotNull] string property) => _view.GetCommand(property, MasterPath);

    public TCommand GetCommand<TCommand>([NotNull] string property) where TCommand : ICommand => _view.GetCommand<TCommand>(property, MasterPath);

    protected virtual void Bind() {}

    protected virtual void OnBound() {}

    protected virtual void Unbind() {}

    protected virtual void OnAwakened() {}

    protected virtual void OnEnabled() {}

    protected virtual void OnDisabled() {}

    private void Awake()
    {
      _view = GetView();
      MasterPath = GetMasterPath();
      OnAwakened();
    }

    private void OnEnable()
    {
      if (!IsBound)
      {
        Bind();
        OnBound();
        IsBound = true;
      }

      OnEnabled();
    }

    private void OnDisable()
    {
      if (!AlwaysActiveForChange)
      {
        Unbind();
        IsBound = false;
      }

      OnDisabled();
    }

    private void OnDestroy()
    {
      if (!IsBound)
        return;

      Unbind();
      IsBound = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IDataView GetView() => (GetComponent<IDataView>() ?? GetComponentInParent<IDataView>());

    private string GetMasterPath()
    {
      var viewGo = (_view as Component)!.gameObject;
      return GetMasterPathOnGameObjectOrParent(gameObject, viewGo, string.Empty);
    }

    private string GetMasterPathOnGameObjectOrParent(GameObject go, GameObject viewGo, string path)
    {
      var masterPath = go.GetComponent<MasterPathBinding>();
      var parent = go.transform.parent;
      if (masterPath != null)
      {
        masterPath.ValidatePath();
        path = string.IsNullOrEmpty(path)
          ? masterPath.Path
          : $"{masterPath.Path}.{path}";
      }

      if (go == viewGo)
        return path;

      return parent is null
        ? path
        : GetMasterPathOnGameObjectOrParent(parent.gameObject, viewGo, path);
    }
  }
}
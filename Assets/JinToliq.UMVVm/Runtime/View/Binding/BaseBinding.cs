using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View.Binding
{
  public abstract class BaseBinding : MonoBehaviour
  {
    private IDataView _view;

    protected virtual bool IsAlwaysActiveForChange => false;
    protected bool IsBound { get; private set; }
    protected string MasterPath { get; private set; }

    public Property GetProperty([NotNull] string property) => _view.GetProperty(property, MasterPath);

    public TProperty GetProperty<TProperty>([NotNull] string property) where TProperty : Property => _view.GetProperty<TProperty>(property, MasterPath);

    public Command GetCommand([NotNull] string property) => _view.GetCommand(property, MasterPath);

    public TCommand GetCommand<TCommand>([NotNull] string property) where TCommand : ICommand => _view.GetCommand<TCommand>(property, MasterPath);

    protected virtual void Bind() {}

    protected virtual void OnBound() {}

    protected virtual void Unbind() {}

    protected virtual void OnUnbound() {}

    protected virtual void OnAwakened() {}

    protected virtual void OnEnabled() {}

    protected virtual void OnDisabled(bool wasBound) {}

    protected virtual void OnDestroyed() {}

    private void Awake()
    {
      _view = GetView();
      MasterPath = GetMasterPath();
      OnAwakened();

      if (IsAlwaysActiveForChange)
        BindInternal();
    }

    private void OnEnable()
    {
      if (!IsAlwaysActiveForChange)
        BindInternal();

      OnEnabled();
    }

    private void OnDisable()
    {
      var wasBound = IsBound;
      if (!IsAlwaysActiveForChange)
        UnbindInternal();

      OnDisabled(wasBound);
    }

    private void OnDestroy()
    {
      UnbindInternal();
      OnDestroyed();
    }

    private void BindInternal()
    {
      if (IsBound)
        return;

      Bind();
      IsBound = true;
      OnBound();
    }

    private void UnbindInternal()
    {
      if (!IsBound)
        return;

      Unbind();
      IsBound = false;
      OnUnbound();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IDataView GetView() =>
      GetComponent<IDataView>() ?? GetComponentInParent<IDataView>();

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
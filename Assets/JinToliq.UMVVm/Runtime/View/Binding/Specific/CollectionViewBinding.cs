using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View.Binding.Specific
{
  public class CollectionViewBinding : CollectionPropertyBinding
  {
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private bool _clearChildrenOnAwake = true;
    private readonly List<DataView> _instances = new();

    private readonly List<object> _collection = new();

    protected IReadOnlyList<object> Collection => _collection;
    protected virtual IEnumerable<object> ValidCollection => Collection;

    protected override void OnAwakened()
    {
      base.OnAwakened();
      if (!_clearChildrenOnAwake)
        return;

      var childCount = _parent.childCount;
      for (var i = childCount - 1; i >= 0; i--)
      {
        var child = _parent.GetChild(i);
        Destroy(child.gameObject);
      }
    }

    protected override void OnChanged(Property property)
    {
      _collection.Clear();

      var value = property.GetValue();
      if (value is not ICollection collection || collection.Count == 0)
      {
        foreach (var instance in _instances)
          OnScenePool.Instance.Recycle(instance);

        _instances.Clear();
        return;
      }

      foreach (var instance in collection)
        _collection.Add(instance);

      OnCollectionUpdated();

      var newCollection = ValidCollection;
      var newCollectionList = newCollection as IList ?? newCollection.ToArray();

      var newCount = newCollectionList.Count;
      var oldCount = _instances.Count;

      if (newCount < oldCount)
      {
        for (var i = oldCount - 1; i >= newCount; i--)
        {
          var instance = _instances[i];

          OnScenePool.Instance.Recycle(instance);
          _instances.RemoveAt(i);
        }
      }
      else if (newCount > oldCount)
      {
        for (var i = oldCount; i < newCount; i++)
        {
          var instance = OnScenePool.Instance.Get<DataView>(_prefab, _parent);
          if (instance == null)
          {
            Debug.LogError($"Failed to instantiate prefab {_prefab.name} or find root component of type {typeof(DataView)} when instantiating views for collection binding");
            continue;
          }

          instance.gameObject.SetActive(true);
          _instances.Add(instance);
        }
      }

      for (var i = 0; i < newCount; i++)
        _instances[i].SetState(newCollectionList[i]);
    }

    protected virtual void OnCollectionUpdated() { }

    private void Reset() =>
      _parent = transform;
  }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JinToliq.Umvvm.View
{
  public class UiPool : MonoBehaviour
  {
    private class Bucket
    {
      public readonly GameObject Original;
      public readonly Stack<GameObject> Pooled;
      public readonly HashSet<GameObject> Used;

      public Bucket(GameObject original)
      {
        Original = original;
        Pooled = new();
        Used = new();
      }
    }

    public static UiPool Instance => LazyInstance.Value;
    private static readonly Lazy<UiPool> LazyInstance = new(Factory);
    private readonly Dictionary<string, Bucket> _registry = new();
    private readonly Dictionary<GameObject, Bucket> _used = new();
    private readonly List<GameObject> _returnList = new();

    private static UiPool Factory()
    {
      var result = new GameObject("UiPool").AddComponent<UiPool>();
      DontDestroyOnLoad(result.gameObject);
      return result;
    }

    private void Awake() =>
      SceneManager.sceneUnloaded += OnSceneUnloaded;

    private void OnDestroy() =>
      SceneManager.sceneUnloaded -= OnSceneUnloaded;

    public void EnsureRegistered(GameObject original, int count = 1)
    {
      if (!IsRegistered(original.name))
        Register(original.name, original, count);
    }

    public bool IsRegistered(GameObject original) => IsRegistered(original.name);

    public bool IsRegistered(string key) => _registry.ContainsKey(key);

    public void Register(GameObject original, int count = 1, bool additive = false)
    {
      Register(original.name, original, count, additive);
    }

    public void Register(string key, GameObject original, int count = 1, bool additive = false)
    {
      RegisterKeyIfNeeded(key, original);

      if (!additive && _registry[key].Pooled.Count >= count)
        return;

      var pool = _registry[key].Pooled;
      for (var i = 0; i < count; i++)
      {
        var instance = Instantiate(original, transform);
        instance.name = $"{key} ({pool.Count + _registry[key].Used.Count})";
        instance.SetActive(false);
        pool.Push(instance);
      }
    }

    public T Get<T>(GameObject original) where T : Component
    {
      EnsureRegistered(original);
      return Get<T>(original.name);
    }

    public T Get<T>(GameObject original, Transform parent) where T : Component
    {
      EnsureRegistered(original);
      return Get<T>(original.name, parent);
    }

    public GameObject Get(GameObject original, Transform parent)
    {
      EnsureRegistered(original);
      return Get(original.name, parent);
    }

    public GameObject Get(GameObject original)
    {
      EnsureRegistered(original);
      return Get(original.name, original);
    }

    public T Get<T>(string key) where T : Component
    {
      var go = GetInternal(key, null);
      return go.GetComponent<T>();
    }

    public T Get<T>(string key, Transform parent) where T : Component
    {
      var go = Get(key, parent);
      return go.GetComponent<T>();
    }

    public GameObject Get(string key, Transform parent)
    {
      var go = GetInternal(key, null);
      var trans = go.transform;
      trans.SetParent(parent, true);
      trans.localScale = _registry[key].Original.transform.localScale;
      return go;
    }

    public GameObject Get(string key, GameObject original = null) =>
      GetInternal(key, original);

    public void Recycle(Component instance) =>
      RecycleInternal(instance.gameObject);

    public void Recycle(Transform instance) =>
      RecycleInternal(instance.gameObject);

    public void Recycle(GameObject instance) =>
      RecycleInternal(instance);

    private void LateUpdate()
    {
      if (_registry.Count == 0)
        return;

      foreach (var item in _returnList)
      {
        if (!_used.Remove(item, out var bucket))
        {
          Debug.LogWarning("Recycling an unused GameObject that was not found");
          continue;
        }

        if (!bucket.Used.Remove(item))
        {
          Debug.LogWarning("Recycling an unused GameObject that was not found in the bucket");
          continue;
        }

        bucket.Pooled.Push(item);
        item.transform.SetParent(transform, true);
      }
    }

    private GameObject GetInternal(string key, GameObject original)
    {
      RegisterKeyIfNeeded(key, original);

      var bucket = _registry[key];
      if (!bucket.Pooled.TryPop(out var instance))
      {
        var originalActive = bucket.Original.gameObject.activeSelf;
        bucket.Original.gameObject.SetActive(false);
        instance = Instantiate(bucket.Original, transform);
        bucket.Original.gameObject.SetActive(originalActive);
        var id = bucket.Pooled.Count + bucket.Used.Count;
        instance.name = $"{key} ({id.ToString()})";
        instance.SetActive(false);

        if (instance.gameObject.activeSelf)
        {
          Debug.LogWarning("Instantiated an enabled GameObject what was not expected");
          instance.SetActive(false);
        }
      }

      _used.Add(instance, bucket);
      bucket.Used.Add(instance);
      return instance;
    }

    private void RecycleInternal(GameObject instance)
    {
      if (instance == null)
        return;

      instance.SetActive(false);
      _returnList.Add(instance);
    }

    private void RegisterKeyIfNeeded(string key, GameObject original)
    {
      if (!_registry.ContainsKey(key))
        _registry.Add(key, new(original));
    }

    private void OnSceneUnloaded(Scene scene)
    {
      foreach (var bucket in _registry.Values)
      {
        foreach (var item in bucket.Pooled)
          Destroy(item);

        bucket.Pooled.Clear();
      }
    }
  }
}
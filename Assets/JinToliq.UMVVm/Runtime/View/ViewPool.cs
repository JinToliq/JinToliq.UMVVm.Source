using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace JinToliq.Umvvm.View
{
  public static class ViewPool
  {
    private static readonly Transform Parent;
    private static readonly Dictionary<GameObject, ObjectPool<GameObject>> Pools = new();

    static ViewPool()
    {
      var parentObject = new GameObject("UmvvmViewPool");
      Object.DontDestroyOnLoad(parentObject);
      Parent = parentObject.transform;
      SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene arg)
    {
      foreach (var pool in Pools.Values)
        pool.Clear();
    }

    public static void Recycle<T>(GameObject prefab, T instance) where T : Component =>
      GetPool(prefab).Release(instance.gameObject);

    public static void Recycle(GameObject prefab, GameObject instance) =>
      GetPool(prefab).Release(instance);

    public static T Get<T>(GameObject prefab) where T : Component =>
      GetPool(prefab).Get().GetComponent<T>();

    public static T Get<T>(GameObject prefab, Transform parent) where T : Component
    {
      var gObject = GetPool(prefab).Get();
      gObject.transform.SetParent(parent, true);
      return gObject.GetComponent<T>();
    }

    public static GameObject Get(GameObject prefab, Transform parent)
    {
      var gObject = GetPool(prefab).Get();
      gObject.transform.SetParent(parent, true);
      return gObject;
    }

    public static GameObject Get(GameObject prefab) =>
      GetPool(prefab).Get();

    private static ObjectPool<GameObject> GetPool(GameObject prefab)
    {
      if (Pools.TryGetValue(prefab, out var pool))
        return pool;

      Pools[prefab] = pool = new(
        () =>
        {
          prefab.SetActive(false);
          var instance = Object.Instantiate(prefab);
          prefab.SetActive(true);
          return instance;
        },
        null,
        gObject =>
        {
          gObject.SetActive(false);
          gObject.transform.SetParent(Parent, true);
        },
        gObject => Object.Destroy(gObject),
        true, 8, 16);

      return pool;
    }
  }
}
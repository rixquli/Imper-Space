using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
  private static PoolManager instance;

  public static PoolManager Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindFirstObjectByType<PoolManager>();
        if (instance == null)
        {
          GameObject singletonObject = new GameObject();
          instance = singletonObject.AddComponent<PoolManager>();
          singletonObject.name = "PoolManager (Singleton)";
          DontDestroyOnLoad(singletonObject);
        }
      }
      return instance;
    }
  }

  private Dictionary<string, Queue<GameObject>> poolDictionary;

  private void Awake()
  {
    instance = this;
    poolDictionary = new Dictionary<string, Queue<GameObject>>();
  }

  public void CreatePool(string tag, GameObject prefab, int size)
  {
    if (!poolDictionary.ContainsKey(tag))
    {
      poolDictionary[tag] = new Queue<GameObject>();

      for (int i = 0; i < size; i++)
      {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
      }
    }
  }

  public GameObject GetFromPool(string tag)
  {
    if (poolDictionary.ContainsKey(tag) && poolDictionary[tag].Count > 0)
    {
      GameObject obj = poolDictionary[tag].Dequeue();
      obj.SetActive(true);
      return obj;
    }
    return null;
  }

  public GameObject GetFromPool(string tag, Vector2 position)
  {
    if (poolDictionary.ContainsKey(tag) && poolDictionary[tag].Count > 0)
    {
      GameObject obj = poolDictionary[tag].Dequeue();
      obj.SetActive(true);
      obj.transform.position = position;
      return obj;
    }
    return null;
  }

  public void ReturnToPool(string tag, GameObject obj)
  {
    obj.SetActive(false);
    if (poolDictionary.ContainsKey(tag))
    {
      poolDictionary[tag].Enqueue(obj);
    }
  }
}
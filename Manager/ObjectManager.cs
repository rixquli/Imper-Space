using UnityEngine;
using System;

public class ObjectManager : MonoBehaviour
{
  public static ObjectManager Instance;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
    }
    else
    {
      Instance = this;
    }
  }

  public void SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation, Action<GameObject> OnInitialized = null)
  {
    Debug.Log(prefab.name);
    // Spawner directement si en mode solo ou si c'est le serveur
    SpawnObjectInternal(prefab, position, rotation, OnInitialized);
  }

  private void SpawnObjectInternal(GameObject prefab, Vector3 position, Quaternion rotation, Action<GameObject> OnInitialized = null)
  {
    Debug.Log(prefab.name);
    GameObject newObject = Instantiate(prefab, position, rotation);

    OnInitialized?.Invoke(newObject);

  }
}
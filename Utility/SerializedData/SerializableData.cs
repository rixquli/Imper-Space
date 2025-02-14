using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableData<T> : ISerializationCallbackReceiver where T : SerializableScriptableObject
{
  public string pid;

  [SerializeField]
  private T data;

  public T Data
  {
    get { return data; }
    set
    {
      data = value;
      if (data != null)
      {
        pid = data.UNIQUEID;
      }
    }
  }

  public T Value => Data;

  public SerializableData(T value)
  {
    data = value;
    if (data != null)
    {
      pid = data.UNIQUEID;
    }
  }

  public void OnBeforeSerialize()
  {
    // No need to do anything here since we only store the PID
  }

  public void OnAfterDeserialize()
  {
    if(string.IsNullOrEmpty(pid))
    {
      Debug.LogWarning($"after PID is null or empty, type: {typeof(T)}, data: {data}, pid: {pid}");
    }
    if (ItemDatabase.Instance != null && pid != null)
    {
      ItemDatabase.Instance.GetSerializableScriptableObjectById(pid, (scriptableObject) =>
      {
        data = scriptableObject as T;
      });
    }
  }

  // Conversion implicite Wrapper<T> -> T
  public static implicit operator T(SerializableData<T> wrapper) => wrapper.data;

  // Conversion implicite T -> Wrapper<T>
  public static implicit operator SerializableData<T>(T value) => new SerializableData<T>(value);

  // Méthode statique pour convertir SerializableData<T>[] en T[]
  public static T[] ToArray(SerializableData<T>[] wrappers)
  {
    T[] array = new T[wrappers.Length];
    for (int i = 0; i < wrappers.Length; i++)
    {
      array[i] = wrappers[i].data;
    }
    return array;
  }
}
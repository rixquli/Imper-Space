using UnityEditor;
using UnityEngine;
using System;

[Serializable]
public class SerializableScriptableObject : ScriptableObject
{
    [SerializeField]
    private string uniqueID;

    public string UNIQUEID => uniqueID;

    public ScriptableObjectReference GetReference()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.LogError($"UniqueID is null or empty for {name}. Make sure it is properly initialized.");
            return null;
        }
        return new ScriptableObjectReference(uniqueID);
    }

    [ContextMenu("Force reset ID")]
    private void ResetId()
    {
        uniqueID = Guid.NewGuid().ToString();
        Debug.Log("Setting new ID on object: " + uniqueID);
    }

    protected virtual void OnValidate()
    {
#if UNITY_EDITOR
        // Si le PID est vide ou nul, récupère le PID existant dans l'AssetDatabase
        if (uniqueID == null || uniqueID == "")
        {
            uniqueID = Guid.NewGuid().ToString();
        }
#endif
    }
}

[System.Serializable]
public class ScriptableObjectReference
{
    public string uniqueID;

    public ScriptableObjectReference(string id)
    {
        uniqueID = id;
    }

    public T GetScriptableObject<T>() where T : class
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            return null;
        }

        object scriptableObject = null;

        // Assuming you have a method in ItemDatabase to get the object by ID
        ItemDatabase.Instance.GetSerializableScriptableObjectById(uniqueID, (so) =>
        {
            scriptableObject = so;
        });

        return scriptableObject as T;
    }
}

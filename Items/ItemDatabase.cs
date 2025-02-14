using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    public Dictionary<int, ItemData> itemDataDictionary { get; private set; } = new();
    public Dictionary<int, GadgetsData> gadgetDataDictionary { get; private set; } = new();
    public Dictionary<int, EquipementData> equipementDataDictionary { get; private set; } = new();

    [SerializeField]
    private List<ItemData> itemDataList = new(); // List of items to populate the dictionary
    [SerializeField]
    private List<GadgetsData> gadgetDataList = new(); // List of items to populate the dictionary
    [SerializeField]
    private List<EquipementData> equipementDataList = new(); // List of items to populate the dictionary

    public List<TroopsData> troops = new();

    public GameObject prefab;

    public UDictionary<string, object> serializableScriptableObjects = new();

    public bool loadingFinished { get; private set; } = false;
    private List<Action> deferredActions = new List<Action>();

    public UDictionary<RessourceType, Sprite> ressourceSprites = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        PopulateDictionary(itemDataDictionary, itemDataList, item => item.itemId);
        PopulateDictionary(gadgetDataDictionary, gadgetDataList, gadget => gadget.gadgetId);

        int i = 0;
        EquipementData[] equipements = Resources.LoadAll<EquipementData>("Equipements/");
        foreach (EquipementData equipement in equipements)
        {
            if (!equipementDataDictionary.ContainsValue(equipement))
            {
                equipementDataDictionary.Add(i, equipement);
            }
            else
            {
                Debug.LogWarning($"Equipement already in the database: {equipement}");
            }
            i++;
        }

        foreach (SerializableScriptableObject sObject in Resources.LoadAll<SerializableScriptableObject>(""))
        {
            if (!serializableScriptableObjects.ContainsKey(sObject.UNIQUEID))
            {
                serializableScriptableObjects.Add(sObject.UNIQUEID, sObject);
            }
            else
            {
                Debug.LogWarning($"Item with ID {sObject.UNIQUEID} is already in the database.");
            }
        }

        loadingFinished = true;

        foreach (var action in deferredActions)
        {
            action();
        }
        deferredActions.Clear();
    }

    public Sprite GetRessourceSprite(RessourceType ressourceType)
    {
        if (ressourceSprites.TryGetValue(ressourceType, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogError($"Sprite for ressource type {ressourceType} not found in the database.");
            return null;
        }
    }

    public ItemData GetItemById(int itemId)
    {
        if (itemDataDictionary.TryGetValue(itemId, out ItemData itemData))
        {
            return itemData;
        }
        else
        {
            Debug.LogError($"Item with ID {itemId} not found in the database.");
            return null;
        }
    }
    public GadgetsData GetGadgetById(int gadgetId)
    {
        if (gadgetDataDictionary.TryGetValue(gadgetId, out GadgetsData gadgetsData))
        {
            return gadgetsData;
        }
        else
        {
            Debug.LogError($"Item with ID {gadgetId} not found in the database.");
            return null;
        }
    }

    public void GetSerializableScriptableObjectById(string pid, Action<object> callback)
    {
        if (string.IsNullOrEmpty(pid))
        {
            Debug.LogError("Provided ID is null or empty.");
            callback?.Invoke(null);
            return;
        }

        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback), "Callback cannot be null.");
        }

        if (loadingFinished)
        {
            var obj = GetSerializableScriptableObjectByIdSync(pid);
            callback(obj);
        }
        else
        {
            deferredActions.Add(() => GetSerializableScriptableObjectById(pid, callback));
        }
    }


    public object GetSerializableScriptableObjectByIdSync(string pid)
    {
        if (string.IsNullOrEmpty(pid))
        {
            Debug.LogError("Provided ID is null or empty.");
            return null;
        }

        if (serializableScriptableObjects.TryGetValue(pid, out object serializableScriptableObject) && serializableScriptableObject != null)
        {
            return serializableScriptableObject;
        }
        else
        {
            Debug.LogError($"Item with ID {pid} not found in the database.");
            return null;
        }
    }

    private void PopulateDictionary<T>(Dictionary<int, T> dictionary, List<T> dataList, Func<T, int> idSelector)
    {
        foreach (var item in dataList)
        {
            int id = idSelector(item);
            if (!dictionary.ContainsKey(id))
            {
                dictionary.Add(id, item);
            }
            else
            {
                Debug.LogWarning($"Item with ID {id} is already in the database.");
            }
        }
    }
}

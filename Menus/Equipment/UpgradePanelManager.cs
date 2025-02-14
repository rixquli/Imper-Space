using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject itemPrefab;

    private List<EquipementsAndAmount> equipements;
    private UDictionary<EquipeSlot, EquipementData> equipedEquipements;

    private List<GameObject> itemPool = new List<GameObject>();

    private void Awake()
    {
        RessourcesManager.Instance.OnValueChanged += OnValueChanged;
    }

    private void Start()
    {
        equipements = new List<EquipementsAndAmount>(RessourcesManager.Instance.equipements);
        equipedEquipements = new UDictionary<EquipeSlot, EquipementData>();
        foreach (var kvp in RessourcesManager.Instance.equipedEquipements)
        {
            equipedEquipements.Add(kvp.Key, kvp.Value);
        }

        CreateItems();
    }

    private void OnDestroy()
    {
        RessourcesManager.Instance.OnValueChanged -= OnValueChanged;
    }

    private void OnValueChanged()
    {
        var newEquipements = RessourcesManager.Instance.equipements;
        var newEquipedEquipements = RessourcesManager.Instance.equipedEquipements;
        if (!UtilsClass.AreListsEqual(newEquipements, equipements) || !UtilsClass.AreDictionariesEqual(newEquipedEquipements, equipedEquipements))
        {
            equipements = new List<EquipementsAndAmount>(newEquipements);
            equipedEquipements = new UDictionary<EquipeSlot, EquipementData>();
            foreach (var kvp in newEquipedEquipements)
            {
                equipedEquipements.Add(kvp.Key, kvp.Value);
            }
            CreateItems();
        }
    }

    private void CreateItems()
    {
        // Désactiver tous les objets du pool
        foreach (var item in itemPool)
        {
            item.SetActive(false);
        }

        int poolIndex = 0;
        foreach (EquipementsAndAmount item in equipements)
        {
            if (!RessourcesManager.Instance.equipedEquipements.Values.ToList().Contains(item.data))
            {
                continue;
            }

            GameObject itemObject;
            if (poolIndex < itemPool.Count)
            {
                itemObject = itemPool[poolIndex];
                itemObject.SetActive(true);
            }
            else
            {
                itemObject = Instantiate(itemPrefab, transform.GetChild(0));
                itemPool.Add(itemObject);
            }

            UpgradePanelItem upgradePanelItem = itemObject.GetComponent<UpgradePanelItem>();
            if (upgradePanelItem != null)
            {
                upgradePanelItem.Initialize(item);
            }

            poolIndex++;
        }

        Invoke(nameof(RefreshLayoutGroup), 0.01f);
    }

    private void UpdateItems()
    {
        Transform parentTransform = transform.GetChild(0);
        int childCount = parentTransform.childCount;

        for (int i = 0; i < equipements.Count; i++)
        {
            if (
                !RessourcesManager
                    .Instance.equipedEquipements.Values.ToList()
                    .Contains(equipements[i].data)
            )
            {
                continue;
            }
            if (i < childCount)
            {
                GameObject itemObject = parentTransform.GetChild(i).gameObject;

                UpgradePanelItem upgradePanelItem = itemObject.GetComponent<UpgradePanelItem>();
                if (upgradePanelItem != null)
                {
                    upgradePanelItem.Initialize(equipements[i]);
                }
            }
            else
            {
                // Handle the case where the child does not exist
                Debug.LogWarning(
                    $"No child at index {i} in {parentTransform.name}. Ensure the hierarchy matches the number of equipements."
                );
            }
        }

        Invoke(nameof(RefreshLayoutGroup), 0.001f);
    }

    private void RefreshLayoutGroup()
    {
        HorizontalOrVerticalLayoutGroup verticalLayoutGroup = transform
            .GetChild(0)
            .GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            verticalLayoutGroup.spacing = verticalLayoutGroup.spacing > 5 ? 5 : 5.1f;
        }
    }

    private void DeleteChildObjects(GameObject parentObject)
    {
        if (parentObject != null && parentObject.transform.childCount > 0)
        {
            // Get the first child
            foreach (Transform child in parentObject.transform)
            {
                Destroy(child.gameObject);
            }
            // Destroy the first child GameObject
        }
    }
}

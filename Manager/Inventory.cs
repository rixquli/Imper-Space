using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GadgetsStoreValue
{
    public GadgetsData data;
    public int level;


    public GadgetsStoreValue(GadgetsData gadget, int level)
    {
        this.data = gadget;
        this.level = level;
    }
}

[System.Serializable]
public struct ItemsAndAmount
{
    public ItemData data;
    public int amount;
    public ItemsAndAmount(ItemData item, int amount)
    {
        this.data = item;
        this.amount = amount;
    }
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;


    public List<ItemsAndAmount> items = new List<ItemsAndAmount>();
    public List<GadgetsStoreValue> gadgets = new List<GadgetsStoreValue>();
    public double goldAmount = 0;
    public double gemAmount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(double amount)
    {
        RessourcesManager.Instance.AddGold(amount);
        goldAmount += amount;
    }
    public void AddGems(double amount)
    {
        RessourcesManager.Instance.AddGems(amount);
        gemAmount += amount;
    }

    public void Add(ItemData item, int amount = 1, Action callback = null)
    {
        if (item.itemId == 0)// Gold
        {
            RessourcesManager.Instance.AddGold(amount);
            goldAmount += amount;
        }
        else if (item.itemId == 1)// Gem
        {
            RessourcesManager.Instance.AddGems(amount);
            gemAmount += amount;
        }
        else
        {
            // Check if the item already exists in the inventory
            bool itemExists = false;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].data == item)
                {
                    items[i] = new ItemsAndAmount(item, items[i].amount + amount);
                    itemExists = true;
                    break;
                }
            }

            // If the item does not exist, add it as a new entry
            if (!itemExists)
            {
                items.Add(new ItemsAndAmount(item, amount));
            }
        }

        callback?.Invoke();
        UIController.Instance.AddNotifNewItem(item);
    }

    public void AddGadgets(GadgetsData gadget, int level)
    {

        gadgets.Add(new GadgetsStoreValue(gadget, level));

    }
    public void RemoveGadgets(int gadgetId, int level)
    {
        for (int i = 0; i < gadgets.Count; i++)
        {
            if (gadgets[i].data.gadgetId == gadgetId && gadgets[i].level == level)
            {
                gadgets.RemoveAt(i);
                break;
            }
        }
    }

}

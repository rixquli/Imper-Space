using System.Collections.Generic;
using UnityEngine;

public class ButtonNotifManager : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    private GameObject notifBattlePassObject;
    [SerializeField]
    private GameObject notifOnShopButtonObject;
    [SerializeField]
    private GameObject notifItemInShopObject;
    [SerializeField]
    private GameObject notifFreePullingButtonObject;

    private void Awake()
    {
        notifBattlePassObject.SetActive(false);
    }

    private void Start()
    {
        RessourcesManager.Instance.OnValueChanged += OnValueChanged;
    }

    private void TestNotifForBattlePass(List<BattlePassLevels> battlePassLevels, bool hasBattlePass)
    {
        if (notifBattlePassObject == null)
            return;

        // Si un niveau de battle pass n'a pas été recuperé alors on affiche la notification
        foreach (var level in battlePassLevels)
        {
            if (
                level.hasReached
                && (!level.hasClaimedFreeCard || (hasBattlePass && !level.hasClaimedGoldCard))
            )
            {
                notifBattlePassObject.SetActive(true);
                return;
            }
        }
        notifBattlePassObject.SetActive(false);
    }

    private void TestNotifForFreePullingInShop(int watchAmountForFreePulling)
    {
        if (notifItemInShopObject == null || notifFreePullingButtonObject == null || notifOnShopButtonObject == null)
            return;

        // Si un item n'a pas été acheté alors on affiche la notification
        if (RessourcesManager.Instance.watchPerDay.watchAmountForFreePulling < RessourcesManager.Instance.watchPerDay.maxWatchAmountForFreePulling)
        {
            notifOnShopButtonObject.SetActive(true);
            notifItemInShopObject.SetActive(true);
            notifFreePullingButtonObject.SetActive(true);
            return;
        }
        notifOnShopButtonObject.SetActive(false);
        notifItemInShopObject.SetActive(false);
        notifFreePullingButtonObject.SetActive(false);
    }

    private void OnValueChanged()
    {
        TestNotifForBattlePass(
            DataPersistenceManager.instance.gameData.battlePassLevels,
            DataPersistenceManager.instance.gameData.hasGoldPass
        );
        TestNotifForFreePullingInShop(RessourcesManager.Instance.watchPerDay.watchAmountForFreePulling);
    }

    public void LoadData(GameData data)
    {
        TestNotifForBattlePass(data.battlePassLevels, data.hasGoldPass);
        TestNotifForFreePullingInShop(data.watchPerDay.watchAmountForFreePulling);
    }

    public void SaveData(GameData data) { }
}

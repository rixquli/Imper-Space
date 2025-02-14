using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitPanelController : MonoBehaviour
{
    // Buttons for open and close
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private GameObject troopCardPrefab;
    [SerializeField] private GameObject troopCardsListParent;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.SetActive(false);
        openButton.onClick.AddListener(Show);
        closeButton.onClick.AddListener(Hide);
    }

    private void UpdateGadgetsListPanel()
    {
        BuildingList[] buildingLists = { };
        if (VillageRessourcesManager.Instance != null)
        {
            buildingLists = VillageRessourcesManager.Instance.buildingList;
        }
        else if (RessourcesManager.Instance != null)
        {
            buildingLists = RessourcesManager.Instance.buildingList;
        }

        if (troopCardsListParent.transform.childCount > 0)
        {
            foreach (Transform item in troopCardsListParent.transform)
            {
                Destroy(item.gameObject);
            }
        }

        foreach (TroopsData troopData in ItemDatabase.Instance.troops)
        {
            TroopsToPlace[] troopsToPlaces = VillageRessourcesManager.Instance.troopsToPlaces;
            TroopsToPlace troop = Array.Find(troopsToPlaces, x => x.troopsData == troopData);

            if (troop == null)
            {
                // Crée un nouveau tableau avec une taille augmentée de 1
                TroopsToPlace[] newTroopsArray = new TroopsToPlace[troopsToPlaces.Length + 1];
                Array.Copy(troopsToPlaces, newTroopsArray, troopsToPlaces.Length);

                // Ajoute le nouveau troop
                troop = new TroopsToPlace(troopData, 0);
                newTroopsArray[newTroopsArray.Length - 1] = troop;

                // Remplace l'ancien tableau par le nouveau
                VillageRessourcesManager.Instance.troopsToPlaces = newTroopsArray;
            }

            var troopIntance = Instantiate(troopCardPrefab, troopCardsListParent.transform);
            if (troopIntance == null || troop.troopsData == null) continue;

            int maxAmount = VillageRessourcesManager.Instance.GetMaxAmount(troop.troopsData);

            TextMeshProUGUI name = troopIntance.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            if (name != null)
            {
                name.text = $"{troop.troopsData.displayName}";
            }

            TextMeshProUGUI count = troopIntance.transform.Find("Count").GetComponent<TextMeshProUGUI>();
            if (count != null)
            {
                count.text = $"{troop.amount}/{maxAmount}";
            }

            Image image = troopIntance.transform.Find("Image").GetComponent<Image>();
            if (image != null)
            {
                image.sprite = troop.troopsData.image;
            }

            Button confirmButton = troopIntance.transform.Find("Buttons/Confirm/Button").GetComponent<Button>();
            TextMeshProUGUI cornfirmText = troopIntance.transform.Find("Buttons/Confirm/Button/Text").GetComponent<TextMeshProUGUI>();
            Button watchAdButton = troopIntance.transform.Find("Buttons/WatchAd/Button").GetComponent<Button>();
            TextMeshProUGUI adText = troopIntance.transform.Find("Buttons/WatchAd/Button/Text").GetComponent<TextMeshProUGUI>();

            confirmButton.onClick.RemoveAllListeners();
            watchAdButton.onClick.RemoveAllListeners();

            cornfirmText.text = troop.troopsData.troopCost.ToString();
            // affiche le nombre de pub qu'il a regardé pour ce bâtiment sur 2
            adText.text = $"WATCH AD";

            confirmButton.onClick.AddListener(() =>
                    {
                        if (troop.amount + 1 > maxAmount) return;

                        RessourcesManager.Instance.Buy(PaymantSubtype.Gold, troop.troopsData.troopCost, () =>
                        {
                            VillageRessourcesManager.Instance.AddTroop(troop.troopsData, 1);
                            UpdateGadgetsListPanel();
                        }, () =>
                        {
                            Debug.LogWarning("Error during upgrade");
                        });
                    });

            watchAdButton.onClick.AddListener(() =>
                    {
                        AdmobManager.Instance.ShowRewardedAd((rewardAd) =>
                        {
                            VillageRessourcesManager.Instance.AddTroop(troop.troopsData, 1);
                            UpdateGadgetsListPanel();
                        });
                    });
        }
    }

    public void Show()
    {
        UpdateGadgetsListPanel();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.TroopPanelClosed();
    }

}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameShopManager : MonoBehaviour
{
    public static IngameShopManager Instance { get; private set; }

    [SerializeField]
    private Button openButton;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private Button gadgetButton;

    [SerializeField]
    private Image gadgetButtonImage;

    [SerializeField]
    private GameObject gadgetPanel;

    [SerializeField]
    private Sprite selectedButtonSprite;

    [SerializeField]
    private Sprite unselectedButtonSprite;

    [SerializeField]
    private GameObject gadgetPrefab;

    [SerializeField]
    private GameObject gadgetsListParent;

    [SerializeField]
    private UDictionary<GadgetsData, int> adWatchedPerBuilding = new();

    [Header("Gadgets Shop")]
    [SerializeField]
    private BuildingList[] gadgetsShopList;

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

        gameObject.SetActive(false);
        if (openButton != null)
            openButton.onClick.AddListener(ShowPanel);
        closeButton.onClick.AddListener(HidePanel);

        gadgetButton.onClick.AddListener(OpenGadgetPanel);

        OpenGadgetPanel();
        HidePanel();
    }

    private void Start()
    {
        UpdateGadgetsListPanel();
    }

    public void HidePanel()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    private void OpenGadgetPanel()
    {
        gadgetPanel.SetActive(true);
        gadgetButtonImage.sprite = selectedButtonSprite;

        UpdateGadgetsListPanel();
    }

    public void ShowPanel()
    {
        if (VillageMovingManager.Instance.isMoving) return;
        UpdateGadgetsListPanel();
        gameObject.SetActive(true);
        Time.timeScale = 0;
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

        if (gadgetsListParent.transform.childCount > 0)
        {
            foreach (Transform item in gadgetsListParent.transform)
            {
                Destroy(item.gameObject);
            }
        }

        foreach (BuildingList gadget in buildingLists)
        {
            // si gadget est l'HDV avec l'id 5 alors continue au prochain batiment
            if (gadget.gadgetsData.gadgetId == 5)
                continue;

            if (!adWatchedPerBuilding.ContainsKey(gadget.gadgetsData))
                adWatchedPerBuilding.Add(gadget.gadgetsData, 0);

            var gadgetIntance = Instantiate(gadgetPrefab, gadgetsListParent.transform);
            if (gadgetIntance == null || gadget.gadgetsData == null)
                continue;

            TextMeshProUGUI name = gadgetIntance
                .transform.Find("Name")
                .GetComponent<TextMeshProUGUI>();
            if (name != null)
            {
                name.text = $"{gadget.gadgetsData.displayName}";
            }

            TextMeshProUGUI count = gadgetIntance
                .transform.Find("Count")
                .GetComponent<TextMeshProUGUI>();
            if (count != null)
            {
                count.text = $"{gadget.currentPlacedBuilding}/{VillageRessourcesManager.Instance.GetMaxAmount(gadget.gadgetsData)}";
            }

            Image image = gadgetIntance.transform.Find("Image").GetComponent<Image>();
            if (image != null)
            {
                image.sprite = gadget.gadgetsData.visual;
            }

            Button confirmButton = gadgetIntance
                .transform.Find("Buttons/Confirm/Button")
                .GetComponent<Button>();
            TextMeshProUGUI cornfirmText = gadgetIntance
                .transform.Find("Buttons/Confirm/Button/Text")
                .GetComponent<TextMeshProUGUI>();
            Button watchAdButton = gadgetIntance
                .transform.Find("Buttons/WatchAd/Button")
                .GetComponent<Button>();
            TextMeshProUGUI adText = gadgetIntance
                .transform.Find("Buttons/WatchAd/Button/Text")
                .GetComponent<TextMeshProUGUI>();

            confirmButton.onClick.RemoveAllListeners();
            watchAdButton.onClick.RemoveAllListeners();

            if (gadget.pricePerBuilding.ContainsKey(gadget.currentPlacedBuilding + 1))
            {
                cornfirmText.gameObject.SetActive(true);
                cornfirmText.text = gadget
                .pricePerBuilding[gadget.currentPlacedBuilding + 1]
                .ToString();

                adText.gameObject.SetActive(true);
                adText.text = $"WATCH AD\n{adWatchedPerBuilding[gadget.gadgetsData]}/{GetAmountOfAdPerUpgradeWithPrice(gadget.pricePerBuilding[gadget.currentPlacedBuilding + 1])}";
            }
            else
            {
                cornfirmText.gameObject.SetActive(false);
                adText.gameObject.SetActive(false);
            }

            // affiche le nombre de pub qu'il a regardé pour ce bâtiment sur 2

            confirmButton.onClick.AddListener(() =>
            {
                HidePanel();

                if (!gadget.pricePerBuilding.ContainsKey(gadget.currentPlacedBuilding + 1))
                    return;

                RessourcesManager.Instance.Buy(
                    PaymantSubtype.Gold,
                    gadget.pricePerBuilding[gadget.currentPlacedBuilding + 1],
                    () =>
                    {
                        if (!VillageRessourcesManager.Instance.CanAddBuilding(gadget.gadgetsData))
                        {
                            Debug.LogWarning("Can't add building");
                            return;
                        }
                        AddToBuildingList(gadget);
                        Inventory.Instance.AddGadgets(gadget.gadgetsData, 1);
                        VillageMovingManager.Instance.SpawnGadget(gadget.gadgetsData.gadgetId, 1);
                        UpdateGadgetsListPanel();
                    },
                    () =>
                    {
                        Debug.LogWarning("Error during upgrade");
                    }
                );
            });

            watchAdButton.onClick.AddListener(() =>
            {
                AdmobManager.Instance.ShowRewardedAd(
                    (rewardAd) =>
                    {
                        adWatchedPerBuilding[gadget.gadgetsData]++;
                        if (adWatchedPerBuilding[gadget.gadgetsData] >= GetAmountOfAdPerUpgradeWithPrice(gadget.pricePerBuilding[gadget.currentPlacedBuilding + 1]))
                        {
                            if (!VillageRessourcesManager.Instance.CanAddBuilding(gadget.gadgetsData))
                            {
                                Debug.LogWarning("Can't add building");
                                return;
                            }
                            adWatchedPerBuilding[gadget.gadgetsData] = 0;
                            HidePanel();
                            AddToBuildingList(gadget);
                            Inventory.Instance.AddGadgets(gadget.gadgetsData, 1);
                            VillageMovingManager.Instance.SpawnGadget(
                                gadget.gadgetsData.gadgetId,
                                1
                            );
                        }
                        else
                        {
                            UpdateGadgetsListPanel();
                        }
                    }
                );
            });
        }
    }

    private int GetAmountOfAdPerUpgradeWithPrice(double price)
    {
        // retourne le nombre de pub à regarder pour acheter un bâtiment
        // 1000 gold = 2 ad
        // 2000 gold = 2 ad
        // 3000 gold = 3 ad
        return Mathf.Max(2, Mathf.CeilToInt((float)(price / 1000)));
    }

    private void AddToBuildingList(BuildingList buildingList)
    {
        if (VillageRessourcesManager.Instance != null)
        {
            // Rechercher s'il existe déjà un élément avec les mêmes gadgetsData dans un tableau
            var existingBuilding = Array.Find(
                VillageRessourcesManager.Instance.buildingList,
                e => e.gadgetsData == buildingList.gadgetsData
            );

            // Vérifier si le bâtiment existe avant d'incrémenter
            if (existingBuilding != null)
            {
                existingBuilding.currentPlacedBuilding++;

            }
            else
            {
                Debug.LogWarning("Bâtiment non trouvé dans la liste.");
            }
        }
        else
        {
            // Rechercher s'il existe déjà un élément avec les mêmes gadgetsData dans un tableau
            var existingBuilding = Array.Find(
                RessourcesManager.Instance.buildingList,
                e => e.gadgetsData == buildingList.gadgetsData
            );

            // Vérifier si le bâtiment existe avant d'incrémenter
            if (existingBuilding != null)
            {
                existingBuilding.currentPlacedBuilding++;
            }
            else
            {
                Debug.LogWarning("Bâtiment non trouvé dans la liste.");
            }
        }
    }




}

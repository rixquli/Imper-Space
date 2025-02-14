using System;
using System.Collections.Generic;
using UnityEngine;

public class VillageRessourcesManager : MonoBehaviour, IDataPersistence
{
    public static VillageRessourcesManager Instance;

    public BuildingList[] buildingList = { };

    public TroopsToPlace[] troopsToPlaces;

    private bool hadInit = false;

    [SerializeField] private GameObject purschasableBuildingPrefab;

    private bool DesactivateBuildingLevelCheckToUpgradeBase = true;

    private List<PurchasableBuildingController> purchasableBuildingControllers = new();
    private bool isConstructionMode = false;

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

    private void Init()
    {
        hadInit = true;
        foreach (var building in buildingList)
        {
            foreach (var buildingObject in building.buildings)
            {
                if (buildingObject == null || building.gadgetsData == null || building.gadgetsData.prefab == null)
                {
                    Debug.LogWarning("Building object or gadgets data is null");
                    continue;
                }

                GameObject purschasableBuilding = Instantiate(
                    purschasableBuildingPrefab,
                    buildingObject.position,
                    Quaternion.Euler(0, 0, 0)
                );

                if (purschasableBuilding != null && purschasableBuilding.TryGetComponent(out PurchasableBuildingController controller))
                {
                    purchasableBuildingControllers.Add(controller);
                    controller.gadgetData = building.gadgetsData;
                    controller.SpawnBuilding(buildingObject);
                }

                // GameObject instance = Instantiate(
                //     building.gadgetsData.prefab,
                //     buildingObject.position,
                //     Quaternion.Euler(0, 0, buildingObject.rotationZ)
                // );
                // instance.SetActive(false); // Disable the instance to prevent visual artifacts

                // IDraggable draggable = instance.GetComponent<IDraggable>();
                // if (draggable != null)
                // {
                //     draggable.Level = buildingObject.level;
                //     draggable.SpawnToVillage(); // Perform necessary setup
                // }

                // instance.SetActive(true);
            }
        }
    }

    public void ToggleConstructionMode()
    {
        isConstructionMode = !isConstructionMode;
        foreach (var controller in purchasableBuildingControllers)
        {
            if (isConstructionMode)
            {
                controller.ShowUnboughtBuilding();
            }
            else
            {
                controller.HideUnboughtBuilding();
            }
        }
    }

    public int GetMaxAmount(TroopsData troopsData)
    {
        // renvoie le nombre maximum de troupes que l'on peut placer
        // en cherchant la liste des casernes qui ont le niveau minimum
        // requis dans troopsData.troopPerCaserneLevel

        int maxAmount = 0;
        BuildingList barracks = Array.Find(buildingList, x => x.gadgetsData.gadgetId == 3);
        foreach (BuildingList.Buildings building in barracks.buildings)
        {
            if (building.level >= troopsData.troopPerCaserneLevel.Keys.ToArray()[0] && building.isPurchase)
            {
                maxAmount += troopsData.troopPerCaserneLevel[
                    troopsData.troopPerCaserneLevel.Keys.ToArray()[0]
                ];
            }
        }

        return maxAmount;
    }

    public void AddTroop(TroopsData troopsData, int amount = 1)
    {
        // ajoute une troupe à la liste des troupes à placer
        TroopsToPlace troop = Array.Find(troopsToPlaces, x => x.troopsData == troopsData);
        if (troop == null)
        {
            troop = new TroopsToPlace(troopsData, amount);
            Array.Resize(ref troopsToPlaces, troopsToPlaces.Length + 1);
            troopsToPlaces[troopsToPlaces.Length - 1] = troop;
        }
        else
        {
            troop.amount += amount;
        }
    }

    public int GetTroopAmount()
    {
        int total = 0;
        foreach (TroopsToPlace troop in troopsToPlaces)
        {
            total += troop.amount;
        }
        return total;
    }

    public bool CanUpgradeBase()
    {
        if (DesactivateBuildingLevelCheckToUpgradeBase) return true;

        // Récupère tous les bâtiments sauf la base (HDV)
        BuildingList[] allBuildingWithoutBase = Array.FindAll(
            buildingList,
            x => x.gadgetsData.gadgetId != 5
        );

        // Récupère la base (HDV)
        BuildingList hdv = Array.Find(buildingList, x => x.gadgetsData.gadgetId == 5);

        // Niveau actuel de la base (HDV)
        int hdvLevel = hdv.buildings[0].level;

        // Parcourt tous les bâtiments sauf la base
        foreach (BuildingList buildingList in allBuildingWithoutBase)
        {
            // Verifie que tous les bâtiment que si hdvLevelRequired est inferieur ou égale au niveau de l'hdv soit acheté
            foreach (BuildingList.Buildings building in buildingList.buildings)
            {
                if (building.hdvLevelRequired <= hdvLevel && !building.isPurchase)
                {
                    return false;
                }
            }

            // Vérifie que tous les bâtiments sont au moins au niveau de la base (HDV)
            // foreach (BuildingList.Buildings building in buildingList.buildings)
            // {
            //     if (building.level < hdvLevel)
            //     {
            //         return false;
            //     }
            // }
        }

        // Si toutes les conditions sont remplies, on peut améliorer la base
        return true;
    }

    public bool CanUpgradeBuilding(int targetLevel)
    {
        // Récupère la base (HDV)
        BuildingList hdv = Array.Find(buildingList, x => x.gadgetsData.gadgetId == 5);

        // Niveau actuel de la base (HDV)
        int hdvLevel = hdv.buildings[0].level;

        // Vérifie que le bâtiment est au moins au niveau de la base (HDV)
        if (targetLevel > hdvLevel)
        {
            return false;
        }

        // Si toutes les conditions sont remplies, on peut améliorer le bâtiment
        return true;
    }

    public bool IsBuildingPurchased(BuildingList.Buildings building)
    {
        return building.isPurchase;
    }

    public bool IsBuildingLocked(BuildingList.Buildings building)
    {
        BuildingList hdv = Array.Find(buildingList, x => x.gadgetsData.gadgetId == 5);
        return !building.isPurchase && building.hdvLevelRequired > hdv.buildings[0].level;
    }

    public int GetMaxAmount(GadgetsData gadgetsData)
    {
        // Récupère tous les bâtiments sauf la base (HDV)
        BuildingList[] allBuildingWithoutBase = Array.FindAll(
            buildingList,
            x => x.gadgetsData.gadgetId != 5
        );

        // Récupère la base (HDV)
        BuildingList hdv = Array.Find(buildingList, x => x.gadgetsData.gadgetId == 5);

        // Niveau actuel de la base (HDV)
        int hdvLevel = hdv.buildings[0].level;

        int maxAmount = 0;

        // Parcourt tous les bâtiments sauf la base
        foreach (BuildingList buildingList in allBuildingWithoutBase)
        {
            // Verifie que tous les bâtiment que si hdvLevelRequired est inferieur ou égale au niveau de l'hdv soit acheté
            foreach (BuildingList.Buildings building in buildingList.buildings)
            {
                if (buildingList.gadgetsData == gadgetsData && building.hdvLevelRequired <= hdvLevel)
                {
                    maxAmount++;
                }
            }
        }

        return maxAmount;
    }

    public int GetAmount(GadgetsData gadgetsData)
    {
        // Récupère le nombre de gadgets déjà placés
        return Array.Find(buildingList, x => x.gadgetsData == gadgetsData)?.buildings.Count ?? 0;
    }

    public bool CanAddBuilding(GadgetsData gadgetsData)
    {
        // Récupère la base (HDV)
        BuildingList hdv = Array.Find(buildingList, x => x.gadgetsData.gadgetId == 5);

        // Niveau actuel de la base (HDV)
        int hdvLevel = hdv.buildings[0].level;

        // Récupère le nombre maximum de gadgets que l'on peut placer
        int maxAmount = GetMaxAmount(gadgetsData);

        // Récupère le nombre de gadgets déjà placés
        int currentAmount =
            Array.Find(buildingList, x => x.gadgetsData == gadgetsData)?.buildings.Count ?? 0;

        // Vérifie que le nombre de gadgets déjà placés est inférieur au nombre maximum de gadgets que l'on peut placer
        return currentAmount < maxAmount;
    }

    public bool TryPurchaseBuilding(BuildingList.Buildings building, GadgetsData gadgetsData)
    {
        if (building.isPurchase) return false;

        return RessourcesManager.Instance.Buy(
            PaymantSubtype.Gold,
            building.price,
            () =>
            {
                building.isPurchase = true;
            },
            () =>
            {
                Debug.LogWarning("Error during upgrade");
            }
        );
    }

    public void LoadData(GameData data)
    {
        if (data.villageData.buildingLists != null && data.villageData.buildingLists.Length > 0)
        {
            this.buildingList = data.villageData.buildingLists;
            Inventory.Instance.gadgets = data.villageData.gadgetList;
        }
        if (!hadInit)
            Init();

        troopsToPlaces = data.villageData.troopsToPlaces;
    }

    public void SaveData(GameData data)
    {
        data.villageData.buildingLists = buildingList;
        data.villageData.gadgetList = Inventory.Instance.gadgets;
        data.villageData.troopsToPlaces = troopsToPlaces;
    }
}

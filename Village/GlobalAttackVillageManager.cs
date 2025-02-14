using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalAttackVillageManager : MonoBehaviour, IDataPersistence
{
    public static GlobalAttackVillageManager Instance;

    [HideInInspector]
    public Transform playerTransform;

    [HideInInspector]
    public VillageCameraController cameraController;

    public event Action OnAttackBegin;
    public event Action OnTroopsSetup;

    public List<TroopsToPlace> troopsToPlaces;
    public TroopsToPlace selectedTroop;
    public int aliveAllies = 0;

    private bool hadInit = false;
    public bool hadStartGame = false;
    private int instantiatedObjectsCount = 0;
    private PlayerBase playerBase;

    private void Awake()
    {
        if (RessourcesManager.Instance == null)
            SceneManager.LoadScene("ManagersScene", LoadSceneMode.Additive);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UIController.Instance.HidePlayerUI();

        if (
            VillagesDataManager.Instance == null
            || VillagesDataManager.Instance.buildingList.Length == 0
        )
            return;

        PlaceBuildings(VillagesDataManager.Instance.buildingList);
        StaticDataManager.isVictory = false;
    }

    private void Update() { }

    public void AllyDeath()
    {
        aliveAllies--;
        if (aliveAllies <= 0)
        {
            UIController.Instance.ShowVillageAttackGameOver();
        }
    }

    public void EnemyBaseDestroyed()
    {
        Inventory.Instance.AddGems(VillagesDataManager.Instance.rewardGem);
        Inventory.Instance.AddGold(VillagesDataManager.Instance.rewardGold);
        StaticDataManager.isVictory = true;
        UIController.Instance.ShowVillageAttackVictory();
    }

    private void PlaceBuildings(BuildingList[] buildingList)
    {
        foreach (var building in buildingList)
        {
            foreach (var buildingObject in building.buildings)
            {
                if (building.gadgetsData.prefab == null)
                    continue;

                GameObject instance = Instantiate(
                    building.gadgetsData.prefab,
                    buildingObject.position,
                    Quaternion.Euler(0, 0, buildingObject.rotationZ)
                );
                instance.SetActive(false); // Disable the instance to prevent visual artifacts

                IDraggable draggable = instance.GetComponent<IDraggable>();
                if (draggable != null)
                {
                    draggable.Level = buildingObject.level;
                }

                if (building.gadgetsData.gadgetId == 5 && instance.TryGetComponent(out PlayerBase playerBase))
                {
                    playerBase.ShowBarrier();
                    this.playerBase = playerBase;
                }

                if (draggable.GadgetsData.gadgetId != 5 && instance.TryGetComponent(out BuildingsBehaviour buildingsBehaviour))
                {
                    instantiatedObjectsCount++;
                    buildingsBehaviour.OnDeath += () =>
                    {
                        instantiatedObjectsCount--;
                        if (instantiatedObjectsCount <= 0)
                        {
                            this.playerBase.HideBarrier();
                        }
                    };
                }

                instance.SetActive(true);
            }
        }
    }

    public void StartAttack(Action callback)
    {
        if (playerTransform == null)
            return;

        callback?.Invoke();

        OnAttackBegin?.Invoke();

        hadStartGame = true;

        UIController.Instance.ShowPlayerUI();
        StaticDataManager.IsAutoModeActive = !StaticDataManager.IsAutoModeActive;
        StaticDataManager.autoMode = AutoMode.TargetEnemy;
    }

    // Set the player trandform to the camera dollow system to follow the player
    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
        cameraController.PlayerTransform = playerTransform;
    }

    public void LoadData(GameData data)
    {
        Debug.LogError("OnLoadData");
        if (data.villageData.troopsToPlaces != null && data.villageData.troopsToPlaces.Length > 0)
        {
            foreach (var item in data.villageData.troopsToPlaces)
            {
                troopsToPlaces.Add(item);
            }
        }
        if (!hadInit)
        {
            hadInit = true;
            OnTroopsSetup?.Invoke();
            Debug.LogError("OnTroopsSetup has been called");
        }
    }

    public void SaveData(GameData data) { }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalDefenseVillageManager : MonoBehaviour, IDataPersistence
{
    [System.Serializable]
    class Corners
    {
        public Vector2 topLeft,
            bottomRight;
    }

    public static GlobalDefenseVillageManager Instance;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    Corners[] spawnableAreas;

    [SerializeField]
    VillageCameraController cameraController;

    public BuildingList[] buildingList = { };

    public int totalEnemiesInWave = 0;
    public int aliveEnemies = 0;

    private bool hadInit = false;

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
        StaticDataManager.IsAutoModeActive = false;
        StaticDataManager.isVictory = false;
    }

    private void Init()
    {
        hadInit = true;
        PlaceBuilding();
        SpawnPlayer();
        SpawnEnemies();
    }

    private void PlaceBuilding()
    {
        foreach (var building in buildingList)
        {
            foreach (var buildingObject in building.buildings)
            {
                if (building.gadgetsData.prefab == null || !buildingObject.isPurchase)
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
                    draggable.SpawnToVillage(); // Perform necessary setup
                }

                instance.SetActive(true);
            }
        }
    }

    private void SpawnPlayer()
    {
        GameObject player = Instantiate(
            playerPrefab,
            Vector3.zero + new Vector3(0, 7, 0),
            Quaternion.identity
        );

        cameraController.PlayerTransform = player.transform;

        // Ensure that GameManager instance is available before starting the game
        AttributesData playerAttributesData = DataPersistenceManager
            .instance
            .gameData
            .playerAttributesData;

        // set the sprite of the plyaer during the game
        Sprite playerSprite = null;
        if (
            DataPersistenceManager.instance.gameData.equipedEquipements != null
            && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(
                EquipeSlot.Ship,
                out var shipData
            )
            && shipData?.icon != null
        )
        {
            playerSprite = shipData.icon;
            Debug.Log("ship sprite loaded: " + shipData.displayName);
        }

        // set the laser prefab during the game
        GameObject laserPrefab = null;
        if (
            DataPersistenceManager.instance.gameData.equipedEquipements != null
            && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(
                EquipeSlot.Laser,
                out var laserData
            )
            && laserData?.prefab != null
        )
        {
            laserPrefab = laserData.prefab;
        }

        // set the missile prefab during the game
        GameObject missiePrefab = null;
        if (
            DataPersistenceManager.instance.gameData.equipedEquipements != null
            && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(
                EquipeSlot.Missile,
                out var missileData
            )
            && missileData?.prefab != null
        )
        {
            missiePrefab = missileData.prefab;
        }

        PlayerEntity playerEntity = player.GetComponent<PlayerEntity>();
        playerEntity.sprite = playerSprite;

        playerEntity.baseAttackDamage = playerAttributesData.attack;
        playerEntity.baseMaxHealth = playerAttributesData.health;
        playerEntity.baseHealthRegenRate = playerAttributesData.regenHealth;
        playerEntity.baseMaxShield = playerAttributesData.shield;
        playerEntity.baseShieldRegenRate = playerAttributesData.regenShield;
        playerEntity.baseSpeed = playerAttributesData.speed;
        playerEntity.baseLaserAttackDamage = playerAttributesData.laserAttribute.attack;
        playerEntity.baseMissileAttackDamage = playerAttributesData.missileAttribute.attack;
        playerEntity.bulletPerSecond = playerAttributesData.bulletPerSecond;

        playerEntity.laserPrefab = laserPrefab;
        playerEntity.missilePrefab = missiePrefab;

        playerEntity.InitializeStats();
    }

    private void SpawnEnemies()
    {
        Vector2 origin = new Vector2(0, 0);
        totalEnemiesInWave = VillagesDataManager.Instance.ennemiesToSpawn.Length;
        aliveEnemies = totalEnemiesInWave;
        UIController.Instance.SetWaveBar(aliveEnemies, totalEnemiesInWave);
        UIController.Instance.SetTextUnderProgressBarObject($"{aliveEnemies} alive enemies");
        // UpdateText($"{aliveEnemies} alive enemies");


        foreach (GameObject enemy in VillagesDataManager.Instance.ennemiesToSpawn)
        {
            Vector2 spawnPoint = GetRandomPoint();
            if (spawnPoint == Vector2.zero)
                continue;
            Debug.Log("Spawning enemy" + enemy.name);
            GameObject enemyObject = Instantiate(enemy, spawnPoint, Quaternion.identity);

            IAttackPlayerBase attackPlayerBase = enemyObject.GetComponent<IAttackPlayerBase>();
            if (attackPlayerBase != null)
            {
                attackPlayerBase.TargetAttackPlayerBase(true);
            }

            IEnemy enemyEntity = enemyObject.GetComponent<IEnemy>();
            if (enemyEntity != null)
            {
                enemyEntity.OnDeath += HandleEnemyDeath;
                int randomLevel = (int)
                    Random.Range(
                        VillagesDataManager.Instance.minEnemiesLevel,
                        VillagesDataManager.Instance.maxEnemiesLevel + 1
                    );
                enemyEntity.Level = randomLevel;
            }
        }
    }

    private void HandleEnemyDeath()
    {
        aliveEnemies--;
        UIController.Instance.SetWaveBar(aliveEnemies, totalEnemiesInWave);
        UIController.Instance.SetTextUnderProgressBarObject($"{aliveEnemies} alive enemies");
        if (aliveEnemies == 0)
        {
            UIController.Instance.ShowVillageDefenseWinPanel();
        }
    }

    private Vector2 GetRandomPoint()
    {
        int randomNumber = Random.Range(0, spawnableAreas.Length);
        Corners randomArea = spawnableAreas[randomNumber];
        float randomX = Random.Range(randomArea.topLeft.x, randomArea.bottomRight.x);
        float randomY = Random.Range(randomArea.topLeft.y, randomArea.bottomRight.y);
        return new Vector2(randomX, randomY);
    }

    public void LoadData(GameData data)
    {
        if (data.villageData.buildingLists != null || this.buildingList.Length == 0)
            this.buildingList = data.villageData.buildingLists;
        if (!hadInit)
            Init();
    }

    public void SaveData(GameData data) { }
}

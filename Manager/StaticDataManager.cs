using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

//TODO: clean this shit

public enum GameMode
{
    Classic,
    Extermination,
    InfinityMode,
    TurretAssault,
    FrontLine,
}

public static class StaticDataManager
{
    public static GameMode gameMode = GameMode.Classic;
    private static bool isSinglePlayer = true;

    public static bool IsSinglePlayer
    {
        get { return isSinglePlayer; }
        set { isSinglePlayer = value; }
    }

    private static GameState currentGameState;
    private static GameObject playerPrefab;
    private static GameObject laserPrefab;
    private static GameObject missiePrefab;
    private static AttributesData playerAttributesData;
    private static Sprite playerSprite;
    private static UDictionary<Transform, GameObject> prefabsToPlace;
    private static GameObject[] ennemiesToSpawn;
    private static Vector2 topLeftCoords;
    private static Vector2 bottomRightCoords;
    public static bool isHost = false;
    public static bool IsAutoModeActive = false;
    public static AutoModeState autoModeState = AutoModeState.Disabled;
    public static AutoMode autoMode = AutoMode.TargetEnemy;
    private static bool isPaused = false;

#if UNITY_EDITOR
    public static int levelNumber = -1; //TODO: IMPORTANT SWITCH TO -1
#endif
#if !UNITY_EDITOR
    public static int levelNumber = -1; //TODO: IMPORTANT SWITCH TO -1
#endif
    public static int minEnemiesLevel = 1;
    public static int maxEnemiesLevel = 3;
    public static RessourceType victoryRewardType;
    public static double victoryRewardAmount;
    public static bool isVictory = false;

    //Create a class that actually inheritance from MonoBehaviour
    public class StaticMB : MonoBehaviour { }

    //Variable reference for the class
    private static StaticMB myStaticMB;

    public enum GameState
    {
        MainMenu,
        Shop,
        Inventory,
        Lobby,
        Loading,
        InGame,
    }

    public enum AutoModeState
    {
        Disabled,
        SemiAuto,
        FullAuto,
    }

    public static void Initialize(
        GameObject player,
        AttributesData attributesData,
        Sprite sprite,
        GameObject laserPb,
        GameObject missilePb
    )
    {
        playerPrefab = player;
        playerAttributesData = attributesData;
        playerSprite = sprite;
        laserPrefab = laserPb;
        missiePrefab = missilePb;

        Pause();
    }

    public static void Initialize(
        GameObject player,
        AttributesData attributesData,
        Sprite sprite,
        UDictionary<Transform, GameObject> prefabs,
        GameObject laserPb,
        GameObject missilePb
    )
    {
        playerPrefab = player;
        playerAttributesData = attributesData;
        playerSprite = sprite;
        prefabsToPlace = prefabs;
        laserPrefab = laserPb;
        missiePrefab = missilePb;

        Pause();
    }

    public static void Initialize(
        GameObject player,
        AttributesData attributesData,
        Sprite sprite,
        GameObject[] prefabs,
        Vector2 topLeftXY,
        Vector2 bottomRightXY,
        GameObject laserPb,
        GameObject missilePb
    )
    {
        playerPrefab = player;
        playerAttributesData = attributesData;
        playerSprite = sprite;
        ennemiesToSpawn = prefabs;
        topLeftCoords = topLeftXY;
        bottomRightCoords = bottomRightXY;
        laserPrefab = laserPb;
        missiePrefab = missilePb;

        Pause();
    }

    //TODO: nettoyer cette merde
    private static void Init()
    {
        //If the instance not exit the first time we call the static class
        if (myStaticMB == null)
        {
            //Create an empty object called MyStatic
            GameObject gameObject = new GameObject("Static");

            //Add this script to the object
            myStaticMB = gameObject.AddComponent<StaticMB>();
        }
    }

    public static void PerformCoroutine()
    {
        //Call the Initialization
        Init();

        //Call the Coroutine
        myStaticMB.StartCoroutine(MyCoroutine());
    }

    private static IEnumerator MyCoroutine()
    {
        Debug.Log("MAGIC");
        yield return new WaitForSeconds(5f);
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartFirstWave();
    }

    public static void LoadGame(bool isSinglePlayer)
    {
        IsSinglePlayer = isSinglePlayer;

        Loader.Load(Loader.Scene.Game);
    }

    public static void StartGame()
    {
        Debug.Log("Starting game");
        IsAutoModeActive = false;
        if (gameMode == GameMode.Classic)
        {
            SpawnPlayer();
            PlacePrefabs();
        }
        if (gameMode == GameMode.FrontLine)
        {
            SpawnPlayer();
            PlacePrefabs();
        }
        else if (gameMode == GameMode.Extermination)
        {
            SpawnPlayer();
            SpawnEnnemies();
        }
        else if (gameMode == GameMode.InfinityMode)
        {
            SpawnPlayer();
        }
        Pause(false);
    }

    public static void BackToMenu(
        List<ItemsAndAmount> itemsAndAmount,
        double goldAmount,
        double gemAmount = 0,
        bool village = false
    )
    {
        // Ajouter des items au gestionnaire de ressources
        RessourcesManager.Instance.AddItems(itemsAndAmount);

        double totalGoldAmount = goldAmount;
        double totalGemAmount = gemAmount;

        if (isVictory)
        {
            switch (victoryRewardType)
            {
                case RessourceType.Gold:
                    totalGoldAmount += victoryRewardAmount;
                    break;
                case RessourceType.Gem:
                    totalGemAmount += victoryRewardAmount;
                    break;
                default:
                    Debug.LogWarning("Unknown victoryRewardType: " + victoryRewardType);
                    break;
            }
            if (!village)
            {
                RessourcesManager.Instance.SetLevelCompleted(levelNumber);
            }

            isVictory = false;
        }

        // Ajouter de l'or et des gemmes au gestionnaire de ressources
        XPManager.Instance.hadSaved = false;
        if (totalGoldAmount != 0)
            RessourcesManager.Instance.AddGold(totalGoldAmount);
        if (totalGemAmount != 0)
            RessourcesManager.Instance.AddGems(totalGemAmount);

        levelNumber = -1;

        if (!village)
        {
            // Afficher une publicité avec une probabilité de 50%
            // int randInt = Random.Range(0, 2);
            // if (randInt == 0)
            // {
            //     // Charger la scène du lobby avec une pub
            //     Loader.LoadWithInterstitialAd(Loader.Scene.Lobby);
            // }
            // else
            // {
            // Charger la scène du lobby sans pub
            Loader.Load(Loader.Scene.Lobby);
            // }
        }
    }

    public static void SpawnItemObjectWithDelay(ItemData item, Vector2 position, float delay)
    {
        CoroutineRunner.Instance.StartCoroutine(SpawnItemObjectDelayed(item, position, delay));
    }

    private static IEnumerator SpawnItemObjectDelayed(ItemData item, Vector2 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        InstantiateItem(item, position);
    }

    public static void SpawnItemObject(ItemData item, Vector2 position)
    {
        InstantiateItem(item, position);
    }

    public static void InstantiateItemSinglePlayer(int itemId, Vector2 position)
    {
        ItemData item = ItemDatabase.Instance.GetItemById(itemId);
        if (item != null)
        {
            GameObject itemOnFloor = Object.Instantiate(
                ItemDatabase.Instance.prefab,
                position,
                Quaternion.identity
            );
            ItemOnFloorManager itemManager = itemOnFloor.GetComponent<ItemOnFloorManager>();

            if (itemManager != null)
            {
                itemManager.item = item;
            }
            else
            {
                Debug.LogError(
                    "ItemOnFloorManager component is missing on the instantiated prefab."
                );
            }

            itemOnFloor.transform.DOScale(1, 0.5f).From(0.75f).SetEase(Ease.OutBack);
        }
        else
        {
            Debug.LogError("Item not found");
        }
    }

    public static void InstantiateItem(ItemData item, Vector2 position)
    {
        InstantiateItemSinglePlayer(item.itemId, position);
    }

    private static void SpawnPlayer()
    {
        GameObject player = Object.Instantiate(
            playerPrefab,
            Vector3.zero + new Vector3(0, 5, 0),
            Quaternion.identity
        );

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

        // Dash
        playerEntity.availableDash = playerAttributesData.dashAmount;
        playerEntity.dashDuration = playerAttributesData.dashDuration;
        playerEntity.dashSpeed = playerAttributesData.dashSpeed;
        playerEntity.dashRegenRate = playerAttributesData.dashRegenRate;

        playerEntity.laserPrefab = laserPrefab;
        playerEntity.missilePrefab = missiePrefab;

        playerEntity.InitializeStats();
    }

    private static void SpawnEnnemies()
    {
        Vector2 origin = new Vector2(0, 0);
        // totalEnemiesInWave = enemies.Length;
        // aliveEnemies = totalEnemiesInWave;
        // UIController.Instance.SetWaveBar(aliveEnemies, totalEnemiesInWave);
        // UpdateText($"{aliveEnemies} alive enemies");

        foreach (var enemy in ennemiesToSpawn)
        {
            Vector2 spawnPoint = GetRandomPoint();
            if (spawnPoint == Vector2.zero)
                continue;
            Debug.Log("Spawning enemy" + enemy.name);
            ObjectManager.Instance.SpawnObject(
                enemy,
                spawnPoint,
                Quaternion.identity,
                (GameObject enemyObject) =>
                {
                    IEnemy enemy = enemyObject.GetComponent<IEnemy>();
                    IGadgetable gadget = enemyObject.GetComponent<IGadgetable>();
                    if (enemy != null)
                    {
                        if (gadget != null)
                        {
                            enemy.Level =
                                (int)
                                    Mathf.Floor(
                                        Random.Range(
                                            minEnemiesLevel * 1.5f,
                                            maxEnemiesLevel * 1.5f + 1
                                        ) / 10
                                    ) + 1;
                        }
                        else
                        {
                            enemy.Level = (int)
                                Random.Range(minEnemiesLevel * 1.5f, maxEnemiesLevel * 1.5f + 1);
                        }
                    }
                }
            );
            // GameObject enemyObject = Instantiate(enemy, spawnPoint, Quaternion.identity);
        }
    }

    private static Vector2 GetRandomPoint()
    {
        // Generate a random point within the bounds
        float randomX = Random.Range(topLeftCoords.x, bottomRightCoords.x);
        float randomY = Random.Range(topLeftCoords.y, bottomRightCoords.y);

        // Convert the point from collider space to world space
        return new Vector2(randomX, randomY);
    }

    private static void PlacePrefabs()
    {
        foreach (var kvp in prefabsToPlace)
        {
            Transform spawnTransform = kvp.Key;
            GameObject prefab = kvp.Value;
            Vector3 spawnPosition = spawnTransform.position;

            Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }

    public static void Pause()
    {
        Time.timeScale = isPaused ? 1 : 0;
        isPaused = !isPaused;
    }

    public static void Pause(bool paused)
    {
        Time.timeScale = paused ? 0 : 1;
        isPaused = paused;
    }

    public static void StartGameplay()
    {
        //TODO: change this shit
        if (gameMode == GameMode.Classic || gameMode == GameMode.InfinityMode)
        {
            PerformCoroutine();
        }
        else if (gameMode == GameMode.Extermination)
        {
            ExterminationManager.Instance.ShowEnemyBar();
        }
    }
}

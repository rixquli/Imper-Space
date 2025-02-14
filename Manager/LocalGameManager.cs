using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LocalGameManager : MonoBehaviour
{
    [SerializeField] private GameMode gameMode = GameMode.Classic;
    [SerializeField] private GameObject player;
    [Header("For Classic Mode")]
    [SerializeField] private UDictionary<Transform, GameObject> prefabs;
    [Header("For FrontLine Mode")]
    [SerializeField] private UDictionary<Transform, GameObject> linesPrefabs;

    [Header("For Extermination Mode")]
    [SerializeField] private GameObject[] ennemiesToSpawn;
    [SerializeField] private GameObject[] ennemiesToSpawnIfLevel1;
    [SerializeField] private Transform topLeftCoords;
    [SerializeField] private Transform bottomRightCoords;

    private void Awake()
    {
        if (RessourcesManager.Instance == null)
            SceneManager.LoadScene("ManagersScene", LoadSceneMode.Additive);
    }


    // Start is called before the first frame update
    private void Start()
    {
        StaticDataManager.gameMode = gameMode;
        // Ensure that GameManager instance is available before starting the game
        AttributesData playerAttributesData = DataPersistenceManager.instance.gameData.playerAttributesData;
        playerAttributesData.UpdateAttribute();

        // set the sprite of the plyaer during the game
        Sprite equippedEquipmentIcon = null;
        if (DataPersistenceManager.instance.gameData.equipedEquipements != null && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(EquipeSlot.Ship, out var shipData) && shipData?.icon != null)
        {
            equippedEquipmentIcon = shipData.icon;
            Debug.Log("ship sprite loaded: " + shipData.displayName);
        }

        // set the laser prefab during the game
        GameObject laserPrefab = null;
        if (DataPersistenceManager.instance.gameData.equipedEquipements != null && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(EquipeSlot.Laser, out var laserData) && laserData?.prefab != null)
        {
            laserPrefab = laserData.prefab;
        }

        // set the missile prefab during the game
        GameObject missilePrefab = null;
        if (DataPersistenceManager.instance.gameData.equipedEquipements != null && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(EquipeSlot.Missile, out var missileData) && missileData?.prefab != null)
        {
            missilePrefab = missileData.prefab;
        }

        if (StaticDataManager.gameMode == GameMode.Classic)
        {
            StaticDataManager.Initialize(player, playerAttributesData, equippedEquipmentIcon, prefabs, laserPrefab, missilePrefab);
        }
        if (StaticDataManager.gameMode == GameMode.FrontLine)
        {
            StaticDataManager.Initialize(player, playerAttributesData, equippedEquipmentIcon, linesPrefabs, laserPrefab, missilePrefab);
            StartCoroutine(ShowAlertText("Kill the Guardian at the top\nto Destroy the Enemy Base"));
        }
        else
        if (StaticDataManager.gameMode == GameMode.InfinityMode)
        {
            StaticDataManager.Initialize(player, playerAttributesData, equippedEquipmentIcon, laserPrefab, missilePrefab);
        }
        else if (StaticDataManager.gameMode == GameMode.Extermination)
        {
            if (StaticDataManager.levelNumber == 1)
            {
                bottomRightCoords.position += new Vector3(0, 30f, 0);

                StaticDataManager.Initialize(player, playerAttributesData, equippedEquipmentIcon, ennemiesToSpawnIfLevel1, (Vector2)topLeftCoords.position, (Vector2)bottomRightCoords.position, laserPrefab, missilePrefab);
            }
            else
            {
                StaticDataManager.Initialize(player, playerAttributesData, equippedEquipmentIcon, ennemiesToSpawn, (Vector2)topLeftCoords.position, (Vector2)bottomRightCoords.position, laserPrefab, missilePrefab);
            }
        }
        StaticDataManager.StartGame();
        // StartCoroutine(StartGameAfterManagersSceneLoaded());
    }

    private IEnumerator ShowAlertText(string text)
    {
        yield return new WaitForSeconds(2f);
        UIController.Instance.ShowAlertText(text);
    }

}

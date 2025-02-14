using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Shop,
        Inventory,
        Loby,
        Loading,
        InGame,
    }

    // Current Game State 
    public GameState currentGameState;

    //All UI elements
    public GameObject[] panelArray;
    public GameObject mainMenuPanel;
    public GameObject inGamePanel;

    [SerializeField] private GameObject playerPrefab;
    public GameObject[] prefabsToPlace;
    public Vector2Int topLeftCoords;
    public Vector2Int bottomRightCoords;
    public float minDistanceBetweenPrefabs = 50f; // Distance minimale entre les prefabs pour éviter la collision
    public int maxAttemptsPerPrefab = 10;

    public bool isSinglePlayer = true;
    public bool isHost = false;



    void Awake()
    {
        Debug.Log("GameManager Awake called.");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager Instance created and set to not destroy on load.");
        }
        else if (Instance != this)
        {
            // Destroy(gameObject);
            Debug.Log("Duplicate GameManager destroyed.");
            return;
        }
        currentGameState = GameState.MainMenu;
        panelArray = new GameObject[] { mainMenuPanel, inGamePanel };
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.Log("GameManager OnDestroy called.");
            Instance = null;
        }
    }

#if UNITY_EDITOR 
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Tab))
        // {
        //     Loader.Load(Loader.Scene.Lobby);
        // }
        // if (Input.GetKeyDown(KeyCode.D))
        // {
        //     UIController.Instance.ShowGameResumScreen(); ;
        // }
    }
#endif


    public void StartGame()
    {
        Debug.Log("Starting game");
        SpawnPlayer();
        PlacePrefabs();

    }

    // Méthode pour désactiver tous les panneaux et activer celui requis
    public void ShowPanel(GameObject panelToShow)
    {
        // Désactiver tous les panneaux
        foreach (GameObject panel in panelArray)
        {
            panel.SetActive(false);
        }

        // Activer le panneau souhaité
        panelToShow.SetActive(true);
    }

    // Exemple d'utilisation pour afficher le panneau du menu principal
    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }

    // Exemple d'utilisation pour afficher le panneau en jeu
    public void ShowInGamePanel()
    {
        ShowPanel(inGamePanel);
    }

    public void SpawnItemObjectWithDelay(ItemData item, Vector2 position, float delay)
    {
        StartCoroutine(SpawnItemObjectDelayed(item, position, delay));
    }

    private IEnumerator SpawnItemObjectDelayed(ItemData item, Vector2 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        InstantiateItem(item, position);
    }

    public void SpawnItemObject(ItemData item, Vector2 position)
    {
        InstantiateItem(item, position);
    }



    public void InstantiateItemSinglePlayer(int itemId, Vector2 position)
    {
        ItemData item = ItemDatabase.Instance.GetItemById(itemId);
        if (item != null)
        {
            GameObject itemOnFloor = Instantiate(ItemDatabase.Instance.prefab, position, Quaternion.identity);
            ItemOnFloorManager itemManager = itemOnFloor.GetComponent<ItemOnFloorManager>();

            if (itemManager != null)
            {
                itemManager.item = item;
            }
            else
            {
                Debug.LogError("ItemOnFloorManager component is missing on the instantiated prefab.");
            }

            if (itemOnFloor != null)
                itemOnFloor.transform.DOScale(1, 0.5f).From(0.75f).SetEase(Ease.OutBack);
        }
        else
        {
            Debug.LogError("Item not found");
        }
    }

    public void InstantiateItem(ItemData item, Vector2 position)
    {

        InstantiateItemSinglePlayer(item.itemId, position);

    }

    // -357 182 top left
    // 323 -168 bottom right

    void SpawnPlayer()
    {
        Instantiate(playerPrefab);
    }

    void PlacePrefabs()
    {
        foreach (GameObject prefab in prefabsToPlace)
        {
            for (int i = 0; i < maxAttemptsPerPrefab; i++)
            {
                // Generate a random position within the defined rectangle
                float x = Random.Range(topLeftCoords.x, bottomRightCoords.x);
                float y = Random.Range(bottomRightCoords.y, topLeftCoords.y);

                Vector3 spawnPosition = new Vector3(x, y, 0f);

                // Check if there are any colliders within minDistanceBetweenPrefabs
                Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, minDistanceBetweenPrefabs);
                bool collisionDetected = false;
                foreach (Collider2D collider in colliders)
                {
                    // Check if the collider belongs to another prefab
                    if (!collider.gameObject.CompareTag("Untagged"))
                    {
                        collisionDetected = true;
                        break;
                    }
                }

                // If no collision detected, instantiate the prefab and break the loop
                if (!collisionDetected)
                {
                    GameObject prefabToPlace = Instantiate(prefab, spawnPosition, Quaternion.identity);


                    break;
                }
            }
        }
    }


}

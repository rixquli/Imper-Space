using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialHelper : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private GraphicRaycaster raycasterMain;

    [SerializeField]
    private GameObject eventSystem;
    private bool hadFinishTuto = false;
    private bool hadDidTheSpaceBaseTutoInLobby = false;

    [SerializeField]
    private bool inGameTuto = false;

    [SerializeField]
    private bool inGameExterminationTuto = false;

    private bool hadStartTutoInGameExtermination = false;
    private bool hadStartTutoInGame = false;

    [SerializeField]
    private bool inSpaceBaseTuto = false;
    private bool hadStartTutoInSpaceBase = false;

    [SerializeField]
    public string scenePath;

    [Header("For In Game Tutorial")]
    [SerializeField]
    private GameObject enemyPrefab;

    public bool blockTuto = false;

    private void Start()
    {
        RessourcesManager.Instance.OnValueChanged += OnValueChanged;
    }

    public async void StartTuto(bool bruteForce = false)
    {
        if (!bruteForce && hadFinishTuto)
        {
            return;
        }
        else
        {
            hadFinishTuto = true;
        }

        if (!bruteForce && hadDidTheSpaceBaseTutoInLobby)
        {
            return;
        }
        else
        {
            hadDidTheSpaceBaseTutoInLobby = true;
        }

        // Charger la scène "Tuto" en mode additive
        if (TutorialManager.Instance == null)
            await LoadTutorialSceneAsync();

        // Passez la référence du raycaster à l'instance du gestionnaire de tutoriel
        TutorialManager.Instance.raycasterMain = raycasterMain;
        TutorialManager.Instance.mainCamera = mainCamera;
        if (eventSystem != null)
            eventSystem.SetActive(false);
        if (inGameTuto || inGameExterminationTuto)
        {
            TutorialManager.Instance.cameraLogic = mainCamera.GetComponent<FollowPlayer>();

            if (inGameExterminationTuto)
                Invoke(nameof(SpawnEnemy), 10f);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
            return;
        GameObject enemy = Instantiate(enemyPrefab, new Vector2(0, 30), Quaternion.identity);
        if (enemy.TryGetComponent<ICountable>(out var countable))
        {
            countable.OnDeath += (
                () =>
                {
                    TutorialManager.Instance.FirstKill();
                }
            );
        }
    }

    private async Task LoadTutorialSceneAsync()
    {
        // Utilisation de LoadSceneAsync pour charger la scène de tutoriel
        var loadOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

        // Attendre que la scène soit complètement chargée
        while (!loadOperation.isDone)
        {
            await Task.Yield();
        }
    }

    private void OnValueChanged()
    {
        if (!inGameTuto && !inSpaceBaseTuto && !inGameExterminationTuto)
        {
            hadFinishTuto = DataPersistenceManager.instance.gameData.hadDidTheLobbyTuto;
            if (!hadFinishTuto && !blockTuto)
            {
                StartTuto();
                hadFinishTuto = true;
            }
            // if (
            //     hadFinishTuto
            //     && !DataPersistenceManager.instance.gameData.hadDidTheSpaceBaseTutoInLobby
            // )
            // {
            //     scenePath = "TutoLobbySpaceBase";
            //     hadDidTheSpaceBaseTutoInLobby = true;
            //     StartTuto();
            // }
        }
        else
        if (inGameExterminationTuto)
        {
            if (StaticDataManager.levelNumber == 1 && !hadStartTutoInGameExtermination)
            {
                hadStartTutoInGameExtermination = true;
                StartTuto();
            }
        }
        else if (inGameTuto)
        {
            if (StaticDataManager.levelNumber == 2 && !hadStartTutoInGame)
            {
                hadStartTutoInGame = true;
                StartTuto();
            }
        }
        else if (inSpaceBaseTuto)
        {
            hadStartTutoInSpaceBase = DataPersistenceManager
                .instance
                .gameData
                .hadDidTheSpaceBaseTuto;
            if (!hadStartTutoInSpaceBase)
            {
                hadStartTutoInSpaceBase = true;
                StartTuto();
            }
        }
    }

    public void LoadData(GameData data)
    {
        if (!inGameTuto && !inSpaceBaseTuto && !inGameExterminationTuto)
        {
            hadFinishTuto = data.hadDidTheLobbyTuto;
            if (!hadFinishTuto && !blockTuto)
            {
                StartTuto();
                hadFinishTuto = true;
                RessourcesManager.Instance.StartWaitForSave();
            }
            // if (hadFinishTuto && !data.hadDidTheSpaceBaseTutoInLobby)
            // {
            //     scenePath = "TutoLobbySpaceBase";
            //     hadDidTheSpaceBaseTutoInLobby = true;
            //     StartTuto();
            // }
        }
        else if (inGameExterminationTuto)
        {
            if (StaticDataManager.levelNumber == 1 && !hadStartTutoInGameExtermination)
            {
                hadStartTutoInGameExtermination = true;
                StartTuto();
                RessourcesManager.Instance.StartWaitForSave();
            }
        }
        else if (inGameTuto)
        {
            if (StaticDataManager.levelNumber == 2 && !hadStartTutoInGame)
            {
                hadStartTutoInGame = true;
                StartTuto();
                RessourcesManager.Instance.StartWaitForSave();
            }
        }
        else if (inSpaceBaseTuto)
        {
            hadStartTutoInSpaceBase = data.hadDidTheSpaceBaseTuto;
            if (!hadStartTutoInSpaceBase)
            {
                hadStartTutoInSpaceBase = true;
                StartTuto();
                RessourcesManager.Instance.StartWaitForSave();
            }
        }
    }

    public void SaveData(GameData data)
    {
        if (!inGameTuto && !inSpaceBaseTuto && !inSpaceBaseTuto)
        {
            if (hadFinishTuto)
                data.hadDidTheLobbyTuto = hadFinishTuto;
            // data.hadDidTheSpaceBaseTutoInLobby = hadDidTheSpaceBaseTutoInLobby;
        }
        if (inSpaceBaseTuto)
        {
            if (hadStartTutoInSpaceBase)
                data.hadDidTheSpaceBaseTuto = hadStartTutoInSpaceBase;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackSelectionManager : MonoBehaviour
{
    [SerializeField] private Button openMapButton;
    [SerializeField] private Button closeMapButton;
    [SerializeField] private RectTransform markerObject;
    [SerializeField] private Button startButton;

    [Header("Level Description Elements")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI levelEnemiesLevelText;
    [SerializeField] private TextMeshProUGUI levelMissionTypeText;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TextMeshProUGUI rewardAmountRewardText;
    [SerializeField] private Sprite gemSprite;
    [SerializeField] private Sprite goldSprite;

    [Header("Alert Confirm Elements")]
    [SerializeField] private GameObject alertConfirmPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;


    private VillagePointInVillageSelection[] levelPointInLevelSelections;
    private EnemyVillages selectedLevel;

    private bool hasAlreadySelectedLevel = false;

    private void Awake()
    {
        gameObject.SetActive(false);

        openMapButton.onClick.AddListener(() =>
        {
            if (VillageMovingManager.Instance.isMoving) return;
            gameObject.SetActive(true);
        });
        closeMapButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        levelPointInLevelSelections = GetComponentsInChildren<VillagePointInVillageSelection>(true);
        SettupButtons();
        startButton.onClick.AddListener(CheckIfCanAttack);

        alertConfirmPanel.SetActive(false);
    }

    private void Start()
    {
        SelectLevelOnStart();
    }

    private void SettupButtons()
    {
        foreach (VillagePointInVillageSelection level in levelPointInLevelSelections)
        {
            if (level.button != null)
            {
                level.button.onClick.AddListener(() =>
                {
                    SelectLevel(level);
                });
            }
        }
    }

    private void SelectLevel(VillagePointInVillageSelection village)
    {
        if (!village.hasReached) return;
        MoveMarker(village.transform.position);
        EnemyVillages currentLevel = RessourcesManager.Instance.enemyVillages.Find(l => l.data == village.villageData);
        selectedLevel = currentLevel;
        UpdateRightCard();
        if (!hasAlreadySelectedLevel)
        {
            hasAlreadySelectedLevel = true;
            GetComponentInChildren<CustomScrollRect>()?.ScrollToTargetChild(village.transform.parent);
        }
    }

    private string GetMissionText(GameMode gameMode)
    {
        return gameMode switch
        {
            GameMode.Classic => "Protect the base",
            GameMode.Extermination => "Extermination",
            // Add more game modes here as needed
            _ => "Unknown Mission" // Default case
        };
    }

    private void UpdateRightCard()
    {
        if (selectedLevel != null)
        {
            if (levelNameText != null)
                levelNameText.text = selectedLevel.data.villageName;

            if (levelEnemiesLevelText != null)
                levelEnemiesLevelText.text = $"Enemies lvl: {selectedLevel.data.enemiesMinLevel}-{selectedLevel.data.enemiesMaxLevel}";

            // if (levelMissionTypeText != null)
            // {
            //     string missionText = GetMissionText(selectedLevel.data.gameMode);
            //     levelMissionTypeText.text = $"Mission: {missionText}";
            // }

            if (rewardIcon != null)
            {
                switch (selectedLevel.data.gainSubType)
                {
                    case RessourceType.Gold:
                        rewardIcon.sprite = goldSprite;
                        break;
                    case RessourceType.Gem:
                        rewardIcon.sprite = gemSprite;
                        break;
                    default:
                        break;
                }
            }

            if (rewardAmountRewardText != null)
                rewardAmountRewardText.text = Mathf.RoundToInt((float)selectedLevel.data.gainAmount).ToString();
        }
    }

    private void SelectLevelOnStart()
    {
        // RessourcesManager.Instance.enemyVillages = data.RessourcesManager.Instance.enemyVillages;

        if (levelPointInLevelSelections == null) return;

        for (int i = 0; i < levelPointInLevelSelections.Length; i++)
        {
            // Check if levels list is null or empty
            if (RessourcesManager.Instance.enemyVillages == null || RessourcesManager.Instance.enemyVillages.Count == 0)
                return;

            var levels = RessourcesManager.Instance.enemyVillages;
            var currentLevelSelection = levelPointInLevelSelections[i];
            int requiredLevelNumber = currentLevelSelection.villageData.requiredVillageNumber;

            // Ensure the required level number is within bounds
            if (levels == null || requiredLevelNumber < 0 || requiredLevelNumber >= levels.Count)
                continue;

            EnemyVillages requiredLevel = levels.Find(l => l?.data?.villageNumber == requiredLevelNumber);

            // Update hasReached if the required level is completed or if it's the first level
            if ((requiredLevel != null && requiredLevel.hasCompleted) || currentLevelSelection.villageData.villageNumber == 1)
            {
                // Directly modify the hasReached property
                currentLevelSelection.hasReached = true;
            }
            if (requiredLevel != null && currentLevelSelection.villageData.villageNumber > 1)
            {
                currentLevelSelection.previousLevelPosition = currentLevelSelection.transform.position;
            }
        }


        foreach (VillagePointInVillageSelection level in levelPointInLevelSelections)
        {
            if (RessourcesManager.Instance.enemyVillages?.Count == 0)
                return;

            EnemyVillages villageData = RessourcesManager.Instance.enemyVillages.Find(l => l.data == level.villageData);
            if (villageData != null && !villageData.hasCompleted && villageData.hasReached)
            {
                SelectLevel(level);
            }
        }
    }

    private void MoveMarker(Vector2 position)
    {
        markerObject.position = position + new Vector2(0, 25);
    }

    private void CheckIfCanAttack()
    {
        if (selectedLevel == null) return;
        if (VillageRessourcesManager.Instance.GetTroopAmount() < 10)
        {
            alertConfirmPanel.SetActive(true);
            confirmButton.onClick.AddListener(() =>
            {
                alertConfirmPanel.SetActive(false);
                StartGame();
            });
            cancelButton.onClick.AddListener(() =>
            {
                alertConfirmPanel.SetActive(false);
            });
        }
        else
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        if (ReferenceEquals(selectedLevel, null) || selectedLevel.data == null) return;
        VillageInVillageSelectionData selectedLevelData = selectedLevel.data;
        VillagesDataManager.Instance.SetVillageData(selectedLevelData.BuildingList, selectedLevelData.enemyToSpawn, selectedLevelData.enemiesMinLevel, selectedLevelData.enemiesMaxLevel, selectedLevelData.gainSubType, selectedLevelData.gainAmount, selectedLevelData.villageNumber);

        Loader.Load(Loader.Scene.Defense_Village);
    }
}

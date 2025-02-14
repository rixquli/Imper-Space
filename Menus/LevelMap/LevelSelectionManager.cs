using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
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


    private LevelPointInLevelSelection[] levelPointInLevelSelections;
    private Levels selectedLevel;

    private bool hasAlreadySelectedLevel = false;

    private void Awake()
    {
        levelPointInLevelSelections = GetComponentsInChildren<LevelPointInLevelSelection>(true);
        SettupButtons();
        startButton.onClick.AddListener(StartGame);
    }

    private void Start()
    {
        SelectLevelOnStart();
    }

    private void SettupButtons()
    {
        foreach (LevelPointInLevelSelection level in levelPointInLevelSelections)
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

    private void SelectLevel(LevelPointInLevelSelection level)
    {
        if (!level.hasReached) return;
        MoveMarker(level.transform.position);
        Levels currentLevel = RessourcesManager.Instance.levels.Find(l => l.data == level.levelData);
        selectedLevel = currentLevel;
        UpdateRightCard();
        if (!hasAlreadySelectedLevel)
        {
            hasAlreadySelectedLevel = true;
            GetComponentInChildren<CustomScrollRect>()?.ScrollToTargetChild(level.transform.parent);
        }
    }

    private string GetMissionText(GameMode gameMode)
    {
        return gameMode switch
        {
            GameMode.Classic => "Protect the base",
            GameMode.Extermination => "Extermination",
            GameMode.InfinityMode => "Survive as long as possible",
            GameMode.TurretAssault => "Turret Assault",
            GameMode.FrontLine => "Attack the Front Line",
            // Add more game modes here as needed
            _ => "Unknown Mission" // Default case
        };
    }

    private void UpdateRightCard()
    {
        if (selectedLevel != null)
        {
            LevelInLevelSelectionData levelData = selectedLevel.data;
            if (levelNameText != null)
                levelNameText.text = levelData.levelName;

            if (levelEnemiesLevelText != null)
                levelEnemiesLevelText.text = $"Enemies lvl: {levelData.enemiesMinLevel}-{levelData.enemiesMaxLevel}";

            if (levelMissionTypeText != null)
            {
                string missionText = GetMissionText(levelData.gameMode);
                levelMissionTypeText.text = $"Mission: {missionText}";
            }

            if (rewardIcon != null)
            {
                switch (levelData.gainSubType)
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
                rewardAmountRewardText.text = Mathf.RoundToInt((float)levelData.gainAmount).ToString();
        }
    }

    private void SelectLevelOnStart()
    {
        // RessourcesManager.Instance.levels = data.RessourcesManager.Instance.levels;

        if (levelPointInLevelSelections == null) return;

        for (int i = 0; i < levelPointInLevelSelections.Length; i++)
        {
            // Check if levels list is null or empty
            if (RessourcesManager.Instance.levels == null || RessourcesManager.Instance.levels.Count == 0)
                return;

            var levels = RessourcesManager.Instance.levels;
            var currentLevelSelection = levelPointInLevelSelections[i];
            int requiredLevelNumber = currentLevelSelection.levelData.requiredLevelNumber;

            // Ensure the required level number is within bounds
            if (levels == null || requiredLevelNumber < 0 || requiredLevelNumber >= levels.Count)
                continue;

            Levels requiredLevel = levels.Find(l => ((LevelInLevelSelectionData)l?.data)?.levelNumber == requiredLevelNumber);

            // Update hasReached if the required level is completed or if it's the first level
            if ((requiredLevel != null && requiredLevel.hasCompleted) || currentLevelSelection.levelData.levelNumber == 1)
            {
                // Directly modify the hasReached property
                currentLevelSelection.hasReached = true;
            }
            if (requiredLevel != null && currentLevelSelection.levelData.levelNumber > 1)
            {
                currentLevelSelection.previousLevelPosition = currentLevelSelection.transform.position;
            }
        }


        if (RessourcesManager.Instance.levels?.Count == 0)
            return;

        LevelPointInLevelSelection levelToPing = null;

        foreach (LevelPointInLevelSelection level in levelPointInLevelSelections)
        {
            Levels levelData = RessourcesManager.Instance.levels.Find(l => l.data == level.levelData);
            if (levelData != null && !levelData.hasCompleted && levelData.hasReached)
            {
                // Select the level with the smallest level number
                if (levelToPing == null || levelData.data.levelNumber < levelToPing.levelData.levelNumber)
                {
                    levelToPing = level;
                }
            }
        }
        SelectLevel(levelToPing);
    }

    private void MoveMarker(Vector2 position)
    {
        markerObject.position = position + new Vector2(0, 25);
    }

    private void StartGame()
    {
        LevelInLevelSelectionData levelData = selectedLevel.data;
        if (ReferenceEquals(selectedLevel, null) || levelData == null) return;
        StaticDataManager.levelNumber = levelData.levelNumber;
        StaticDataManager.minEnemiesLevel = levelData.enemiesMinLevel;
        StaticDataManager.maxEnemiesLevel = levelData.enemiesMaxLevel;
        StaticDataManager.gameMode = levelData.gameMode;
        StaticDataManager.victoryRewardType = levelData.gainSubType;
        StaticDataManager.victoryRewardAmount = levelData.gainAmount;

        StaticDataManager.IsSinglePlayer = true;
        if (levelData.gameMode == GameMode.Classic)
        {
            Loader.Load(Loader.Scene.Game);
        }
        else if (levelData.gameMode == GameMode.Extermination)
        {
            Loader.Load(Loader.Scene.Game_Extermination);
        }
        else if (levelData.gameMode == GameMode.InfinityMode)
        {
            Loader.Load(Loader.Scene.Game_Infinity_Mode);
        }
        else if (levelData.gameMode == GameMode.TurretAssault)
        {
            Loader.Load(Loader.Scene.Game_TurretAssault);
        }
        else if (levelData.gameMode == GameMode.FrontLine)
        {
            Loader.Load(Loader.Scene.Game_Front);
        }
    }
}

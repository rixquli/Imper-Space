using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class XPManager : MonoBehaviour, IDataPersistence
{
    public static XPManager Instance;

    public enum Stats
    {
        Speed,
        AttackDamage,
        Health,
        HealthRegenSpeed,
        Shield,
        ShieldRegenSpeed,
    }

    [SerializeField]
    private float level = 1;

    [SerializeField]
    private float xp = 0;
    public float Xp
    {
        get { return totalXp; }
    }

    [SerializeField]
    private float xpToNextLevel = 100;

    [SerializeField]
    private GameObject XPMenu;

    [SerializeField]
    private GameObject XPStatPrefab;

    public Dictionary<Stats, int> StatsDictionary = new Dictionary<Stats, int>();
    public Dictionary<Stats, GameObject> StatsDictionaryGameObject =
        new Dictionary<Stats, GameObject>();

    public event Action<Stats, int> OnStatsChange;

    private bool isSetup = false;
    public int pointsToAttributes = 6;
    private bool hasAlreadyCloseStatPanelOnce = false;

    // For Battle Pass
    private float totalXp = 0;
    private float totalXpToSave = 0;
    public bool hadSaved = true;
    private bool canSave = false;

    // [SerializeField] private GameObject TutoEnableAutoModeObject;

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
        HideMenu();
        UpdateProgressBar();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        // InitializeStatsDictionary();
    }

    private void UpdateProgressBar_OnValueChanged(float previousValue, float newValue)
    {
        UpdateProgressBar();
    }

    private void InitializeStatsDictionary()
    {
        Transform parentTransform = XPMenu.transform.Find("Background");

        foreach (Transform child in parentTransform)
        {
            // Destroy each child game object
            Destroy(child.gameObject);
        }

        foreach (Stats stat in System.Enum.GetValues(typeof(Stats)))
        {
            GameObject XPStatObject = Instantiate(XPStatPrefab, parentTransform);
            StatsDictionaryGameObject[stat] = XPStatObject;

            SetupButton(XPStatObject, stat);
            SetupText(XPStatObject, stat.ToString());
            SetupPoints(XPStatObject, StatsDictionary[stat]);
        }
        // prevent open menu
        /*if (!isSetup)
        {
            isSetup = true;
            ShowMenu();
        }*/
    }

    public void SetStats(Dictionary<Stats, int> keyValuePairs)
    {
        // Update the serializable dictionary
        StatsDictionary.Clear();
        foreach (var kvp in keyValuePairs)
        {
            StatsDictionary[kvp.Key] = kvp.Value;
        }
        InitializeStatsDictionary();
    }

    private void SetupButton(GameObject XPStatObject, Stats stat)
    {
        Button[] btns = XPStatObject.GetComponentsInChildren<Button>();
        if (btns != null && btns.Length > 0)
        {
            Stats localStat = stat; // Local copy for lambda capture
            foreach (Button btn in btns)
            {
                if (btn.gameObject.name == "Add")
                {
                    btn.onClick.AddListener(() => Add_Button_Click(localStat));
                }
                else if (btn.gameObject.name == "Minus")
                {
                    btn.onClick.AddListener(() => Minus_Button_Click(localStat));
                }
            }
        }
        else
        {
            Debug.LogError("Button component not found in the instantiated item.");
        }
    }

    private void SetupText(GameObject XPStatObject, string statName)
    {
        TextMeshProUGUI textComponent = XPStatObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = statName;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in the instantiated item.");
        }
    }

    private void SetupPoints(GameObject XPStatObject, int pointsToProcess)
    {
        GameObject[] points = FindAllChildObjectsByParentName(
            XPStatObject.transform.Find("Background"),
            "Points"
        );
        if (points.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Image img = points[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(
                        73f / 255,
                        176f / 255,
                        41f / 255,
                        i < pointsToProcess ? 1f : 0f
                    );
                }
                else
                {
                    Debug.LogError("Image component not found on point object.");
                }
            }
        }
        else
        {
            Debug.LogError("No child objects with name 'Points' found.");
        }

        TextMeshProUGUI lvlText = XPStatObject
            .transform.Find("Background/Level")
            .GetComponent<TextMeshProUGUI>();
        if (lvlText != null)
        {
            lvlText.text = $"[{pointsToProcess}]";
        }
    }

    private GameObject[] FindAllChildObjectsByParentName(Transform parent, string name)
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parent.Find(name))
        {
            children.Add(child.gameObject);
        }

        return children.ToArray();
    }

    private void Add_Button_Click(Stats stat)
    {
        int newLevel = StatsDictionary[stat] + 1;

        Debug.Log("newLevel = " + newLevel);

        if (newLevel <= 10)
        {
            if (pointsToAttributes > 0)
            {
                pointsToAttributes--;
                StatsDictionary[stat] = newLevel;
                SetupPoints(StatsDictionaryGameObject[stat], StatsDictionary[stat]);
                OnStatsChange?.Invoke(stat, newLevel);
                if (pointsToAttributes <= 0)
                    HideMenu();
            }
            else
            {
                HideMenu();
            }
        }
        else
        {
            Debug.LogWarning($"Stat '{stat}' has reached the maximum level.");
        }
    }

    private void Minus_Button_Click(Stats stat)
    {
        int newLevel = StatsDictionary[stat] - 1;

        Debug.Log("newLevel = " + newLevel);

        if (newLevel >= 0)
        {
            pointsToAttributes++;
            StatsDictionary[stat] = newLevel;
            SetupPoints(StatsDictionaryGameObject[stat], StatsDictionary[stat]);
            OnStatsChange?.Invoke(stat, newLevel);
            if (pointsToAttributes <= 0)
                HideMenu();
        }
        else
        {
            Debug.LogWarning($"Stat '{stat}' has reached the maximum level.");
        }
    }

    public void AddXP(float xp)
    {
        this.xp += xp;
        totalXp += xp;

        if (this.xp >= xpToNextLevel)
        {
            level++;
            pointsToAttributes++;
            float multiplier = Mathf.Pow(1.2f, level - 1);
            this.xp = 0;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * multiplier);
            //ShowMenu();
        }
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (UIController.Instance != null)
            UIController.Instance.SetXpBar(xp, xpToNextLevel);
    }

    public void ShowMenu()
    {
        //TODO: il ne s'affiche pas chez le client
        XPMenu.SetActive(true);
        StaticDataManager.Pause(true);
    }

    public void HideMenu()
    {
        XPMenu.SetActive(false);
        StaticDataManager.Pause(false);
        if (!hasAlreadyCloseStatPanelOnce)
        {
            hasAlreadyCloseStatPanelOnce = true;
            if (StaticDataManager.levelNumber != 1 && StaticDataManager.levelNumber != 2)
            {
                StaticDataManager.StartGameplay();
            }
            else
            {
                if (TutorialManager.Instance != null)
                    TutorialManager.Instance.HadCloseStatPanelOnce();
            }

            // if(VillagesDataManager.Instance != null && VillagesDataManager.Instance.villageNumber == 1 && TutoEnableAutoModeObject != null)
            // {
            //     TutoEnableAutoModeObject.SetActive(true);
            // }
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        totalXpToSave = totalXp;
        if (StaticDataManager.isVictory)
            totalXp += StaticDataManager.maxEnemiesLevel * 2;
        canSave = true;
        DataPersistenceManager.instance.SaveGame();
    }

    public void LoadData(GameData data) { }

    public void SaveData(GameData data)
    {
        if (!canSave || hadSaved || totalXpToSave == 0)
            return;
        data.battlePassXp += totalXpToSave;
        hadSaved = true;
    }
}

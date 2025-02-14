using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipementsPanelManager : MonoBehaviour
{
    [Header("Buttons of each part")]
    [SerializeField] private UDictionary<Button, GameObject> partsWithButtons;

    [SerializeField] private Button equipementsButton;

    [SerializeField] private Button shipsButton;

    [Header("Panels of each part")]
    [SerializeField] private GameObject equipementsPanelObject;

    [SerializeField] private GameObject shipsPanelObject;


    [Header("Info/Equip/Unequip buttons")]
    [SerializeField] private GameObject selectedMenuObject;
    [SerializeField] private Button infoObject;
    [SerializeField] private Button equipObject;
    [SerializeField] private Button unequipObject;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI levelTextSelectedPanel;

    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private TextMeshProUGUI shieldText;

    [SerializeField] private TextMeshProUGUI attackText;

    [SerializeField] private TextMeshProUGUI regenHealthText;

    [SerializeField] private TextMeshProUGUI regenShieldText;

    [SerializeField] private TextMeshProUGUI speedText;

    [SerializeField] private TextMeshProUGUI firingModeText;

    [Header("Info Popup")]
    [SerializeField] private StatInfoPopupController statInfoPopupController;

    [Header("Upgrade Panel")]
    [SerializeField] private UpgradePanelController upgradePanelObject;

    private Color unselectedColor = new Color(100f / 255f, 100f / 255f, 100f / 255f);
    private Color selectedColor = new Color(0 / 255f, 0 / 255f, 0 / 255f);
    private int currentInventoryPart = 0;
    private List<Button> partButtonsList;

    private UDictionary<EquipeSlot, EquipementData> equipedEquipements;
    private Dictionary<EquipeSlot, EquipementData> currentEquipement = new();
    private AttributesData playerAttributesData = new();

    private EquipementData currentSelectedEquipementForInfo;


    private void Awake()
    {
        equipementsPanelObject.SetActive(true);
        shipsPanelObject.SetActive(false);
        upgradePanelObject.Hide();
        selectedMenuObject.SetActive(false);
        equipObject.gameObject.SetActive(false);
        unequipObject.gameObject.SetActive(false);

        // equipementsButton.onClick.AddListener(EquipementsButtonOnClick);
        // shipsButton.onClick.AddListener(ShipsButtonOnClick);
        // upgradeButton.onClick.AddListener(UpgradeButtonOnClick);

        infoObject.onClick.AddListener(InfoButtonClicked);
        equipObject.onClick.AddListener(EquipeButtonClicked);
        unequipObject.onClick.AddListener(UnequipeButtonClicked);
        upgradeButton.onClick.AddListener(UpgradeButtonOnClick);

        partButtonsList = new List<Button>(partsWithButtons.Keys);
        AddListenerForPartButtons();
        ShowShopPart(currentInventoryPart);

    }

    private void Start()
    {
        equipedEquipements =
            RessourcesManager.Instance.equipedEquipements
            ?? new UDictionary<EquipeSlot, EquipementData>();
        playerAttributesData = RessourcesManager.Instance.playerAttributesData;

        RessourcesManager.Instance.OnValueChanged += OnValueChanged;
        UpdateStatTexts();
    }

    private void OnDestroy()
    {
        RessourcesManager.Instance.OnValueChanged -= OnValueChanged;
    }

    private void OnValueChanged()
    {
        equipedEquipements =
            RessourcesManager.Instance.equipedEquipements
            ?? new UDictionary<EquipeSlot, EquipementData>();
        playerAttributesData = RessourcesManager.Instance.playerAttributesData;

        UpdateStatTexts();
    }

    private void AddListenerForPartButtons()
    {
        foreach (var kvp in partsWithButtons)
        {
            Button button = kvp.Key;
            int index = partButtonsList.IndexOf(button);
            button?.onClick.AddListener(() => ShowShopPart(index));
        }
    }

    private void ShowShopPart(int index)
    {
        currentInventoryPart = index;
        for (int i = 0; i < partsWithButtons.Values.Count; i++)
        {
            partsWithButtons.Values[i].SetActive(i == index);
        }
        UpdatePartButtons();
        HideSelectedMenuButton();
    }

    private void UpdatePartButtons()
    {
        for (int i = 0; i < partsWithButtons.Keys.Count; i++)
        {
            Image image = partsWithButtons.Keys[i].gameObject.GetComponent<Image>();
            image.color = (i == currentInventoryPart) ? selectedColor : unselectedColor;
        }
    }

    private void EquipementsButtonOnClick()
    {
        equipementsPanelObject.SetActive(true);
        shipsPanelObject.SetActive(false);
    }

    private void ShipsButtonOnClick()
    {
        equipementsPanelObject.SetActive(false);
        shipsPanelObject.SetActive(true);
    }

    public void ShowEquipButton(RectTransform targetTransform, EquipeSlot slot, EquipementData data)
    {
        currentSelectedEquipementForInfo = data;
        currentEquipement.Clear();
        currentEquipement[slot] = data;

        selectedMenuObject.SetActive(true);
        equipObject.gameObject.SetActive(true);
        unequipObject.gameObject.SetActive(false);
        levelTextSelectedPanel.text = "lvl. " + RessourcesManager.Instance.GetLevel(data);

        SetRectPosition(selectedMenuObject.GetComponent<RectTransform>(), targetTransform);
    }

    public void ShowUnequipButton(RectTransform targetTransform, EquipeSlot slot, EquipementData data)
    {
        currentSelectedEquipementForInfo = data;
        currentEquipement.Clear();
        currentEquipement[slot] = null;

        selectedMenuObject.SetActive(true);
        equipObject.gameObject.SetActive(false);
        unequipObject.gameObject.SetActive(true);
        levelTextSelectedPanel.text = "lvl. " + RessourcesManager.Instance.GetLevel(data);

        SetRectPosition(selectedMenuObject.GetComponent<RectTransform>(), targetTransform);
    }

    public void ShowInfoButton(RectTransform targetTransform, EquipeSlot slot, EquipementData data)
    {
        currentSelectedEquipementForInfo = data;
        currentEquipement.Clear();
        currentEquipement[slot] = data;

        selectedMenuObject.SetActive(true);
        equipObject.gameObject.SetActive(false);
        unequipObject.gameObject.SetActive(false);
        levelTextSelectedPanel.text = "lvl. " + RessourcesManager.Instance.GetLevel(data);

        SetRectPosition(selectedMenuObject.GetComponent<RectTransform>(), targetTransform);
    }

    private void SetRectPosition(RectTransform rectTransform, RectTransform targetTransform)
    {
        // Get screen dimensions
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Get target corners in screen space
        Vector3[] corners = new Vector3[4];
        targetTransform.GetWorldCorners(corners);
        Vector3 targetCenter = (corners[0] + corners[2]) / 2f; // Center point

        // Check if target is in the right half of the screen
        bool isOnRightSide = targetCenter.x > screenSize.x / 2;
        // Check if target is in the top half of the screen
        bool isOnTopSide = targetCenter.y > screenSize.y / 2;

        // Set pivot based on position
        if (isOnRightSide && isOnTopSide)
        {
            // If target is in top-right, place menu at bottom-left
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.position = corners[0]; // Bottom-left corner
        }
        else if (isOnRightSide)
        {
            // If target is in bottom-right, place menu at top-left
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.position = corners[1]; // Top-left corner
        }
        else if (isOnTopSide)
        {
            // If target is in top-left, place menu at bottom-right
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.position = corners[3]; // Bottom-right corner
        }
        else
        {
            // If target is in bottom-left, place menu at top-right
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.position = corners[2]; // Top-right corner
        }

        // Add a small offset
        rectTransform.anchoredPosition += new Vector2(isOnRightSide ? -10 : 10, isOnTopSide ? -10 : 10);
    }

    public void HideSelectedMenuButton()
    {
        selectedMenuObject.SetActive(false);
        equipObject.gameObject.SetActive(false);
        unequipObject.gameObject.SetActive(false);
    }

    private void EquipeButtonClicked()
    {
        // Ensure currentEquipement contains exactly one item
        if (currentEquipement.Count == 1)
        {
            // Get the key and value from currentEquipement
            var currentKey = currentEquipement.Keys.First();
            var currentValue = currentEquipement.Values.First();

            Debug.LogWarning("currentKey" + currentKey);
            Debug.LogWarning("currentValue" + currentValue);

            RessourcesManager.Instance.SetEquipedEquipments(currentKey, currentValue);
            currentEquipement.Clear();
            HideSelectedMenuButton();
            // RessourcesManager.Instance.StartWaitForReloading();
        }
    }

    IEnumerator WaitForSaving()
    {
        yield return null;
        DataPersistenceManager.instance.SaveGame();
    }

    IEnumerator WaitForLoading()
    {
        yield return null;
        DataPersistenceManager.instance.LoadGame();
    }

    IEnumerator WaitForReloading()
    {
        yield return null;
        StartCoroutine(WaitForSaving());
        StartCoroutine(WaitForLoading());
    }

    private void UnequipeButtonClicked()
    {
        // Ensure currentEquipement contains exactly one item
        Debug.Log("UnequipeButtonClicked");
        if (currentEquipement.Count == 1)
        {
            // Get the key from currentEquipement
            var currentKey = currentEquipement.Keys.First();
            Debug.Log("currentKey: " + currentKey);
            Debug.Log("equipedEquipements: " + equipedEquipements);

            RessourcesManager.Instance.SetEquipedEquipments(currentKey, null);
            currentEquipement.Clear();
            HideSelectedMenuButton();
            RessourcesManager.Instance.StartWaitForReloading();
            // StartCoroutine(WaitForReloading());
        }
    }

    private void UpgradeButtonOnClick()
    {
        if (currentEquipement.Count == 1)
        {
            var currentKey = currentEquipement.Values.First();
            upgradePanelObject.Show(currentKey);
        }
    }

    private void InfoButtonClicked()
    {
        // Ensure currentEquipement contains exactly one item
        if (currentEquipement.Count == 1)
        {
            // Get the value from currentEquipement
            Debug.Log("currentValue: " + currentSelectedEquipementForInfo);

            statInfoPopupController.OpenInfoPopUp(currentSelectedEquipementForInfo);
        }
    }

    private void UpdateStatTexts()
    {
        // Reset stats
        if (playerAttributesData == null)
            playerAttributesData = new();
        playerAttributesData.UpdateAttribute();

        Debug.Log("Health: " + playerAttributesData.health);

        healthText.text = GetStatToText(playerAttributesData.health);
        shieldText.text = GetStatToText(playerAttributesData.shield);
        attackText.text = GetStatToText(playerAttributesData.attack);
        regenHealthText.text = GetStatToText(playerAttributesData.regenHealth);
        regenShieldText.text = GetStatToText(playerAttributesData.regenShield);
        speedText.text = GetStatToText(playerAttributesData.speed);
        firingModeText.text = GetStatToText(playerAttributesData.bulletPerSecond);

        RessourcesManager.Instance.SetPlayerAttributes(playerAttributesData);
    }

    private string GetStatToText(float value)
    {
        return Mathf.RoundToInt(value).ToString();
    }
}

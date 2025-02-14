using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenusManager : MonoBehaviour
{
    [Header("Main Menus Panel Object")]
    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject inventoryMenuObject;
    [SerializeField] private GameObject shopMenuObject;

    [Header("Settings Menu Button")]
    [SerializeField] private Button settingsButton;

    [Header("Other Menus Panel Object")]
    [SerializeField] private GameObject settingsMenuObject;
    [SerializeField] private GameObject levelSelectionsPanelObject;
    [SerializeField] private GameObject modeSelectionPanelObject;
    [SerializeField] private GameObject battlePassPanelObject;

    [Header("Other Menus Button")]
    [SerializeField] private Button openModeSelectionButton;
    [SerializeField] private Button closeModeSelectionButton;
    [SerializeField] private Button openLevelSelectionsButton;
    [SerializeField] private Button closeLevelSelectionsButton;
    [SerializeField] private Button openBattlePassButton;
    [SerializeField] private Button closeBattlePassButton;
    [SerializeField] private Button goToPlayerVillageButton;

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;

    public RectTransform[] mainPanels;
    public const float transitionDuration = 0.5f;  // Duration of the transition
    private int currentPanelIndex = 0;
    private float canvasWidth;

    private void Awake()
    {
        mainPanels = new RectTransform[3];
        mainPanels[0] = inventoryMenuObject.GetComponent<RectTransform>();
        mainPanels[1] = mainMenuObject.GetComponent<RectTransform>();
        mainPanels[2] = shopMenuObject.GetComponent<RectTransform>();

        modeSelectionPanelObject.SetActive(false);
        openModeSelectionButton.onClick.AddListener(OpenModeSelectionButton_Click);
        closeModeSelectionButton.onClick.AddListener(CloseModeSelectionButton_Click);

        levelSelectionsPanelObject.SetActive(false);
        openLevelSelectionsButton.onClick.AddListener(OpenLevelsSelectionButton_Click);
        closeLevelSelectionsButton.onClick.AddListener(CloseLevelsSelectionButton_Click);

        battlePassPanelObject.SetActive(false);
        openBattlePassButton.onClick.AddListener(OpenBattlePassButton_Click);
        closeBattlePassButton.onClick.AddListener(CloseBattlePassButton_Click);

        goToPlayerVillageButton.onClick.AddListener(GoToPlayerVillageButton_Click);

        settingsMenuObject.SetActive(false);
        settingsButton.onClick.AddListener(OpenSettingsButton_Click);
    }

    void Start()
    {
        // Get the width of the canvas
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        canvasWidth = canvasRectTransform.rect.width;

        // Ensure panels are in the correct starting position
        for (int i = 0; i < mainPanels.Length; i++)
        {
            mainPanels[i].anchoredPosition = new Vector2(canvasWidth * i, 0);
        }

        NavigateToPanel(1, 0f);

        // StartCoroutine(WaitForBanier());
    }

    private IEnumerator WaitForBanier()
    {
        yield return new WaitForSeconds(1f);
        AdmobManager.Instance.LoadShowBanner();
        // AdsManager.Instance.bannerAds.ShowBannerAd();
    }

    private void OpenSettingsButton_Click()
    {
        settingsMenuObject.SetActive(true);
    }

    private void OpenModeSelectionButton_Click()
    {
        modeSelectionPanelObject.SetActive(true);
    }

    private void CloseModeSelectionButton_Click()
    {
        modeSelectionPanelObject.SetActive(false);
    }

    private void OpenLevelsSelectionButton_Click()
    {
        levelSelectionsPanelObject.SetActive(true);
        modeSelectionPanelObject.SetActive(false);
    }

    private void CloseLevelsSelectionButton_Click()
    {
        levelSelectionsPanelObject.SetActive(false);
    }
    private void OpenBattlePassButton_Click()
    {
        battlePassPanelObject.SetActive(true);
    }

    private void CloseBattlePassButton_Click()
    {
        battlePassPanelObject.SetActive(false);
    }

    private void GoToPlayerVillageButton_Click()
    {
        Loader.Load(Loader.Scene.Player_Village);
    }

    public void NavigateToPanel(int panelIndex, float duration = transitionDuration)
    {
        Debug.Log("Navigating to panel " + panelIndex);
        if (panelIndex < 0 || panelIndex >= mainPanels.Length)
            return;

        // si panelIndex = 1 (main menu) alors on affiche la bannieère de pub sinon on la cache
        if (panelIndex == 1)
        {
            AdmobManager.Instance.ShowBanner();
        }
        else
        {
            AdmobManager.Instance.HideBanner();
        }

        Vector2 targetPosition = new Vector2(canvasWidth * (panelIndex - currentPanelIndex), 0);
        Debug.Log("targetPosition=" + targetPosition);

        foreach (var panel in mainPanels)
        {
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            Vector2 newPosition = new Vector2(rectTransform.anchoredPosition.x - targetPosition.x, rectTransform.anchoredPosition.y);

            // Si la durée est zéro, déplacement instantané
            if (duration == 0)
            {
                rectTransform.anchoredPosition = newPosition;
            }
            else
            {
                // Animation avec DoTween
                // rectTransform.DOAnchorPosX(newPosition.x, duration).SetEase(Ease.InOutCubic);
                rectTransform.anchoredPosition = new Vector2(newPosition.x, rectTransform.anchoredPosition.y);
            }
        }
        currentPanelIndex = panelIndex;
    }

    public void NavigateToPanel(int panelIndex)
    {
        NavigateToPanel(panelIndex, transitionDuration);
    }
}

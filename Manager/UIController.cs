using System.Collections;
using DG.Tweening;
using HUDIndicator;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public enum Environment
    {
        NormalGame,
        Village,
        AttackVillage,
        DefenseVillage,
    }

    public static UIController Instance;
    public Environment environment = Environment.NormalGame;

    [field: SerializeField]
    public PlayerEntity player { get; set; }
    public GameObject resumGameScreen;
    public GameObject victoryScreen;
    public GameObject villageDefenseWinPanel;
    public GameObject villageDefenseGameOverPanel;
    public GameObject villageAttackWinPanel;
    public GameObject villageAttackGameOverPanel;
    public GameObject villageAttackResumGameScreen;
    public GameObject gameOverScreen { get; set; }
    public GameObject playerController;

    [Header("Elements For Village Defense and Attack")]
    [SerializeField] private GameObject deathScreenDuringVillage;

    [SerializeField]
    private IndicatorRenderer indicatorRenderer;
    private SettingsMenu settingsMenu;

    [Header("ProgressBars")]
    [SerializeField]
    private ProgressBar healthBar;

    [SerializeField]
    private ProgressBar shieldBar;

    [SerializeField]
    private ProgressBar waveBar;

    [Header("New Item Notif elements")]
    [SerializeField]
    private GameObject notifParent;

    [SerializeField]
    private GameObject notifPrefab;

    [Header("UI element when player play")]
    [SerializeField]
    private GameObject healthBarObject;

    [SerializeField]
    private GameObject shieldBarObject;

    [SerializeField]
    private GameObject xpBarObject;

    [SerializeField]
    private GameObject waveBarObject;

    [SerializeField]
    private GameObject waveTextObject;

    [SerializeField]
    private GameObject autoModeButton;

    [SerializeField]
    private GameObject bigAutoModeButton;

    [SerializeField]
    private GameObject bigSettingsButton;

    [SerializeField]
    private GameObject smallSettingsButton;

    [Header("Alert elements")]
    [SerializeField]
    private TextMeshProUGUI alertText;

    [SerializeField]
    private float fadeInalertTextDuration = 1.0f;

    [SerializeField]
    private float displayalertTextDuration = 2.0f;

    [SerializeField]
    private float fadeOutalertTextDuration = 1.0f;

    [Header("XP elements")]
    [SerializeField]
    private ProgressBar xpBar;

    [SerializeField]
    private TextMeshProUGUI textUnderProgressBarObject;

    // TODO: Implement all ui functionality here


    public GameObject gameUIGameObject;

    void Awake()
    {
        Instance = this;

        alertText.alpha = 0;

        settingsMenu = FindFirstObjectByType<SettingsMenu>(FindObjectsInactive.Include);

        if (deathScreenDuringVillage != null)
            deathScreenDuringVillage.SetActive(false);
    }

    void Update()
    {
        if (player != null)
        {
            if (player.health != healthBar.BarValue)
            {
                healthBar.maxBarValue = player.maxHealth;
                healthBar.BarValue = player.health;
            }
            if (player.shield != shieldBar.BarValue)
            {
                shieldBar.maxBarValue = player.maxShield;
                shieldBar.BarValue = player.shield;
            }
        }
    }

    public void ShowSettingsMenu()
    {
        if (settingsMenu != null)
            settingsMenu.ShowSettingsMenu();
    }

    public void SetTextUnderProgressBarObject(string newText)
    {
        textUnderProgressBarObject.text = newText;
    }

    public void SetWaveBar(float value, float maxValue)
    {
        if (waveBar == null || waveBar.isActiveAndEnabled == false)
            return;
        waveBar.maxBarValue = maxValue;
        waveBar.BarValue = value;
    }

    public void AddNotifNewItem(ItemData item)
    {
        if (notifPrefab == null)
        {
            Debug.LogError("notifPrefab is not assigned.");
            return;
        }

        if (notifParent == null)
        {
            Debug.LogError("notifParent is not assigned.");
            return;
        }

        GameObject notif = Instantiate(notifPrefab, notifParent.transform);
        UnityEngine.UI.Image imageRenderer = notif.GetComponentInChildren<UnityEngine.UI.Image>();
        TextMeshProUGUI textMeshPro = notif
            .transform.Find("NameItem")
            ?.GetComponent<TextMeshProUGUI>();

        if (imageRenderer != null)
        {
            imageRenderer.sprite = item.visual;
        }
        else
        {
            Debug.LogError("Image component not found in notification prefab.");
        }

        if (textMeshPro != null)
        {
            textMeshPro.text = item.displayName;
        }
        else
        {
            Debug.LogError("TextMeshPro component not found in notification prefab.");
        }

        StartCoroutine(DeleteNotif(notif));
    }

    IEnumerator DeleteNotif(GameObject notif)
    {
        yield return new WaitForSeconds(5f);

        CanvasGroup canvasGroup = notif.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup
                .DOFade(0, 1f)
                .SetEase(Ease.OutQuint)
                .OnComplete(() =>
                {
                    Destroy(notif);
                });
        }
        else
        {
            Destroy(notif);
        }
    }

    public void ShowGameOverScreen()
    {
        StaticDataManager.Pause(true);
        if (gameOverScreen == null || environment != Environment.NormalGame)
        {
            ShowDeathScreenDuringVillage();
            return;
        }
        // Amener l'écran de Game Over au premier plan
        playerController.transform.SetSiblingIndex(gameUIGameObject.transform.childCount - 1);

        // Afficher l'écran de Game Over
        GameOverScreen screen = gameOverScreen.GetComponent<GameOverScreen>();
        screen?.Show();
    }

    public void ShowBigAutoModeButton()
    {
        autoModeButton.SetActive(false);
        bigAutoModeButton.SetActive(true);
    }

    public void ShowSmallAutoModeButton()
    {
        autoModeButton.SetActive(true);
        bigAutoModeButton.SetActive(false);
    }

    public void ShowPlayerUI()
    {
        healthBarObject.SetActive(true);
        shieldBarObject.SetActive(true);
        xpBarObject.SetActive(true);
        autoModeButton.SetActive(true);
        bigAutoModeButton.SetActive(false);
        smallSettingsButton.SetActive(true);
        bigSettingsButton.SetActive(false);
        if (playerController != null)
            playerController.SetActive(true);
        // waveBarObject.SetActive(true);
        // waveTextObject.SetActive(true);
    }

    public void HidePlayerUI()
    {
        healthBarObject.SetActive(false);
        shieldBarObject.SetActive(false);
        xpBarObject.SetActive(false);
        autoModeButton.SetActive(false);
        bigAutoModeButton.SetActive(false);
        smallSettingsButton.SetActive(false);
        bigSettingsButton.SetActive(true);
        if (playerController != null)
            playerController.SetActive(false);
        // waveBarObject.SetActive(false);
        // waveTextObject.SetActive(false);
    }

    public void ShowVillageDefenseGameOver()
    {
        Time.timeScale = 0;
        villageDefenseGameOverPanel.SetActive(true);
    }

    public void HideVillageDefenseGameOver()
    {
        Time.timeScale = 1;
        villageDefenseGameOverPanel.SetActive(false);
    }

    public void ShowVillageDefenseWinPanel()
    {
        Time.timeScale = 0;
        if (villageDefenseWinPanel != null)
            villageDefenseWinPanel.SetActive(true);
    }

    public void HideVillageDefenseWinPanel()
    {
        Time.timeScale = 1;
        villageDefenseWinPanel.SetActive(false);
    }

    public void ShowVillageAttackGameOver()
    {
        Time.timeScale = 0;
        villageAttackGameOverPanel.SetActive(true);
    }

    public void HideVillageAttackGameOver()
    {
        Time.timeScale = 1;
        villageAttackGameOverPanel.SetActive(false);
    }

    public void ShowVillageAttackVictory()
    {
        Time.timeScale = 0;
        villageAttackWinPanel.SetActive(true);
    }

    public void HideVillageAttackVictory()
    {
        Time.timeScale = 1;
        villageAttackWinPanel.SetActive(false);
    }

    public void ShowVillageAttackGameResumScreen()
    {
        Time.timeScale = 0;
        if (playerController != null)
            playerController.SetActive(true);
        villageAttackGameOverPanel.SetActive(false);
        villageAttackResumGameScreen.SetActive(true);
    }

    public void HideVillageAttackGameResumScreen()
    {
        Time.timeScale = 1;
        villageAttackResumGameScreen.SetActive(false);
    }

    public void ShowPlayerBaseGameOverScreen()
    {
        playerController.transform.SetSiblingIndex(gameUIGameObject.transform.childCount - 1);

        GameOverScreen screen = gameOverScreen.GetComponent<GameOverScreen>();
        screen?.ShowBackToMenuOnly();
        StaticDataManager.Pause(true);
    }

    public void ShowGameResumScreen()
    {
        if (environment == Environment.Village)
        {
            Loader.Load(Loader.Scene.Lobby);
            return;
        }
        // Amener l'écran de Game Over au premier plan
        if (playerController != null)
        {
            playerController.SetActive(true);
            playerController.transform.SetSiblingIndex(gameUIGameObject.transform.childCount - 1);
        }

        resumGameScreen.SetActive(true);
        StaticDataManager.Pause(true);
    }

    public void HideGameOverScreen()
    {
        // Remettre l'écran de Game Over en arrière-plan
        playerController.transform.SetSiblingIndex(0);

        // Désactiver l'écran de Game Over
        gameOverScreen.SetActive(false);
        StaticDataManager.Pause(false);
    }

    public void ShowVictoryScreen()
    {
        StaticDataManager.Pause(false);
        // Amener l'écran de Game Over au premier plan
        if (playerController != null)
            playerController.transform.SetSiblingIndex(gameUIGameObject.transform.childCount - 1);

        if (victoryScreen != null)
            victoryScreen.SetActive(true);
    }

    public void HideVictoryScreen()
    {
        // Désactiver l'écran de Victoire
        victoryScreen.SetActive(false);
        StaticDataManager.Pause(false);
    }

    public void ContinueGame()
    {
        HideGameOverScreen();
        if (deathScreenDuringVillage != null)
            deathScreenDuringVillage.SetActive(false);
        player.Respawn();
    }

    public void SetXpBar(float value, float maxValue)
    {
        if (xpBar == null)
            return;
        xpBar.maxBarValue = maxValue;
        xpBar.BarValue = value;
    }

    public void ShowAlertText(string text)
    {
        alertText.text = text;

        // Séquence d'animation
        Sequence waveMessageSequence = DOTween.Sequence();
        waveMessageSequence
            .Append(alertText.DOFade(1, fadeInalertTextDuration)) // Apparition (fade in)
            .AppendInterval(displayalertTextDuration) // Temps d'affichage
            .Append(alertText.DOFade(0, fadeOutalertTextDuration)) // Disparition (fade out)
            .Play();
    }

    public void SetIndicatorRenderer(Indicator indicator)
    {
        indicator.SetRenderer(indicatorRenderer);
    }

    public void ShowDeathScreenDuringVillage()
    {
        if (deathScreenDuringVillage != null)
            deathScreenDuringVillage.SetActive(true);
    }
}

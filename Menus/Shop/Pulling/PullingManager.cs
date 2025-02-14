using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PullingManager : MonoBehaviour
{
    public static PullingManager Instance;

    public Camera lobbyCamera;
    public GameObject lobbyCanvas;

    [Header("Camera Shakes Effect")]
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private float cameraSpeed = 2.0f; // Vitesse du mouvement de la caméra

    [SerializeField]
    private float shakeIntensity = 0.5f; // Intensité de la secousse pour simuler la vitesse

    [SerializeField]
    private float shakeDuration = 0.2f; // Durée de chaque secousse

    [Header("Popup Card Animation")]
    [SerializeField]
    private UIEffectsManager uiEffectsManager;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject laserPrefab; // Prendre un prefab de laser

    [SerializeField]
    private GameObject yellowFloatingPointPrefab; // Prendre un prefab de point jaune

    [SerializeField]
    private RawImageScroller scrollingBackground; // Prendre un prefab de point jaune

    [SerializeField]
    private Transform target;

    [SerializeField]
    private List<Transform> asteroids;

    [SerializeField]
    private float laserSpeed = 10f; // Vitesse du laser

    [Header("Checker variables")]
    [SerializeField]
    private bool hasShooted = false; // Vitesse du laser

    [SerializeField]
    private bool hasExplosed = false; // Vitesse du laser

    [SerializeField]
    private bool hasShownReward = false; // Vitesse du laser

    [SerializeField]
    private InputAction press;

    [Header("Reward elements")]
    [SerializeField]
    private Button rewardCloseButton;

    [SerializeField]
    private Button rewardEquipButton;

    [SerializeField]
    private Image rewardImage;

    [SerializeField]
    private TextMeshProUGUI rewardText;

    [SerializeField]
    private GameObject rewardObject;

    private GameObject yellowFloatingPointObject;
    private GameObject laser;
    public EquipementData[] currentEarnEquipement;
    private int currentEarnEquipementIndex = 0;

    [Header("Help texts")]
    [SerializeField] private GameObject shootHelpText;
    [SerializeField] private GameObject pickupHelpText;

    private Coroutine shootHelpTextCoroutine;
    private Coroutine pickupHelpTextCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rewardObject.SetActive(false);
        rewardCloseButton.onClick.AddListener(ClaimReward);
        rewardEquipButton.onClick.AddListener(EquipReward);
    }

    private void OnEnable()
    {
        press.Enable();
        press.performed += ctx => HandleClick();
    }

    private void OnDisable()
    {
        press.Disable();
        press.performed -= ctx => HandleClick();
    }

    private void Start()
    {
        // Commencer un mouvement de la caméra en boucle pour simuler l'accélération
        StartCoroutine(MoveCamera());
        shootHelpTextCoroutine = StartCoroutine(ShowShootHelpText());
    }

    private void HandleClick()
    {
        if (!hasExplosed)
        {
            StartShooting();
        }
        else if (!hasShownReward)
        {
            ShowReward();
        }
    }

    // Coroutine pour afiicher cliquer pour tirer au bout de 2 secondes avec un fade in avec doTween
    private IEnumerator ShowShootHelpText()
    {
        yield return new WaitForSeconds(2);
        if (!hasExplosed && shootHelpText != null)
        {
            shootHelpText.SetActive(true);
            shootHelpText.GetComponent<CanvasGroup>().DOFade(1, 0.5f).From(0);
        }
    }
    private IEnumerator ShowPickupHelpText()
    {
        yield return new WaitForSeconds(2);
        if (!hasShownReward && pickupHelpText != null)
        {
            pickupHelpText.SetActive(true);
            pickupHelpText.GetComponent<CanvasGroup>().DOFade(1, 0.5f).From(0);
        }
    }

    private void HideHelpText(GameObject helpText)
    {
        helpText.SetActive(false);
    }

    private IEnumerator MoveCamera()
    {
        while (!hasExplosed)
        {
            // Mouvement subtil de la caméra pour simuler un léger balancement
            mainCamera
                .transform.DOMoveY(
                    mainCamera.transform.position.y + Random.Range(-shakeIntensity, shakeIntensity),
                    shakeDuration
                )
                .SetEase(Ease.InOutSine);
            mainCamera
                .transform.DOMoveX(
                    mainCamera.transform.position.x + Random.Range(-shakeIntensity, shakeIntensity),
                    shakeDuration
                )
                .SetEase(Ease.InOutSine);

            // Vous pouvez aussi ajouter un zoom léger pour accentuer la vitesse
            mainCamera
                .DOFieldOfView(mainCamera.fieldOfView + 1.0f, cameraSpeed)
                .SetLoops(2, LoopType.Yoyo);

            foreach (var asteroid in asteroids)
            {
                asteroid
                    .transform.DOMoveY(
                        asteroid.transform.position.y
                            + Random.Range(-shakeIntensity / 2, shakeIntensity / 2),
                        shakeDuration
                    )
                    .SetEase(Ease.InOutSine);
                asteroid
                    .transform.DOMoveX(
                        asteroid.transform.position.x
                            + Random.Range(-shakeIntensity / 2, shakeIntensity / 2),
                        shakeDuration
                    )
                    .SetEase(Ease.InOutSine);
                asteroid
                    .transform.DOScale(
                        asteroid.transform.localScale.x
                            + Random.Range(-shakeIntensity / 5, shakeIntensity / 5),
                        shakeDuration
                    )
                    .SetEase(Ease.InOutSine);
            }

            yield return new WaitForSeconds(shakeDuration);
        }
    }

    private void StartShooting()
    {
        if (hasShooted)
            return;
        hasShooted = true;

        // Empeche son affichage et Cache le message d'aide si afficher
        if (shootHelpTextCoroutine != null)
            StopCoroutine(shootHelpTextCoroutine);
        HideHelpText(shootHelpText);

        // Prépare à afficher le message d'aide suivant
        pickupHelpTextCoroutine = StartCoroutine(ShowPickupHelpText());

        // Obtenir la position actuelle de la caméra en 2D (transformée en coordonnées du monde)
        Vector3 screenPosition = new(-10, -20, 0);

        // Instancier un laser à cette position
        laser = Instantiate(laserPrefab, screenPosition, Quaternion.identity);
        laser.transform.localScale *= 2;

        // Calculer la direction du tir vers le point (0,0)
        Vector2 direction = ((Vector2)target.position - (Vector2)screenPosition).normalized;

        // Appliquer un mouvement linéaire au laser
        Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.linearVelocity = direction * laserSpeed;
        }
    }

    private void ShowReward()
    {
        hasShownReward = true;
        rewardImage.sprite = currentEarnEquipement != null ? currentEarnEquipement[0].icon : null;
        rewardText.text =
            currentEarnEquipement != null ? currentEarnEquipement[0].displayName : null;
        rewardObject.SetActive(true);
        Destroy(yellowFloatingPointObject);
        // Empeche son affichage et Cache le message d'aide si afficher
        if (pickupHelpTextCoroutine != null)
            StopCoroutine(pickupHelpTextCoroutine);
        HideHelpText(pickupHelpText);
    }

    private void ClaimReward()
    {
        if (currentEarnEquipementIndex < currentEarnEquipement.Length - 1)
        {
            currentEarnEquipementIndex++;
            uiEffectsManager.Run("Move01");
            rewardImage.sprite = currentEarnEquipement[currentEarnEquipementIndex].icon;
            rewardText.text = currentEarnEquipement[currentEarnEquipementIndex].displayName;
        }
        else
        {
            CloseReward();
        }
    }

    private void CloseReward()
    {
        rewardObject.SetActive(false);
        currentEarnEquipement = null;
        lobbyCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(true);
        lobbyCanvas.SetActive(true);
        UnloadScenesIfLoaded();
    }

    private void UnloadScenesIfLoaded()
    {
        // Define the scene names as strings (or use ToString() if you prefer)
        string sceneName1 = Loader.Scene.Pulling.ToString();
        string sceneName2 = Loader.Scene.Pullingx10.ToString();

        // Check if the first scene is loaded before attempting to unload it
        Scene scene1 = SceneManager.GetSceneByName(sceneName1);
        if (scene1.isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName1);
        }

        // Check if the second scene is loaded before attempting to unload it
        Scene scene2 = SceneManager.GetSceneByName(sceneName2);
        if (scene2.isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName2);
        }
    }

    private void EquipReward()
    {
        RessourcesManager.Instance.SetEquipedEquipments(
            currentEarnEquipement[currentEarnEquipementIndex].slot,
            currentEarnEquipement[currentEarnEquipementIndex]
        );
        ClaimReward();
    }

    private void Update()
    {
        if (hasShooted && !hasExplosed && laser == null)
        {
            hasExplosed = true;
            EffectManager.Instance.Explosion(Vector2.zero, 50, 2);

            scrollingBackground.canScroll = false;
            yellowFloatingPointObject = Instantiate(
                yellowFloatingPointPrefab,
                new Vector3(0, 0, 0),
                Quaternion.identity
            );
            Destroy(target.gameObject);
        }
    }
}

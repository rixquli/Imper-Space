using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovingManager : MonoBehaviour
{
    public static MovingManager Instance;

    [SerializeField]
    private GameObject showGadgetsListPanelButton;

    [SerializeField]
    private GameObject gadgetsListPanelObject;

    [SerializeField]
    private GameObject gadgetsListPrefab;

    [SerializeField]
    private GameObject selectPanelObject;

    [SerializeField]
    private RectTransform moveButton;

    [SerializeField]
    private RectTransform pickupButton;

    [SerializeField]
    private RectTransform upgradeButton;

    [SerializeField]
    private RectTransform rotateButton;

    [SerializeField]
    private RectTransform upgradeConfirmPanel;

    [SerializeField]
    private Button upgradeConfirmCloseButton;

    [SerializeField]
    private Button upgradeConfirmConfirmUpgrade;

    [SerializeField]
    private Button upgradeConfirmWatchAdUpgrade;

    [SerializeField]
    private TextMeshProUGUI upgradeConfirmLevelText;

    [SerializeField]
    private TextMeshProUGUI upgradeConfirmPlayerAlreadyHaveAmoutText;

    [SerializeField]
    private TextMeshProUGUI upgradeConfirmCostAmoutText;

    [SerializeField]
    private GameObject draggingPanelObject;

    [SerializeField]
    private RectTransform confirmButton;

    [SerializeField]
    private RectTransform cancelButton;

    [SerializeField]
    private InputAction press,
        screenPos;
    private Vector3 originalPosition;

    private Vector3 curScreenPos;

    Camera _camera;
    private bool isDragging;

    public Transform selectedObject = null;

    private Camera mainCamera;

    // private Vector3 originalPosition;
    public bool canBeDragged = true;
    public bool isMoving = false;

    private Vector3 WorldPos
    {
        get
        {
            float z = _camera.WorldToScreenPoint(transform.position).z;
            return _camera.ScreenToWorldPoint(curScreenPos + new Vector3(0, 0, z));
        }
    }
    public bool isClickedOn
    {
        get
        {
            Vector2 rayOrigin = _camera.ScreenToWorldPoint(curScreenPos);
            int layerMask = 1 << LayerMask.NameToLayer("Damageable");
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 1000f, layerMask);

            if (hit.collider != null)
            {
                IDraggable draggable = hit.transform.GetComponent<IDraggable>();
                if (draggable != null)
                {
                    if (!draggable.DisableDragObject && draggable.canBeDragged)
                    {
                        if (selectedObject == null && hit.transform != selectedObject)
                            Select(hit.transform);
                        return true;
                    }
                }
            }
            return false;
        }
    }

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

        selectPanelObject.SetActive(false);
        upgradeConfirmPanel.gameObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        gadgetsListPanelObject.SetActive(false);
        mainCamera = Camera.main; // Cache the main camera

        _camera = Camera.main;
        screenPos.Enable();
        press.Enable();
        screenPos.performed += context =>
        {
            curScreenPos = context.ReadValue<Vector2>();
        };
        press.performed += _ =>
        {
            if (UtilsClass.IsPointerOverUI())
                return;
            // if (!canBeDragged) return;
            bool currentIsClickOn = isClickedOn;
            if (currentIsClickOn && !isMoving && !selectPanelObject.activeSelf)
            {
                selectPanelObject.SetActive(true);
            }
            if (isMoving)
            {
                StartCoroutine(Drag());
            }
            // Debug.Log(currentIsClickOn);
            // Debug.Log(isMoving);
            if (!currentIsClickOn && !isMoving)
            {
                StartCoroutine(WaitBeforeCancelling());
            }
        };
        press.canceled += _ =>
        {
            isDragging = false;

            StopCoroutine(Drag());
        };
    }

    private void OnDestroy()
    {
        screenPos.Disable();
        press.Disable();
    }

    private IEnumerator WaitBeforeCancelling()
    {
        yield return new WaitForSeconds(0.1f);
        if (!isMoving)
        {
            selectedObject = null;
        }
        selectPanelObject.SetActive(false);
    }

    private IEnumerator Drag()
    {
        UtilsClass.DesactivateCollision(
            selectedObject.gameObject,
            new string[] { "Player", "Enemy" }
        );

        isDragging = true;
        // Debug.LogWarning(Vector2.Distance((Vector2)WorldPos, (Vector2)transform.position));
        if (
            !isClickedOn
            && Vector2.Distance((Vector2)WorldPos, (Vector2)selectedObject.position) > 10
        )
            selectedObject.position = WorldPos;
        Vector3 offset = selectedObject.position - WorldPos;
        // grab
        // GetComponent<Rigidbody2D>().useGravity = false;
        while (isDragging)
        {
            // dragging
            // selectedObject.position = WorldPos + offset;
            Vector3 position = WorldPos + offset;
            position.x = Mathf.Round(position.x / 5f) * 5f;
            position.y = Mathf.Round(position.y / 5f) * 5f;
            position.z = originalPosition.z;
            selectedObject.position = position;
            yield return null;
        }
        // drop
        // GetComponent<Rigidbody2D>().useGravity = true;
    }

    private void Start()
    {
        moveButton.GetComponent<Button>().onClick.AddListener(MoveButtonClicked);
        pickupButton.GetComponent<Button>().onClick.AddListener(PickupButtonClicked);
        upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButtonButtonClicked);
        rotateButton.GetComponent<Button>().onClick.AddListener(RotateButtonButtonClicked);
        confirmButton.GetComponent<Button>().onClick.AddListener(ConfirmButtonClicked);
        cancelButton.GetComponent<Button>().onClick.AddListener(CancelButtonClicked);
        upgradeConfirmCloseButton.onClick.AddListener(HideUpgradeConfirmPanel);
        showGadgetsListPanelButton
            .GetComponentInChildren<Button>()
            .onClick.AddListener(OnGadgetsListPanelClick);
    }

    private void MoveButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(true);
        isMoving = true;

        IDraggable draggable = selectedObject?.GetComponent<IDraggable>();
        if (draggable != null)
        {
            draggable.OnDrag();

            UtilsClass.DesactivateCollision(
                selectedObject.gameObject,
                new string[] { "Player", "Enemy" }
            );
        }
    }

    private void PickupButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;

        //TODO: ajouter gadgets à l'inventaire
        IGadgetable gadgetable = selectedObject?.GetComponent<IGadgetable>();
        if (gadgetable != null)
        {
            GadgetsData gadgetsData = ItemDatabase.Instance.GetGadgetById(
                gadgetable.GadgetsData.gadgetId
            );
            Inventory.Instance.AddGadgets(gadgetsData, gadgetable.Level);
        }

        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    private void RotateButtonButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;

        RotateObject();
    }

    private void UpgradeButtonButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;

        //TODO: afficher la popUp pour l'ameliorer
        ShowUpgradeConfirmPanel();

        // selectedObject = null;
    }

    private void RotateObject()
    {
        // Obtenir la rotation actuelle de l'objet sous forme d'Euler angles
        float angle = selectedObject.rotation.eulerAngles.z + 90;

        // Normalise l'angle pour rester entre 0° et 360°
        angle = angle % 360;
        if (angle < 0)
        {
            angle += 360; // Si l'angle est négatif, on l'ajuste
        }

        // Appliquer la nouvelle rotation à l'objet
        selectedObject.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void HideUpgradeConfirmPanel()
    {
        Time.timeScale = 1;
        upgradeConfirmPanel.gameObject.SetActive(false);
        upgradeConfirmConfirmUpgrade.onClick.RemoveAllListeners();
        upgradeConfirmWatchAdUpgrade.onClick.RemoveAllListeners();
    }

    private void ShowUpgradeConfirmPanel()
    {
        IDraggable draggable = selectedObject.GetComponent<IDraggable>();
        if (draggable != null)
        {
            if (!draggable.GadgetsData.upgradeStateAndPrice.ContainsKey(draggable.Level + 1))
                return;

            Time.timeScale = 0;
            upgradeConfirmPanel.gameObject.SetActive(true);

            upgradeConfirmLevelText.text = $"Lvl.{draggable.Level} -> {draggable.Level + 1}";
            upgradeConfirmPlayerAlreadyHaveAmoutText.text =
                $"{RessourcesManager.Instance.goldAmount} GOLD";
            upgradeConfirmCostAmoutText.text =
                $"{RessourcesManager.Instance.goldAmount - draggable.GadgetsData.upgradeStateAndPrice[draggable.Level + 1]} GOLD";

            upgradeConfirmConfirmUpgrade.onClick.AddListener(() =>
            {
                HideUpgradeConfirmPanel();
                UpgradeGadget(draggable);
            });
            upgradeConfirmWatchAdUpgrade.onClick.AddListener(() =>
            {
                AdmobManager.Instance.ShowRewardedAd(
                    (rewardAd) =>
                    {
                        HideUpgradeConfirmPanel();
                        UpgradeGadgetWithAd(draggable);
                    }
                );
            });
        }
    }

    private void UpgradeGadget(IDraggable draggable)
    {
        if (!draggable.GadgetsData.upgradeStateAndPrice.ContainsKey(draggable.Level + 1))
            return;

        RessourcesManager.Instance.Buy(
            PaymantSubtype.Gold,
            draggable.GadgetsData.upgradeStateAndPrice[draggable.Level + 1],
            () =>
            {
                draggable.Upgrade();
            },
            () =>
            {
                Debug.LogWarning("Error during upgrade");
            }
        );
    }

    private void UpgradeGadgetWithAd(IDraggable draggable)
    {
        if (!draggable.GadgetsData.upgradeStateAndPrice.ContainsKey(draggable.Level + 1))
            return;

        draggable.Upgrade();
    }

    private void ConfirmButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;
        UtilsClass.ActivateCollision(selectedObject.gameObject, new string[] { "Player", "Enemy" });

        IDraggable draggable = selectedObject.GetComponent<IDraggable>();
        if (draggable != null)
        {
            draggable.OnDragEnd(); // Perform necessary setup
        }

        selectedObject = null;
    }

    private void CancelButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;
        UtilsClass.ActivateCollision(selectedObject.gameObject, new string[] { "Player", "Enemy" });

        IDraggable draggable = selectedObject.GetComponent<IDraggable>();
        if (draggable != null)
        {
            draggable.OnDragEnd(); // Perform necessary setup
        }
        selectedObject.position = originalPosition;
        selectedObject = null;
    }

    public void Select(Transform transformObject)
    {
        draggingPanelObject.SetActive(false);
        selectPanelObject.SetActive(true);

        selectedObject = transformObject;
        originalPosition = transformObject.position;
    }

    private void Update()
    {
        if (selectedObject != null)
        {
            // Calculer la position dans le monde de l'objet sélectionné
            Vector3 objectWorldPosition = selectedObject.position;

            // Convertir la position du monde en position écran
            Vector2 screenPoint = mainCamera.WorldToScreenPoint(objectWorldPosition);

            // Vérifier si l'objet est en mouvement
            if (isMoving)
            {
                SetButtonPositions(
                    screenPoint,
                    new RectTransform[] { confirmButton, cancelButton }
                );
            }
            else
            {
                // Liste dynamique pour les boutons à afficher
                List<RectTransform> buttonsToShow = new List<RectTransform>
                {
                    moveButton,
                    pickupButton,
                };

                // Récupérer l'interface IDraggable
                IDraggable draggable = selectedObject.GetComponent<IDraggable>();

                // Gérer l'affichage du bouton d'amélioration
                if (
                    draggable != null
                    && draggable.GadgetsData.upgradeStateAndPrice.ContainsKey(draggable.Level + 1)
                )
                {
                    upgradeButton.gameObject.SetActive(true);
                    buttonsToShow.Add(upgradeButton); // Ajout du bouton d'amélioration
                }
                else
                {
                    upgradeButton.gameObject.SetActive(false);
                }

                // Gérer l'affichage du bouton de rotation
                if (draggable != null && draggable.canBeRotated)
                {
                    rotateButton.gameObject.SetActive(true);
                    buttonsToShow.Add(rotateButton); // Ajout du bouton de rotation
                }
                else
                {
                    rotateButton.gameObject.SetActive(false);
                }

                // Mettre à jour la position des boutons à afficher
                SetButtonPositions(screenPoint, buttonsToShow.ToArray());
            }
        }
    }

    private void SetButtonPositions(Vector2 screenPoint, RectTransform[] buttons = null)
    {
        if (buttons == null || buttons.Length == 0)
        {
            return;
        }

        float spacing = Screen.width / 1.75f / 10; // Adjust this value as needed for spacing

        float totalWidth = (buttons.Length - 1) * spacing;

        for (int i = 0; i < buttons.Length; i++)
        {
            Vector2 buttonPoint = new Vector2(
                screenPoint.x - totalWidth / 2 + i * spacing,
                screenPoint.y + buttons[i].rect.height / 2
            );

            buttons[i].position = buttonPoint;
        }
    }

    public void ShowGadgetsList()
    {
        gadgetsListPanelObject.SetActive(true);
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
    }

    public void HideGadgetsList()
    {
        gadgetsListPanelObject.SetActive(false);
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
    }

    private void InstantiateAndSetup(GameObject prefab, Vector3 position, int level)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.SetActive(false); // Disable the instance to prevent visual artifacts

        Transform instanceTransform = instance.transform;
        IDraggable draggable = instance.GetComponent<IDraggable>();
        if (draggable != null)
        {
            draggable.Level = level;
            draggable.SpawnAsPreview(); // Perform necessary setup
        }

        instance.SetActive(true); // Enable the instance after setup
        selectedObject = instanceTransform;
        Select(selectedObject);
        MoveButtonClicked();
    }

    private void GadgetsListPanelClearContent()
    {
        // Find the Content transform
        Transform contentTransform = gadgetsListPanelObject.transform.Find(
            "Gadgets/Viewport/Content"
        );
        if (contentTransform == null)
        {
            Debug.LogError("Content transform not found. Check the hierarchy path.");
            return;
        }

        // Iterate through all children of the Content transform and destroy them
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("All children of the Content transform have been cleared.");
    }

    private void OnGadgetsListPanelClick()
    {
        List<GadgetsStoreValue> gadgets = Inventory.Instance.gadgets;

        GadgetsListPanelClearContent();

        foreach (var gadgetValue in gadgets)
        {
            GameObject item = Instantiate(
                gadgetsListPrefab,
                gadgetsListPanelObject.transform.Find("Gadgets/Viewport/Content")
            );

            // Get the TextMeshPro component and set the text
            TextMeshProUGUI textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"{gadgetValue.data.displayName} lvl.{gadgetValue.level}";
            }
            else
            {
                Debug.LogError(
                    "TextMeshProUGUI component not found in the instantiated gadget item."
                );
            }

            Image image = item.transform.Find("Button/Image").GetComponent<Image>();
            if (image != null)
            {
                image.sprite = gadgetValue.data.visual;
            }
            else
            {
                Debug.LogError("Image component not found in the instantiated gadget item.");
            }

            Button btn = item.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    SpawnGadget(gadgetValue.data.gadgetId, gadgetValue.level);
                    Inventory.Instance.RemoveGadgets(gadgetValue.data.gadgetId, gadgetValue.level);
                });
            }
            else
            {
                Debug.LogError("Button component not found in the instantiated gadget item.");
            }

            item.SetActive(true);
        }
    }

    public void SpawnGadget(int gadgetId, int level)
    {
        GameObject prefab = ItemDatabase.Instance.GetGadgetById(gadgetId).prefab;
        if (prefab)
        {
            gadgetsListPanelObject.SetActive(false);
            selectPanelObject.SetActive(false);
            draggingPanelObject.SetActive(false);

            isMoving = false;

            // The center of the camera's viewport is at (0.5, 0.5)
            Vector3 centerViewportPosition = new Vector3(0.5f, 0.5f, _camera.nearClipPlane);
            // Convert the viewport position to world position
            Vector3 position = _camera.ViewportToWorldPoint(centerViewportPosition);

            position.z = prefab.transform.position.z;
            Debug.Log(position);
            InstantiateAndSetup(prefab, position, level);
        }
    }
}

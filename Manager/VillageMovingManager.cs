using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VillageMovingManager : MonoBehaviour
{
    [System.Serializable]
    public struct Rectangle
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomRight;
        public Vector2 bottomLeft;
    }

    public static VillageMovingManager Instance;

    [SerializeField] private GameObject showGadgetsListPanelButton;

    [SerializeField] private GameObject gadgetsListPanelObject;

    [SerializeField] private GameObject gadgetsListPrefab;

    [SerializeField] private GameObject selectPanelObject;

    [SerializeField] private RectTransform moveButton;

    [SerializeField] private RectTransform pickupButton;

    [SerializeField] private RectTransform upgradeButton;

    [SerializeField] private RectTransform rotateButtonWhenMove;

    [SerializeField] private RectTransform rotateButton;

    [SerializeField] private RectTransform recruitButton;

    [SerializeField] private RecruitPanelController recruitPanel;

    [SerializeField] private RectTransform upgradeConfirmPanel;

    [SerializeField] private Button upgradeConfirmCloseButton;

    [SerializeField] private Button upgradeConfirmConfirmUpgrade;

    [SerializeField] private Button upgradeConfirmWatchAdUpgrade;

    [SerializeField] private TextMeshProUGUI upgradeConfirmLevelText;

    [SerializeField] private TextMeshProUGUI upgradeConfirmCostAmoutText;

    [SerializeField] private GameObject draggingPanelObject;

    [SerializeField] private RectTransform confirmButton;

    [SerializeField] private RectTransform cancelButton;

    [Header("Alert Messages")]
    [SerializeField] private GameObject cantUpgradeBaseAlert;

    [SerializeField] private Button cantUpgradeBaseAlertCloseButton;

    [SerializeField] private GameObject cantUpgradeBuildingAlert;

    [SerializeField] private Button cantUpgradeBuildingAlertCloseButton;
    [SerializeField] private GameObject cantBuyBuildingAlert;
    [SerializeField] private Button cantBuyBuildingAlertCloseButton;

    [SerializeField] private VillageCameraController villageCameraController;

    [SerializeField]
    private InputAction press,
        screenPos;
    private Vector3 originalPosition;
    private float originalRotation;
    private Vector3 previousPosition;

    private Vector3 curScreenPos;

    Camera _camera;
    private bool isDragging;
    public Collider2D selectedObstacleCollider = null;

    public Transform _selectedObject = null;
    public Transform selectedObject
    {
        get { return _selectedObject; }
        set
        {
            _selectedObject = value;
            if (value != null)
                selectedObstacleCollider = UtilsClass.FindChildCollidersOnLayer(
                    value.gameObject,
                    new string[] { "Player", "Enemy" }
                );
        }
    }

    private bool hasSpawnFromInventory = false;
    private bool onNextConfirmPlaceRemoveFromInventory = false;

    private Camera mainCamera;

    // private Vector3 originalPosition;
    public bool canBeDragged = true;

    private bool _isMoving = false; // Backing field
    public bool isMoving
    {
        get { return _isMoving; }
        set
        {
            _isMoving = value;
            if (villageCameraController != null)
                villageCameraController.blockMoving = value;
        }
    }

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
                        if (!isMoving && hit.transform != selectedObject)
                            Select(hit.transform);
                        return true;
                    }
                }
            }
            return false;
        }
    }

    [Header("For area selection")]
    public List<Rectangle> rectangles; // Liste de rectangles, chaque rectangle avec 4 coins
    private Mesh mesh;
    private int oldRectanglesCount = 0;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshFilter borderMeshFilter;
    private Tween anim;

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
        if (upgradeConfirmPanel != null)
            upgradeConfirmPanel?.gameObject?.SetActive(false);
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

        if (cantUpgradeBaseAlert != null)
        {
            cantUpgradeBaseAlert.SetActive(false);
            cantUpgradeBaseAlertCloseButton.onClick.AddListener(
                () => cantUpgradeBaseAlert.SetActive(false)
            );
        }
        if (cantUpgradeBuildingAlert != null)
        {
            cantUpgradeBuildingAlert.SetActive(false);
            cantUpgradeBuildingAlertCloseButton.onClick.AddListener(
                () => cantUpgradeBuildingAlert.SetActive(false)
            );
        }
        if (cantBuyBuildingAlert != null)
        {
            cantBuyBuildingAlert.SetActive(false);
            cantBuyBuildingAlertCloseButton.onClick.AddListener(
                () => cantBuyBuildingAlert.SetActive(false)
            );
        }
    }

    private void OnDestroy()
    {
        screenPos.Disable();
        press.Disable();
    }

    public void ShowCantBuyBuildingAlert()
    {
        cantBuyBuildingAlert.SetActive(true);
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

    private bool CanBePlaced(Vector3 position, string[] layerNames)
    {
        foreach (var layerName in layerNames)
        {
            if (!CanBePlaced(position, layerName))
            {
                return false;
            }
        }
        return true;
    }

    private bool CanBePlaced(Vector3 position, string layerName)
    {
        // On prend la position et les dimensions du collider de l'obstacle sélectionné
        Bounds bounds = selectedObstacleCollider.bounds;

        // Calculer les extents de la boîte (moitié des dimensions de la boîte englobante)
        Vector2 halfExtents = bounds.extents;

        // Ajuster le centre des bounds pour la nouvelle position (en 2D, on ignore l'axe Z)
        Vector2 newCenter =
            (Vector2)position
            + (Vector2)(bounds.center - selectedObstacleCollider.transform.position);

        // Utilisation de Physics2D.OverlapBoxAll pour vérifier les collisions autour du collider à la nouvelle position
        Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(
            newCenter,
            halfExtents * 2,
            selectedObstacleCollider.transform.rotation.eulerAngles.z,
            1 << LayerMask.NameToLayer(layerName)
        );

        // Exclure l'objet actuellement sélectionné des collisions détectées
        foreach (Collider2D col in overlappingColliders)
        {
            if (col.gameObject != selectedObstacleCollider.gameObject) // On ignore l'objet sélectionné
            {
                return false; // Une collision avec un autre objet est détectée
            }
        }

        return true; // Aucun autre objet en collision, la position est valide
    }

    private IEnumerator Drag()
    {
        UtilsClass.DesactivateCollision(
            selectedObject.gameObject,
            new string[] { "Player", "Enemy" }
        );

        isDragging = true;
        if (
            !isClickedOn
            && Vector2.Distance((Vector2)WorldPos, (Vector2)selectedObject.position) > 5
        )
        {
            if (CanBePlaced(WorldPos, new string[] { "Player", "Enemy" }))
            {
                selectedObject.position = WorldPos;
            }
        }

        Vector3 offset = selectedObject.position - WorldPos;

        while (isDragging)
        {
            Vector3 position = WorldPos + offset;

            // Arrondir la position à la grille de 1 unités
            position.x = Mathf.Round(position.x / 0.25f) * 0.25f;
            position.y = Mathf.Round(position.y / 0.25f) * 0.25f;
            position.z = originalPosition.z;

            // Si la nouvelle position n'est pas identique à l'ancienne
            if (position != selectedObject.position)
            {
                // Vérifier si l'objet peut être placé à la nouvelle position
                if (CanBePlaced(position, new string[] { "Player", "Enemy" }))
                {
                    selectedObject.position = position;
                }
            }

            yield return null;
        }
    }

    private void Start()
    {
        moveButton.GetComponent<Button>().onClick.AddListener(MoveButtonClicked);
        pickupButton.GetComponent<Button>().onClick.AddListener(PickupButtonClicked);
        if (rotateButtonWhenMove != null)
            rotateButtonWhenMove.GetComponent<Button>().onClick.AddListener(RotateButtonButtonClicked);
        upgradeButton.GetComponent<Button>().onClick.AddListener(UpgradeButtonButtonClicked);
        rotateButton.GetComponent<Button>().onClick.AddListener(RotateButtonButtonClicked);
        recruitButton.GetComponent<Button>().onClick.AddListener(RecruitButtonButtonClicked);
        confirmButton.GetComponent<Button>().onClick.AddListener(ConfirmButtonClicked);
        cancelButton.GetComponent<Button>().onClick.AddListener(CancelButtonClicked);
        if (upgradeConfirmPanel != null)
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

        DeleteGadgetDataInBuildingList(selectedObject.GetComponent<IDraggable>());

        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    private void RotateButtonButtonClicked()
    {
        RotateObject();
    }

    private void RecruitButtonButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;

        recruitPanel.Show();
    }

    private void UpgradeButtonButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;

        // Verifie si l'objet est un batiment et peut etre ameliorer en ragardant si il a un IDraggable et son gadget data
        // et si la base dans VillageRessourcesManager a le bon niveau pour l'ameliorer
        if (selectedObject.TryGetComponent<IDraggable>(out IDraggable draggable))
        {
            if (draggable.GadgetsData != null)
            {
                if (draggable.GadgetsData.gadgetId == 5) // Base
                {
                    if (!VillageRessourcesManager.Instance.CanUpgradeBase())
                    {
                        cantUpgradeBaseAlert.SetActive(true);
                        return;
                    }
                }
                else
                {
                    if (!VillageRessourcesManager.Instance.CanUpgradeBuilding(draggable.Level + 1))
                    {
                        cantUpgradeBuildingAlert.SetActive(true);
                        return;
                    }
                }
            }
        }

        //TODO: afficher la popUp pour l'ameliorer
        ShowUpgradeConfirmPanel();

        // selectedObject = null;
    }

    private void RotateObject()
    {
        // Obtenir la rotation actuelle de l'objet sous forme d'Euler angles
        float angle = selectedObject.rotation.eulerAngles.z - 45;

        // Normalise l'angle pour rester entre 0° et 360°
        angle = angle % 360;
        if (angle < 0)
        {
            angle += 360; // Si l'angle est négatif, on l'ajuste
        }

        // Appliquer la nouvelle rotation à l'objet
        selectedObject.rotation = Quaternion.Euler(0, 0, angle);

        UpdateBuildingList(selectedObject.GetComponent<IDraggable>());
    }

    private void HideUpgradeConfirmPanel()
    {
        Time.timeScale = 1;
        upgradeConfirmPanel.gameObject.SetActive(false);
        upgradeConfirmConfirmUpgrade.onClick.RemoveAllListeners();
        // upgradeConfirmWatchAdUpgrade.onClick.RemoveAllListeners();
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

            upgradeConfirmLevelText.text = $"Upgrade to\nLvl.{draggable.Level + 1}";
            upgradeConfirmCostAmoutText.text =
                $"{draggable.GadgetsData.upgradeStateAndPrice[draggable.Level + 1]} GOLD";

            upgradeConfirmConfirmUpgrade.onClick.AddListener(() =>
            {
                HideUpgradeConfirmPanel();
                UpgradeGadget(draggable);
            });
            // upgradeConfirmWatchAdUpgrade.onClick.AddListener(() =>
            // {
            //     AdmobManager.Instance.ShowRewardedAd(
            //         (rewardAd) =>
            //         {
            //             HideUpgradeConfirmPanel();
            //             UpgradeGadgetWithAd(draggable);
            //         }
            //     );
            // });
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
                // Assuming 'buildingList' is an array, use Array.Find
                var buildingListEntry = Array.Find(
                    VillageRessourcesManager.Instance.buildingList,
                    entry => entry.gadgetsData == draggable.GadgetsData
                );

                // Check if the BuildingList entry was found
                if (buildingListEntry != null)
                {
                    // Create a new Buildings object and add the selected GameObject to it
                    BuildingList.Buildings newBuilding;
                    if (
                        buildingListEntry.buildings.Exists(e =>
                            e.position == (Vector2)originalPosition && e.level == draggable.Level
                        )
                    )
                    {
                        newBuilding = buildingListEntry.buildings.Find(e =>
                            e.position == (Vector2)originalPosition && e.level == draggable.Level
                        );
                        newBuilding.position = selectedObject.position;
                        newBuilding.rotationZ = selectedObject.rotation.eulerAngles.z;
                        newBuilding.level = draggable.Level + 1;
                    }
                    else
                    {
                        Debug.LogError("No BuildingList.Buildings");
                        newBuilding = new BuildingList.Buildings
                        {
                            position = selectedObject.position, // Assign the correct position here
                            rotationZ = selectedObject.rotation.eulerAngles.z,
                            level = draggable.Level + 1,
                        };

                        // Add the new building to the buildings list
                        buildingListEntry.buildings.Add(newBuilding);
                    }
                }
                else
                {
                    // Handle case where no matching entry is found
                    Debug.LogError(
                        "No matching BuildingList entry found for the provided GadgetsData."
                    );
                }
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
        // Assuming 'buildingList' is an array, use Array.Find
        var buildingListEntry = Array.Find(
            VillageRessourcesManager.Instance.buildingList,
            entry => entry.gadgetsData == draggable.GadgetsData
        );

        // Check if the BuildingList entry was found
        if (buildingListEntry != null)
        {
            // Create a new Buildings object and add the selected GameObject to it
            BuildingList.Buildings newBuilding;
            if (
                buildingListEntry.buildings.Exists(e =>
                    e.position == (Vector2)originalPosition && e.level == draggable.Level
                )
            )
            {
                newBuilding = buildingListEntry.buildings.Find(e =>
                    e.position == (Vector2)originalPosition && e.level == draggable.Level
                );
                newBuilding.position = selectedObject.position;
                newBuilding.rotationZ = selectedObject.rotation.eulerAngles.z;
                newBuilding.level = draggable.Level + 1;
            }
            else
            {
                Debug.LogError("No BuildingList.Buildings");
                newBuilding = new BuildingList.Buildings
                {
                    position = selectedObject.position, // Assign the correct position here
                    rotationZ = selectedObject.rotation.eulerAngles.z,
                    level = draggable.Level + 1,
                };

                // Add the new building to the buildings list
                buildingListEntry.buildings.Add(newBuilding);
            }
        }
        else
        {
            // Handle case where no matching entry is found
            Debug.LogError("No matching BuildingList entry found for the provided GadgetsData.");
        }
        draggable.Upgrade();
    }

    private void DeleteGadgetDataInBuildingList(IDraggable draggable)
    {
        // Assuming 'buildingList' is an array, use Array.Find
        var buildingListEntry = Array.Find(
            VillageRessourcesManager.Instance.buildingList,
            entry => entry.gadgetsData == draggable.GadgetsData
        );

        // Check if the BuildingList entry was found
        if (buildingListEntry != null)
        {
            // Create a new Buildings object and add the selected GameObject to it
            BuildingList.Buildings newBuilding;
            if (
                buildingListEntry.buildings.Exists(e =>
                    e.position == (Vector2)originalPosition && e.level == draggable.Level
                )
            )
            {
                newBuilding = buildingListEntry.buildings.Find(e =>
                    e.position == (Vector2)originalPosition && e.level == draggable.Level
                );
                buildingListEntry.buildings.Remove(newBuilding);
            }
        }
        else
        {
            // Handle case where no matching entry is found
            Debug.LogError("No matching BuildingList entry found for the provided GadgetsData.");
        }
    }

    private void UpdateBuildingList(IDraggable draggable, bool canCreatIfNotExist = false)
    {
        // Assuming 'buildingList' is an array, use Array.Find
        var buildingListEntry = Array.Find(
            VillageRessourcesManager.Instance.buildingList,
            entry => entry.gadgetsData == draggable.GadgetsData
        );

        // Check if the BuildingList entry was found
        if (buildingListEntry != null)
        {
            // Create a new Buildings object and add the selected GameObject to it
            BuildingList.Buildings newBuilding;
            if (
                buildingListEntry.buildings.Exists(e =>
                    e.position == (Vector2)originalPosition && e.level == draggable.Level
                )
            )
            {
                newBuilding = buildingListEntry.buildings.Find(e =>
                    e.position == (Vector2)originalPosition && e.level == draggable.Level
                );
                newBuilding.position = selectedObject.position;
                newBuilding.rotationZ = selectedObject.rotation.eulerAngles.z;
                newBuilding.level = draggable.Level;
            }
            else if (canCreatIfNotExist)
            {
                Debug.LogError("No BuildingList.Buildings");
                newBuilding = new BuildingList.Buildings
                {
                    position = selectedObject.position, // Assign the correct position here
                    rotationZ = selectedObject.rotation.eulerAngles.z,
                    level = draggable.Level,
                };

                // Add the new building to the buildings list
                buildingListEntry.buildings.Add(newBuilding);
            }
        }
        else
        {
            // Handle case where no matching entry is found
            Debug.LogError("No matching BuildingList entry found for the provided GadgetsData.");
        }
    }

    private void ConfirmButtonClicked()
    {
        if (!CanBePlaced(selectedObject.position, new string[] { "Player", "Enemy" }))
            return;
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;
        UtilsClass.ActivateCollision(selectedObject.gameObject, new string[] { "Player", "Enemy" });

        IDraggable draggable = selectedObject.GetComponent<IDraggable>();
        if (draggable != null)
        {
            draggable.OnDragEnd(); // Perform necessary setup
        }

        // si le batiment vient d'etre acheter alors l'enlever de l'inventaire au moment de le poser
        if (onNextConfirmPlaceRemoveFromInventory)
        {
            onNextConfirmPlaceRemoveFromInventory = false;
            Inventory.Instance.RemoveGadgets(draggable.GadgetsData.gadgetId, draggable.Level);
        }
        if (hasSpawnFromInventory)
        {
            hasSpawnFromInventory = false;
            Inventory.Instance.RemoveGadgets(draggable.GadgetsData.gadgetId, draggable.Level);
        }

        UpdateBuildingList(draggable, true);

        selectedObject = null;
    }

    private void CancelButtonClicked()
    {
        selectPanelObject.SetActive(false);
        draggingPanelObject.SetActive(false);
        isMoving = false;

        // si le batiment vient d'etre acheter alors l'enlever de l'inventaire au moment de le poser
        if (onNextConfirmPlaceRemoveFromInventory)
        {
            onNextConfirmPlaceRemoveFromInventory = false;
            Destroy(selectedObject.gameObject);
        }
        else if (hasSpawnFromInventory)
        {
            hasSpawnFromInventory = false;
            Destroy(selectedObject.gameObject);
        }
        else
        {
            UtilsClass.ActivateCollision(
                selectedObject.gameObject,
                new string[] { "Player", "Enemy" }
            );

            IDraggable draggable = selectedObject.GetComponent<IDraggable>();
            if (draggable != null)
            {
                draggable.OnDragEnd(); // Perform necessary setup
            }
            selectedObject.position = originalPosition;
            selectedObject.rotation = Quaternion.Euler(0, 0, originalRotation);
        }

        selectedObject = null;
    }

    public void Select(Transform transformObject)
    {
        draggingPanelObject.SetActive(false);
        selectPanelObject.SetActive(true);

        selectedObject = transformObject;
        originalPosition = transformObject.position;
        originalRotation = transformObject.rotation.z;

        if (anim != null)
            anim.Kill();
        anim = transformObject.DOScale(transformObject.localScale, 0.5f)
                .From(transformObject.localScale * 0.9f)
                .SetEase(Ease.OutBack);
    }

    private void Update()
    {
        if (oldRectanglesCount != rectangles.Count)
        {
            oldRectanglesCount = rectangles.Count;
            GenerateCombinedMesh();
        }
        if (selectedObject != null)
        {
            // Vérifier si l'objet est en mouvement
            if (isMoving)
            {
                // Liste dynamique pour les boutons à afficher
                List<RectTransform> buttonsToShow = new List<RectTransform>
                {
                    confirmButton,
                    cancelButton,
                };

                // Récupérer l'interface IDraggable
                IDraggable draggable = selectedObject.GetComponent<IDraggable>();

                // Gérer l'affichage du bouton de rotation
                if (draggable != null && draggable.canBeRotated)
                {
                    // rotateButtonWhenMove.gameObject.SetActive(true);
                    // buttonsToShow.Add(rotateButtonWhenMove); // Ajout du bouton de rotation
                }
                else
                {
                    rotateButtonWhenMove.gameObject.SetActive(false);
                }

                // SetButtonPositions(screenPoint, buttonsToShow.ToArray());
            }
            else
            {
                // Récupérer l'interface IDraggable
                IDraggable draggable = selectedObject.GetComponent<IDraggable>();

                // Gérer l'affichage du bouton pour bouger le batiment
                if (draggable != null && draggable.CanBeMove)
                {
                    moveButton.gameObject.SetActive(true);
                }
                else
                {
                    moveButton.gameObject.SetActive(false);
                }

                // Gérer l'affichage du bouton de ramassage
                if (draggable != null && draggable.CanBePickup)
                {
                    pickupButton.gameObject.SetActive(true);
                }
                else
                {
                    pickupButton.gameObject.SetActive(false);
                }

                // Gérer l'affichage du bouton d'amélioration
                if (
                    draggable != null
                    && draggable.GadgetsData != null
                    && draggable.GadgetsData.upgradeStateAndPrice.ContainsKey(draggable.Level + 1)
                )
                {
                    upgradeButton.gameObject.SetActive(true);
                }
                else
                {
                    upgradeButton.gameObject.SetActive(false);
                }

                // Gérer l'affichage du bouton de rotation
                if (draggable != null && draggable.canBeRotated)
                {
                    // rotateButton.gameObject.SetActive(true);
                }
                else
                {
                    rotateButton.gameObject.SetActive(false);
                }

                // Gérer l'affichage du bouton de recuitement
                if (draggable != null && draggable.canOpenRecruitPanel)
                {
                    recruitButton.gameObject.SetActive(true);
                }
                else
                {
                    recruitButton.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowGadgetsList()
    {
        if (isMoving)
            return;
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
            "Main/Body/Viewport/Content"
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
        if (isMoving)
            return;

        List<GadgetsStoreValue> gadgets = Inventory.Instance.gadgets;
        Debug.Log(gadgets.Count);

        GadgetsListPanelClearContent();

        foreach (var gadgetValue in gadgets)
        {
            GameObject item = Instantiate(
                gadgetsListPrefab,
                gadgetsListPanelObject.transform.Find("Main/Body/Viewport/Content")
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
                    hasSpawnFromInventory = true;
                    SpawnGadget(gadgetValue.data.gadgetId, gadgetValue.level);
                });
            }
            else
            {
                Debug.LogError("Button component not found in the instantiated gadget item.");
            }

            item.SetActive(true);
        }
    }

    public void SpawnGadget(int gadgetId, int level, bool removeFromInventory = false)
    {
        hasSpawnFromInventory = true;
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
            onNextConfirmPlaceRemoveFromInventory = removeFromInventory;
        }
    }

    #region Area Selection

    private UDictionary<Vector2, Vector2> GetAreaSizePlacement()
    {
        UDictionary<Vector2, Vector2> areaSizePlacement = new UDictionary<Vector2, Vector2>();

        foreach (BuildingList buildingList in VillageRessourcesManager.Instance.buildingList)
        {
            var gadgetsData = buildingList.gadgetsData;

            foreach (var item in buildingList.buildings)
            {
                Vector2 topLeft = item.position + gadgetsData.areaSizePlacement.Keys.First();
                Vector2 bottomRight = item.position + gadgetsData.areaSizePlacement.Values.First();
                areaSizePlacement.Add(topLeft, bottomRight);
            }
        }

        return areaSizePlacement;
    }

    private void GenerateCombinedMesh()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(); // Nouvelle liste pour les UV

        // Dictionnaire pour détecter les coins partagés
        Dictionary<Vector2, int> vertexLookup = new Dictionary<Vector2, int>();

        foreach ((var topLeft, var bottomRight) in GetAreaSizePlacement())
        {
            // Convertir chaque coin en coordonnées 3D
            Vector3[] rectVertices = new Vector3[]
            {
            new Vector3(topLeft.x, topLeft.y, 0),
            new Vector3(bottomRight.x, topLeft.y, 0),
            new Vector3(bottomRight.x, bottomRight.y, 0),
            new Vector3(topLeft.x, bottomRight.y, 0)
            };

            Vector2[] rectUVs = new Vector2[] // UV pour chaque coin du rectangle
            {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 0)
            };

            int[] rectTriangles = new int[6];

            // Ajout des vertices et UV tout en évitant les duplications
            for (int i = 0; i < rectVertices.Length; i++)
            {
                Vector2 vertex2D = new Vector2(rectVertices[i].x, rectVertices[i].y);

                if (vertexLookup.TryGetValue(vertex2D, out int existingIndex))
                {
                    rectTriangles[i] = existingIndex;
                }
                else
                {
                    int newIndex = vertices.Count;
                    vertices.Add(rectVertices[i]);
                    uvs.Add(rectUVs[i]); // Ajouter l'UV correspondant
                    vertexLookup[vertex2D] = newIndex;
                    rectTriangles[i] = newIndex;
                }
            }

            // Ajouter les triangles pour le rectangle
            triangles.Add(rectTriangles[0]);
            triangles.Add(rectTriangles[1]);
            triangles.Add(rectTriangles[2]);

            triangles.Add(rectTriangles[0]);
            triangles.Add(rectTriangles[2]);
            triangles.Add(rectTriangles[3]);
        }

        // Mettre à jour le mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray(); // Assigner les UV au mesh
        mesh.RecalculateNormals();

        borderMeshFilter.mesh = GenerateBorderMesh(mesh, 0.5f);
    }


    private Mesh GenerateBorderMesh(Mesh originalMesh, float borderWidth)
    {
        Mesh borderMesh = new Mesh();

        // Récupérer les sommets et triangles du mesh d'origine
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;

        // Dictionnaire pour compter les arêtes
        Dictionary<(int, int), int> edgeCount = new Dictionary<(int, int), int>();

        // Fonction pour ajouter une arête en gardant toujours l’ordre (min, max)
        void AddEdge(int indexA, int indexB)
        {
            var edge = (Mathf.Min(indexA, indexB), Mathf.Max(indexA, indexB));
            if (edgeCount.ContainsKey(edge))
                edgeCount[edge]++;
            else
                edgeCount[edge] = 1;
        }

        // Ajouter chaque arête des triangles et compter les occurrences
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            AddEdge(originalTriangles[i], originalTriangles[i + 1]);
            AddEdge(originalTriangles[i + 1], originalTriangles[i + 2]);
            AddEdge(originalTriangles[i + 2], originalTriangles[i]);
        }

        // Liste des arêtes extérieures (apparues une seule fois)
        List<(Vector3, Vector3)> outerEdges = new List<(Vector3, Vector3)>();
        foreach (var edge in edgeCount)
        {
            if (edge.Value == 1) // Si l'arête n'est utilisée qu'une fois, c'est une arête extérieure
            {
                Vector3 v1 = originalVertices[edge.Key.Item1];
                Vector3 v2 = originalVertices[edge.Key.Item2];
                outerEdges.Add((v1, v2));
            }
        }

        // Créer la bordure le long des arêtes extérieures
        List<Vector3> borderVertices = new List<Vector3>();
        List<int> borderTriangles = new List<int>();

        int vertexOffset = 0;
        foreach ((Vector3 start, Vector3 end) in outerEdges)
        {
            // Calcul de la direction et du vecteur perpendiculaire pour le décalage
            Vector3 edgeDir = (end - start).normalized;
            Vector3 offset = new Vector3(-edgeDir.y, edgeDir.x, 0) * borderWidth;

            // Créer les sommets intérieurs et extérieurs pour l’arête
            Vector3 outerStart = start + offset;
            Vector3 outerEnd = end + offset;
            Vector3 innerStart = start - offset;
            Vector3 innerEnd = end - offset;

            // Ajouter les sommets au mesh de bordure
            borderVertices.Add(outerStart);
            borderVertices.Add(innerStart);
            borderVertices.Add(outerEnd);
            borderVertices.Add(innerEnd);

            // Créer les triangles pour la bande de bordure
            borderTriangles.Add(vertexOffset);
            borderTriangles.Add(vertexOffset + 1);
            borderTriangles.Add(vertexOffset + 2);

            borderTriangles.Add(vertexOffset + 1);
            borderTriangles.Add(vertexOffset + 3);
            borderTriangles.Add(vertexOffset + 2);

            vertexOffset += 4; // Mise à jour de l'offset pour les prochains sommets
        }

        // Mise à jour du mesh de bordure
        borderMesh.vertices = borderVertices.ToArray();
        borderMesh.triangles = borderTriangles.ToArray();
        borderMesh.RecalculateNormals();
        borderMesh.RecalculateBounds();

        return borderMesh;
    }



    #endregion
}

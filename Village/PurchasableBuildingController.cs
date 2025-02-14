using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasableBuildingController : MonoBehaviour
{
    public GadgetsData gadgetData;
    [SerializeField] private MeshFilter meshFilter;
    private float baseMeshZ = -1;
    private Mesh mesh;
    [SerializeField] private Transform meshTransform;
    [SerializeField] private Transform lockButtonParent;
    [SerializeField] private Button lockButton;
    [SerializeField] private Transform buyButtonParent;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject buyButtonGreenBackground;
    [SerializeField] private TextMeshProUGUI PriceText;
    private BuildingList.Buildings building;
    private IDraggable draggable;
    private bool isConstructionMode = false;

    private void Awake()
    {
        RessourcesManager.Instance.OnValueChanged += OnValueChanged;
    }

    private void OnDestroy()
    {
        RessourcesManager.Instance.OnValueChanged -= OnValueChanged;
    }

    private void Start()
    {
        buyButton.onClick.AddListener(() =>
        {
            if (VillageRessourcesManager.Instance != null && VillageRessourcesManager.Instance.TryPurchaseBuilding(building, gadgetData))
            {
                draggable.Buy();
                HideBuyButton();
                if (TutorialManager.Instance != null)
                    TutorialManager.Instance.BuildingPlaced(gadgetData.gadgetId);
            }
        });
        lockButton.onClick.AddListener(() =>
        {
            if (VillageRessourcesManager.Instance != null)
            {
                VillageMovingManager.Instance.ShowCantBuyBuildingAlert();
            }
        });
    }

    private void OnValueChanged()
    {
        if (IsPurchased() || !isConstructionMode)
        {
            HideBuyButton();
            HideLockButton();
        }
        else if (!IsLocked())
        {
            if (!IsPurchased() && isConstructionMode)
                ShowBuyButton();
            HideLockButton();
        }
    }

    private bool IsLocked()
    {
        return VillageRessourcesManager.Instance != null && VillageRessourcesManager.Instance.IsBuildingLocked(building);
    }

    private bool IsPurchased()
    {
        return VillageRessourcesManager.Instance != null && VillageRessourcesManager.Instance.IsBuildingPurchased(building);
    }

    private void ShowBuyButton()
    {
        buyButtonParent.gameObject.SetActive(true);
        if (building.price == 0)
        {
            buyButtonGreenBackground.SetActive(true);
        }
        else
        {
            buyButtonGreenBackground.SetActive(false);
        }
        HideMeshFilter();
    }

    private void HideBuyButton()
    {
        buyButtonParent.gameObject.SetActive(false);
        HideMeshFilter();
    }

    private void HideLockButton()
    {
        if (lockButtonParent != null)
            lockButtonParent.gameObject.SetActive(false);
        HideMeshFilter();
    }

    private void HideMeshFilter()
    {
        if (IsPurchased())
        {
            meshFilter.gameObject.SetActive(false);
        }
        else
        {
            meshFilter.gameObject.SetActive(true);
        }
    }

    public void SpawnBuilding(BuildingList.Buildings buildingObject)
    {
        building = buildingObject;
        GameObject buildingInstance = Instantiate(gadgetData.prefab, transform.position, Quaternion.Euler(0, 0, buildingObject.rotationZ));
        buildingInstance.transform.SetParent(transform);

        meshTransform.position = meshTransform.position + new Vector3(0, 0, 4f);
        WorldCanvasManager.Instance.AddUIElementToCanvas(buyButtonParent.gameObject);
        buyButtonParent.position = buildingInstance.transform.position + new Vector3(0, 2f, -7.5f);
        WorldCanvasManager.Instance.AddUIElementToCanvas(lockButtonParent.gameObject);
        lockButtonParent.position = buildingInstance.transform.position + new Vector3(0, 2f, -7.5f);

        if (IsPurchased()) // si le batiment est acheté tout cacher
        {
            HideBuyButton();
            HideLockButton();
        }
        else if (IsLocked()) // sinon si il est lock cacher le bouton achat
        {
            HideBuyButton();
        }
        if (!IsPurchased()) // sinon afficher le bouton achat
        {
            GenerateCombinedMesh(gadgetData, buildingObject);
            meshTransform.rotation = Quaternion.Euler(0, 0, buildingObject.rotationZ + 90);
            if (buildingObject.price == 0)
            {
                // si le prix est 0 afficher le nom du batiment
                PriceText.text = gadgetData.displayName.ToUpper();
            }
            else
            {
                PriceText.text = buildingObject.price.ToString() + " GOLD";
            }
        }

        // Perform necessary setup
        buildingInstance.SetActive(false); // Disable the instance to prevent visual artifacts
        draggable = buildingInstance.GetComponent<IDraggable>();
        if (draggable != null)
        {
            draggable.IsPurchaseable = true;
            draggable.IsBought = IsPurchased();
            draggable.Level = buildingObject.level;
            draggable.DisableDragObject = !IsPurchased();
            draggable.SpawnToVillage(); // Perform necessary setup
        }
        buildingInstance.SetActive(true);
        HideUnboughtBuilding();
    }

    public void ShowUnboughtBuilding()
    {
        if (!IsPurchased())
        {
            ShowBuyButton();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
        isConstructionMode = true;
    }

    public void HideUnboughtBuilding()
    {
        if (!IsPurchased())
        {
            HideBuyButton();
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
        isConstructionMode = false;
    }

    private UDictionary<Vector2, Vector2> GetAreaSizePlacement(GadgetsData gadgetsData, BuildingList.Buildings building)
    {
        UDictionary<Vector2, Vector2> areaSizePlacement = new UDictionary<Vector2, Vector2>();

        Vector2 topLeft = gadgetsData.areaSizePlacement.Keys.First();
        Vector2 bottomRight = gadgetsData.areaSizePlacement.Values.First();
        areaSizePlacement.Add(topLeft, bottomRight);

        return areaSizePlacement;
    }

    private void GenerateCombinedMesh(GadgetsData gadgetsData, BuildingList.Buildings building)
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(); // Nouvelle liste pour les UV

        // Dictionnaire pour détecter les coins partagés
        Dictionary<Vector2, int> vertexLookup = new Dictionary<Vector2, int>();

        var areaSizePlacement = GetAreaSizePlacement(gadgetsData, building);
        var topLeft = areaSizePlacement.Keys.First();
        var bottomRight = areaSizePlacement.Values.First();

        // Convertir chaque coin en coordonnées 3D
        Vector3[] rectVertices = new Vector3[]
        {
            new Vector3(topLeft.x, topLeft.y, baseMeshZ),
            new Vector3(bottomRight.x, topLeft.y, baseMeshZ),
            new Vector3(bottomRight.x, bottomRight.y, baseMeshZ),
            new Vector3(topLeft.x, bottomRight.y, baseMeshZ)
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


        // Mettre à jour le mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray(); // Assigner les UV au mesh
        mesh.RecalculateNormals();
    }
}

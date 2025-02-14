using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllyBotManager : MonoBehaviour
{
    public static AllyBotManager Instance;

    [SerializeField] private GameObject playerCompanionPrefab;
    [SerializeField] private GameObject turretCompanionPrefab;

    public UDictionary<Transform, bool> companionTransform;

    [Header("UI Elements")]
    [SerializeField] private IngameShopManager ingameShopManager;
    [SerializeField] private RectTransform companionShopPanel;
    [SerializeField] private Button companionShopCloseButton;
    [SerializeField] private Button companionShopConfirm;
    [SerializeField] private Button companionShopWatchAd;
    [SerializeField] private TextMeshProUGUI numberOfCompanionText;
    [SerializeField] private TextMeshProUGUI goldCostAmoutText;


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

        if (companionShopPanel != null)
            companionShopPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (companionShopCloseButton != null)
            companionShopCloseButton.onClick.AddListener(HideCompanionShopPanel);
    }

#if UNITY_EDITOR
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.C))
        // {
        //     AddCompanion();
        // }
    }
#endif

    private void HideCompanionShopPanel()
    {
        if (ingameShopManager != null)
            ingameShopManager.HidePanel();
        Time.timeScale = 1;
        companionShopPanel.gameObject.SetActive(false);
        // companionShopConfirm.onClick.RemoveAllListeners();
        // companionShopWatchAd.onClick.RemoveAllListeners();
    }
    public void ShowCompanionShopPanel()
    {
        int empltyPlaceExist = 0;
        foreach (var companion in companionTransform)
        {
            if (!companion.Value)
            {
                empltyPlaceExist++;
            }
        }

        if (empltyPlaceExist == 0) return;
        companionShopPanel.gameObject.SetActive(true);

        Time.timeScale = 0;
        companionShopPanel.gameObject.SetActive(true);

        int numberOfCompanionAlreadyHave = companionTransform.Count - empltyPlaceExist;

        numberOfCompanionText.text = $"{numberOfCompanionAlreadyHave}/{companionTransform.Count} Allies";
        // goldPlayerAlreadyHaveAmoutText.text = $"{RessourcesManager.Instance.goldAmount} GOLD";
        goldCostAmoutText.text = $"{RessourcesManager.Instance.costPerPlayerCompanion[numberOfCompanionAlreadyHave + 1]} GOLD";

        companionShopConfirm.onClick.RemoveAllListeners();
        companionShopWatchAd.onClick.RemoveAllListeners();

        companionShopConfirm.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            companionShopPanel.gameObject.SetActive(false);

            if (!RessourcesManager.Instance.costPerPlayerCompanion.ContainsKey(numberOfCompanionAlreadyHave + 1)) return;

            RessourcesManager.Instance.Buy(PaymantSubtype.Gold, RessourcesManager.Instance.costPerPlayerCompanion[numberOfCompanionAlreadyHave + 1], () =>
            {
                AddCompanionToPlayer();
            }, () =>
            {
                Debug.LogWarning("Error during upgrade");
            });
        });
        companionShopWatchAd.onClick.AddListener(() =>
        {
            AdmobManager.Instance.ShowRewardedAd((rewardAd) =>
            {
                Time.timeScale = 1;
                companionShopPanel.gameObject.SetActive(false);
                AddCompanionToPlayer();
            });
        });
    }

    public void AddCompanionToPlayer()
    {
        Debug.Log("AddCompanion called.");

        // Créez une liste temporaire pour stocker les clés à modifier
        List<Transform> keysToUpdate = new List<Transform>();

        foreach (var companion in companionTransform)
        {
            if (!companion.Value)
            {
                keysToUpdate.Add(companion.Key);
                break;
            }
        }

        // Maintenant, modifiez le dictionnaire en dehors de la boucle foreach
        foreach (var key in keysToUpdate)
        {
            Vector3 position = key.position;

            // Vérifiez les valeurs de position avant l'instantiation
            if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z) ||
                float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
            {
                Debug.LogError($"Invalid position detected before instantiation: {position}");
                continue;
            }

            GameObject companionObject = Instantiate(playerCompanionPrefab, position, Quaternion.identity);
            CompanionEntity companionEntity = companionObject.GetComponent<CompanionEntity>();

            if (companionEntity != null)
            {
                companionTransform[key] = true;
                companionEntity.Initialize(key);
            }

            IEnemy enemy = companionEntity.GetComponent<IEnemy>();
            enemy?.GoToPlayerSide();
        }
    }
    public Transform AddCompanionToTurret(Transform spawnPosition)
    {

        GameObject companionObject = Instantiate(turretCompanionPrefab, spawnPosition.position, Quaternion.identity);
        CompanionEntity companionEntity = companionObject.GetComponent<CompanionEntity>();

        companionEntity?.Initialize(spawnPosition);

        IEnemy enemy = companionEntity.GetComponent<IEnemy>();
        enemy?.GoToPlayerSide();

        return companionEntity.transform;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class ResumGameManager : MonoBehaviour
{
    [SerializeField] private GameObject itemsPrefab;

    [SerializeField] private TextMeshProUGUI killCounterTextMeshPro;
    [SerializeField] private TextMeshProUGUI goldAmountTextMeshPro;
    private DynamicAddingTextEffect goldAmountTextEffect;

    [SerializeField] private GameObject goldScreen;
    [SerializeField] private GameObject itemsScreen;

    [SerializeField] private Button continueButton;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button backToMenuButton;

    private double golds;
    private bool hadDoubledGolds = false;

    private void Awake()
    {
        if (goldAmountTextMeshPro != null && goldAmountTextMeshPro.TryGetComponent(out DynamicAddingTextEffect effect))
        {
            goldAmountTextEffect = effect;
        }
        continueButton.onClick.AddListener(Continue);
        watchAdButton.onClick.AddListener(WatchAd);
        backToMenuButton.onClick.AddListener(BackToMenu);
    }

    private void Start()
    {
        goldScreen.SetActive(true);
        itemsScreen.SetActive(false);
    }

    public void SetTextGoldAmount()
    {
        golds = Inventory.Instance.goldAmount;
        goldAmountTextEffect.SetNumber((float)golds);
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
        {
            if (EnemyCounterManager.Instance != null)
                killCounterTextMeshPro.text = EnemyCounterManager.Instance.killCount.ToString();

            // Clear previous items from the list
            Transform listParent = itemsScreen.transform.Find("ListParent");
            if (listParent != null)
            {
                foreach (Transform child in listParent)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                Debug.LogError("ListParent transform not found on itemsScreen.");
            }

            // For items
            foreach (ItemsAndAmount item in Inventory.Instance.items)
            {
                GameObject itemObject = Instantiate(itemsPrefab, listParent);
                if (itemObject != null)
                {
                    TextMeshProUGUI text = itemObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = item.amount.ToString();
                    }
                    else
                    {
                        Debug.LogError("TextMeshProUGUI component not found in instantiated item prefab.");
                    }

                    Image[] images = itemObject.GetComponentsInChildren<Image>();
                    if (images != null)
                    {
                        foreach (var img in images)
                        {
                            if (img.gameObject.name == "Image")
                            {
                                img.sprite = item.data.visual;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Image component not found in instantiated item prefab.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to instantiate item prefab.");
                }
            }
        }
        else
        {
            Debug.LogError("Inventory.Instance is null.");
        }
    }

    private void Continue()
    {
        goldScreen.SetActive(false);
        // TODO: if add items show the items screen and remove BackToMenu()
        // itemsScreen.SetActive(true);
        BackToMenu();
    }

    private void WatchAd()
    {
        AdmobManager.Instance.ShowRewardedAd((Reward reward) =>
       {
           DoubleGold();
           TextMeshProUGUI amountGoldText = goldScreen.transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>();
           if (amountGoldText != null)
           {
               goldAmountTextEffect.SetNumber((float)golds);
           }
           else
           {
               Debug.LogError("TextMeshProUGUI component for gold amount not found on goldScreen.");
           }
           watchAdButton.interactable = false;
       });
    }

    private void BackToMenu()
    {
        StaticDataManager.BackToMenu(Inventory.Instance.items, hadDoubledGolds ? golds / 2 : 0);
    }

    private void DoubleGold()
    {
        golds *= 2;
        hadDoubledGolds = true;
    }
}

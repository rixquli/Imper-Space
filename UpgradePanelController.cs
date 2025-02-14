using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelController : MonoBehaviour
{
    [Header("Left And Right Button")]
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;

    [Header("Left Upgrade Panel Side")]
    [SerializeField] private Image equipmentImage;

    [Header("Right Upgrade Panel Side")]
    [SerializeField] private TextMeshProUGUI equipmentLevel;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private DynamicProgressBarEffect currentStageProgressBar;
    [SerializeField] private DynamicProgressBarEffect nextStageProgressBar;
    [SerializeField] private Image costRessourceImage;
    [SerializeField] private TextMeshProUGUI costPriceText;
    [SerializeField] private GameObject statsParentObject;
    [SerializeField] private GameObject statsPrefab;
    [SerializeField] private Button upgradeButton;

    [Header("Effects")]
    [SerializeField] private UIEffectsManager equipmentImageEffect;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Icon For Stats")]
    [SerializeField] private UDictionary<EquiementsStats, Sprite> equipmentStatsSprites;

    private EquipementData equipementData;

    private void Start()
    {
        leftButton.GetComponent<Button>().onClick.AddListener(() => SwitchEquipmentToLeft(RessourcesManager.Instance.GetPreviousEquipement(equipementData)));
        rightButton.GetComponent<Button>().onClick.AddListener(() => SwitchEquipmentToRight(RessourcesManager.Instance.GetNextEquipement(equipementData)));

        equipmentImageEffect.Settings.Find(x => x.Name == "SwipeToLeft").OnFinished.AddListener(SwipeToLeftAnimationFinished);
        equipmentImageEffect.Settings.Find(x => x.Name == "SwipeToRight").OnFinished.AddListener(SwipeToRightAnimationFinished);

        upgradeButton.onClick.AddListener(() =>
        {
            RessourcesManager.Instance.UpgradeEquipement(equipementData);
            ChangeValue();
        });

        closeButton.onClick.AddListener(Hide);
    }

    public void Show(EquipementData equipementData)
    {
        this.equipementData = equipementData;
        gameObject.SetActive(true);
        ChangeValue();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SwitchEquipmentToLeft(EquipementData equipementData)
    {
        this.equipementData = equipementData;
        Debug.LogError(equipementData.name);
        equipmentImageEffect.Run("SwipeToLeft");
    }

    private void SwitchEquipmentToRight(EquipementData equipementData)
    {
        this.equipementData = equipementData;
        equipmentImageEffect.Run("SwipeToRight");
    }

    private void SwipeToRightAnimationFinished()
    {
        ChangeValue();
        equipmentImageEffect.Run("SwipeToCenterFromLeft");
    }

    private void SwipeToLeftAnimationFinished()
    {
        ChangeValue();
        equipmentImageEffect.Run("SwipeToCenterFromRight");
    }

    private void ChangeValue()
    {
        int level = RessourcesManager.Instance.GetLevel(equipementData);
        equipmentImage.sprite = equipementData.icon;
        equipmentLevel.text = "Lvl." + level.ToString();
        stageText.text = "Stage " + (Mathf.CeilToInt((level - 1) / 4) + 1).ToString();
        if (level % 4 == 0)
            currentStageProgressBar.ResetProgress();
        else currentStageProgressBar.SetProgress(level % 4 / 4f);
        nextStageProgressBar.SetProgress(level % 4 / 4f + 0.24f);
        costRessourceImage.sprite = ItemDatabase.Instance.ressourceSprites[RessourceType.Gold];
        costPriceText.text = RessourcesManager.Instance.costPerLevelToUpgradeEquipment(level + 1).ToString();

        foreach (Transform child in statsParentObject.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var stat in equipementData.stats)
        {
            var statObject = Instantiate(statsPrefab, statsParentObject.transform);
            Sprite sprite = equipmentStatsSprites.TryGetValue(stat.Key, out sprite) ? sprite : null;
            statObject.GetComponent<UpgradePanelStats>().SetStat(equipementData, stat.Key, sprite);
        }

        // upgradeButton.interactable = equipementData.canUpgrade;
    }



}

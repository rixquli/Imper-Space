using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatInfoPopupController : MonoBehaviour
{
    [Header("Info Popup elements")]
    [SerializeField]
    private Button infoPopUpCloseButton;

    [SerializeField]
    private TextMeshProUGUI infoPopUpTitleText;

    [SerializeField]
    private UDictionary<EquiementsStats, TextMeshProUGUI> infoPopUpStatsText;
    [SerializeField]
    private UDictionary<EquiementsStats, GameObject> infoPopUpStatsParents;

    [SerializeField]
    private UDictionary<RarityType, TextMeshProUGUI> infoPopUpRaritiesText;

    [SerializeField]
    private TextMeshProUGUI infoPopUpBulletPerSecondStatText; //TODO: integrer infoPopUpBulletPerSecondStatText au dictionnare
    [SerializeField]
    private GameObject infoPopUpBulletPerSecondStatParent; // TODO: integrer infoPopUpBulletPerSecondStatParent au dictionnare

    [SerializeField]
    private Image infoPopUpImage;

    [SerializeField]
    private GameObject infoPopUpRaritiesObject;

    [SerializeField]
    private GameObject infoPopUpSatsObject;

    private void Awake()
    {
        gameObject.SetActive(false);
        infoPopUpCloseButton.onClick.AddListener(CloseInfoPopUp);
    }

    private void CloseInfoPopUp()
    {
        gameObject.SetActive(false);
    }

    public void OpenInfoPopUp(EquipementData currentInfoPopupEquipement)
    {
        infoPopUpSatsObject.SetActive(true);
        infoPopUpRaritiesObject.SetActive(false);
        UpdateStatTexts(currentInfoPopupEquipement);
        infoPopUpImage.sprite = currentInfoPopupEquipement.icon;
        infoPopUpTitleText.text = currentInfoPopupEquipement.displayName;
        gameObject.SetActive(true);
    }

    public void OpenInfoPopUp(RandomGachaEquipementData currentInfoPopupEquipement)
    {
        infoPopUpSatsObject.SetActive(false);
        infoPopUpRaritiesObject.SetActive(true);
        UpdateRaritiesTexts(currentInfoPopupEquipement);
        infoPopUpImage.sprite = currentInfoPopupEquipement.visualIconInShop;
        infoPopUpTitleText.text = currentInfoPopupEquipement.displayName;
        gameObject.SetActive(true);
    }

    public void UpdateRaritiesTexts(RandomGachaEquipementData currentInfoPopupEquipement)
    {
        if (currentInfoPopupEquipement == null)
            return;

        infoPopUpRaritiesText[RarityType.Common].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Common
        );
        infoPopUpRaritiesText[RarityType.Rare].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Rare
        );
        infoPopUpRaritiesText[RarityType.Epic].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Epic
        );
        infoPopUpRaritiesText[RarityType.Legendary].text = GetRarity(
            currentInfoPopupEquipement,
            RarityType.Legendary
        );
    }

    public void UpdateStatTexts(EquipementData currentInfoPopupEquipement)
    {
        if (currentInfoPopupEquipement == null)
            return;
        if (currentInfoPopupEquipement.slot == EquipeSlot.Ship)
        {
            foreach (EquiementsStats item in EquiementsStats.GetValues(typeof(EquiementsStats)))
            {
                SetShipStat(item, currentInfoPopupEquipement);
            }

            if (infoPopUpBulletPerSecondStatParent != null)
            {
                infoPopUpBulletPerSecondStatParent.SetActive(true);
            }
            infoPopUpBulletPerSecondStatText.text =
                currentInfoPopupEquipement.bulletPerSecond.ToString();
        }
        else
        {
            foreach (EquiementsStats item in EquiementsStats.GetValues(typeof(EquiementsStats)))
            {
                SetEquipmentStat(item, currentInfoPopupEquipement);
            }

            infoPopUpBulletPerSecondStatParent.SetActive(false);
        }
    }

    private void SetEquipmentStat(EquiementsStats equiementsStats, EquipementData equipement)
    {
        if (equipement == null || equipement.stats == null || equipement.stats.Count == 0)
            return;
        string statToShow = GetStat(
                equipement,
                equiementsStats
            );
        if (statToShow != "+0%" && infoPopUpStatsText.ContainsKey(equiementsStats))
        {
            infoPopUpStatsText[equiementsStats].text = statToShow;
            if (infoPopUpStatsParents.ContainsKey(equiementsStats))
                infoPopUpStatsParents[equiementsStats].SetActive(true);
        }
        else if (infoPopUpStatsParents.ContainsKey(equiementsStats))
            infoPopUpStatsParents[equiementsStats].SetActive(false);
    }

    private void SetShipStat(EquiementsStats equiementsStats, EquipementData equipement)
    {
        if (equipement == null || equipement.stats == null || equipement.stats.Count == 0)
            return;
        string statToShow = GetShipStat(
                equipement,
                equiementsStats
            );
        if (statToShow != "" && infoPopUpStatsText.ContainsKey(equiementsStats))
        {
            infoPopUpStatsText[equiementsStats].text = statToShow;
            if (infoPopUpStatsParents.ContainsKey(equiementsStats))
                infoPopUpStatsParents[equiementsStats].SetActive(true);
        }
        else if (infoPopUpStatsParents.ContainsKey(equiementsStats))
            infoPopUpStatsParents[equiementsStats].SetActive(false);
    }

    private string GetShipStat(EquipementData equipement, EquiementsStats statType)
    {
        if (equipement == null || equipement.stats == null || equipement.stats.Count == 0)
            return ""; // Changed to return 1 if the equipement or stats are null/empty

        float value = AttributesData.GetShipStat(equipement, statType);
        return value == 0 ? "" : Mathf.RoundToInt(value).ToString();
    }

    private string GetStat(EquipementData equipement, EquiementsStats statType)
    {
        if (equipement == null || equipement.stats == null || equipement.stats.Count == 0)
            return "+0%"; // Changed to return "0%" if the equipement or stats are null/empty

        float value = AttributesData.GetStat(equipement, statType);
        return value == 0 ? "+0%" : $"{(value >= 0 ? "+" : "−")}{Mathf.Round(Mathf.Abs(value * 100))}%";
    }

    private string GetRarity(RandomGachaEquipementData equipement, RarityType rarityType)
    {
        if (
            equipement == null
            || equipement.rarityPercentages == null
            || equipement.rarityPercentages.Count == 0
        )
            return ""; // Changed to return "0%" if the equipement or stats are null/empty

        if (equipement.rarityPercentages.TryGetValue(rarityType, out float value))
        {
            return $"{value}%";
        }
        return ""; // Changed to return "0%" if the stat is not found
    }

}

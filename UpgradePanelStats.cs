using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelStats : MonoBehaviour
{
    [SerializeField] private Image statImage;
    [SerializeField] private TextMeshProUGUI statCurrentValue;
    [SerializeField] private TextMeshProUGUI statNextValue;

    public void SetStat(EquipementData equipementData, EquiementsStats stat, Sprite sprite)
    {
        // statImage.sprite = GetSprite(stat);
        if (equipementData.slot == EquipeSlot.Ship)
        {
            statCurrentValue.text = Mathf.Round(AttributesData.GetStat(equipementData, stat) * 100).ToString();
            statNextValue.text = "+" + Mathf.Round(AttributesData.GetUpgradeStat(equipementData, stat) * 100).ToString();
        }
        else
        {
            statCurrentValue.text = "+" + Mathf.Round(AttributesData.GetStat(equipementData, stat) * 100).ToString() + "%";
            statNextValue.text = "+" + Mathf.Round(AttributesData.GetUpgradeStat(equipementData, stat) * 100).ToString();
        }

        if (sprite != null)
        {
            statImage.sprite = sprite;
        }
    }
}

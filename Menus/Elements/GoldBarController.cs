using TMPro;
using UnityEngine;

public class GoldBarController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI goldsAmountText;

    private void Start()
    {
        RessourcesManager.Instance.OnValueChanged += UpdateAmountTexts;
        RessourcesManager.Instance.RefreshValue();
    }

    private void OnDestroy()
    {
        RessourcesManager.Instance.OnValueChanged -= UpdateAmountTexts;
    }

    private void UpdateAmountTexts()
    {
        goldsAmountText.text = RessourcesManager.Instance.goldAmount.ToString();
    }
}

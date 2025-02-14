using TMPro;
using UnityEngine;

public class GemBarController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gemsAmountText;

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
        gemsAmountText.text = RessourcesManager.Instance.gemAmount.ToString();
    }
}

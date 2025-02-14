using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftCodeInput : MonoBehaviour
{
    private Button btn;
    [SerializeField] private string code;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PopupController popupControllerSuccess;
    [SerializeField] private PopupController popupControllerFail;

    private void Awake()
    {
        btn = GetComponentInChildren<Button>();
        btn.onClick.AddListener(async () =>
        {
            Debug.Log("Button Clicked");
            GiftCodeInDBReward reward = await DataPersistenceManager.instance.gameData.giftCodes.ClaimCode(code);
            if (reward != null)
            {
                popupControllerSuccess.SetTexts(0, reward.amount);
                popupControllerSuccess.SetTexts(1, reward.type);
                popupControllerSuccess.ShowPopup();
            }
            else
            {
                popupControllerFail.ShowPopup();
            }
        });
        inputField.onValueChanged.AddListener((value) =>
        {
            code = value;
        });
    }
}

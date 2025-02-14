using UnityEngine;
using UnityEngine.UI;

public class ShopCardGemAndGoldData : MonoBehaviour
{
    public Button buyButton;
    public Image image;
    public ConsumableEquipementData data;

    private void Awake()
    {
        if (!data.requireIAP)
            buyButton.onClick.AddListener(OnButtonClick);
        image.sprite = data.visualIconInShop;
    }

    private void OnButtonClick()
    {
        GetComponentInParent<ShopManager>().OpenPopUp(data);
    }
}

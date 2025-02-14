using UnityEngine;
using UnityEngine.UI;

public class ShopCardShipData : MonoBehaviour
{
    public Button buyButton;
    public Button infoButton;
    public Image image;
    public NonConsumableEquipementData data;

    private void Awake()
    {
        if (!data.requireIAP)
            buyButton.onClick.AddListener(OnBuyButtonClick);
        infoButton.onClick.AddListener(OnInfoButtonClick);
        image.sprite = data.visualIconInShop;
    }

    private void OnBuyButtonClick()
    {
        GetComponentInParent<ShopManager>().OpenPopUp(data);
    }
    private void OnInfoButtonClick()
    {
        GetComponentInParent<ShopManager>().OpenInfoPopUp(data.data);
    }
}


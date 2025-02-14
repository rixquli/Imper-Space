using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class ShopCardItemsData : MonoBehaviour
{
    public Button buyButton;
    public Button infoButton;
    public Image image;
    public RandomGachaEquipementData data;
    public TextMeshProUGUI priceText;
    public LocalizedString localizedStringPriceText;
    public bool isWatchAd;
    private IntVariable amountAdWatched;
    private IntVariable maxAmountAdWatched;

    private void Awake()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnButtonClick);
        infoButton.onClick.AddListener(OnInfoButtonClick);
        image.sprite = data.visualIconInShop;
    }

    private void Start()
    {
        if (localizedStringPriceText != null && isWatchAd)
        {
            if (!localizedStringPriceText.TryGetValue("amount", out var amountAdWatchedVariable))
            {
                amountAdWatched = new IntVariable();
                localizedStringPriceText.Add("amount", amountAdWatched);
            }
            else
            {
                amountAdWatched = amountAdWatchedVariable as IntVariable;
            }

            if (!localizedStringPriceText.TryGetValue("max", out var maxAmountAdWatchedVariable))
            {
                maxAmountAdWatched = new IntVariable();
                localizedStringPriceText.Add("max", maxAmountAdWatched);
            }
            else
            {
                maxAmountAdWatched = maxAmountAdWatchedVariable as IntVariable;
            }

            localizedStringPriceText.StringChanged += UpdateText;
            RessourcesManager.Instance.OnValueChanged += OnValueChanged;
            OnValueChanged(); // Initialiser les valeurs immédiatement
        }
    }

    private void OnDestroy()
    {
        if (localizedStringPriceText != null && isWatchAd)
        {
            localizedStringPriceText.StringChanged -= UpdateText;
            RessourcesManager.Instance.OnValueChanged -= OnValueChanged;
        }
    }
    private void UpdateText(string value)
    {
        Debug.Log("UpdateText" + value);
        priceText.text = value;
    }

    private void OnValueChanged()
    {
        amountAdWatched.Value = RessourcesManager.Instance.watchPerDay.watchAmountForFreePulling;
        maxAmountAdWatched.Value = RessourcesManager.Instance.watchPerDay.maxWatchAmountForFreePulling;
        localizedStringPriceText.RefreshString();
    }

    private void OnButtonClick()
    {
        GetComponentInParent<ShopManager>().OpenPopUp(data);
    }

    private void OnInfoButtonClick()
    {
        GetComponentInParent<ShopManager>().OpenInfoPopUp(data);
    }
}

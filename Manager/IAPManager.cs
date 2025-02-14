using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    [SerializeField] private CodelessIAPButton gold240Button;
    public string environment = "production";

    private void Awake()
    {

        gold240Button.onPurchaseComplete.AddListener(OnPurchaseComplete);
    }

    async void Start()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            // An error occurred during initialization.
        }
    }

    private void OnPurchaseComplete(Product product)
    {
        switch (product.definition.payout.subtype)
        {
            case "Gold":
                RessourcesManager.Instance.AddGold(product.definition.payout.quantity);
                break;
            case "Gem":
                RessourcesManager.Instance.AddGems(product.definition.payout.quantity);
                break;
        }
    }

}

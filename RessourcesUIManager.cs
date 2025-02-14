using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

public class RessourcesUIManager : MonoBehaviour
{
    public static RessourcesUIManager Instance;

    private enum Ressources
    {
        Gem,
        Gold,
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void AddAdButtonListener(Button button, Ressources ressource, int amount)
    {
        button.onClick.AddListener(() =>
        {
            AdmobManager.Instance.ShowRewardedAd(
                (Reward reward) =>
                {
                    UnityMainThreadDispatcher
                        .Instance()
                        .Enqueue(() =>
                        {
                            switch (ressource)
                            {
                                case Ressources.Gold:
                                    AddGold(amount);
                                    break;
                                case Ressources.Gem:
                                    AddGems(amount);
                                    break;
                            }
                        });
                }
            );
            // AdsManager.Instance.rewardedAds.ShowRewardedAd((UnityAdsShowCompletionState showCompletionState) =>
            // {
            //     Debug.Log("Callback invoked. Ad state: " + showCompletionState);
            //     if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            //     {
            //         switch (ressource)
            //         {
            //             case Ressources.Gold:
            //                 AddGold(amount);
            //                 break;
            //             case Ressources.Gem:
            //                 AddGems(amount);
            //                 break;
            //         }
            //     }
            // });
        });
    }

    private void AddGems(int amount)
    {
        RessourcesManager.Instance.AddGems(amount);
    }

    private void AddGold(int amount)
    {
        RessourcesManager.Instance.AddGold(amount);
    }

    private void AddGems(int amount, float timeToWait)
    {
        StartCoroutine(WaitBeforeAddGems(amount, timeToWait));
    }

    private void AddGold(int amount, float timeToWait)
    {
        StartCoroutine(WaitBeforeAddGold(amount, timeToWait));
    }

    private IEnumerator WaitBeforeAddGems(int amount, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        RessourcesManager.Instance.AddGems(amount);
    }

    private IEnumerator WaitBeforeAddGold(int amount, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        RessourcesManager.Instance.AddGold(amount);
    }
}

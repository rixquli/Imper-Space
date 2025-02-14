//Script Made By www.UnitySourceCode.store
//This Is The Latest Updated Script For Google Mobile Ads Plugin (Latest Version As Of 18th July 2024)

using System;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdmobManager : MonoBehaviour
{
    BannerView bannerView;
    private RewardedInterstitialAd rewardedInterstitialAd;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    public string bannerId = "";
    public string rewardInterstitialId = "";
    public string interstitialId = "";
    public string rewardedId = "";

    public static AdmobManager Instance;

    public delegate void RewardAdCompletedCallback(Reward reward);
    private RewardAdCompletedCallback onRewardAdCompletedCallback;
    public delegate void RewardInterstitialAdCompletedCallback();
    private RewardInterstitialAdCompletedCallback onRewardInterstitialAdCompletedCallback;
    public delegate void InterstitialAdCompletedCallback();
    private InterstitialAdCompletedCallback onInterstitialAdCompletedCallback;

    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize(
            (InitializationStatus initStatus) =>
            {
                // This callback is called once the MobileAds SDK is initialized.
                CreateBannerView();
                LoadRewardedAd();
                // LoadRewardInterstitialAd();
                // LoadInterstitialAd();
            }
        );
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ecoute si une scene est chargée
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //ADMOB BANNER****************************************************************************************

    public void CreateBannerView()
    {
        Debug.Log("Creating banner view");

        // If we already have a banner, destroy the old one.
        if (bannerView != null)
        {
            DestroyAd();
        }

        // Create a 320x50 banner at top of the screen
        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
        LoadShowBanner();
    }

    public void LoadShowBanner()
    {
        if (DataPersistenceManager.instance != null && DataPersistenceManager.instance.gameData.hasGoldPass)
        {
            return;
        }
        // create an instance of a banner view first.
        if (bannerView == null)
        {
            CreateBannerView();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerView.LoadAd(adRequest);
    }

    public void DestroyBannerAd()
    {
        bannerView.Destroy();
        bannerView.Hide();
    }

    public void ShowBanner()
    {
        if (DataPersistenceManager.instance.gameData.hasGoldPass || bannerView == null)
        {
            return;
        }
        bannerView.Show();
    }

    public void HideBanner()
    {
        if (DataPersistenceManager.instance.gameData.hasGoldPass || bannerView == null)
        {
            return;
        }
        bannerView.Hide();
    }

    private void ListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : " + bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : " + error);
            CreateBannerView();
        };
        // Raised when the ad is estimated to have earned money.
        bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(
                String.Format("Banner view paid {0} {1}.", adValue.Value, adValue.CurrencyCode)
            );
        };
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    public void DestroyAd()
    {
        if (bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            bannerView.Destroy();
            bannerView = null;
        }
    }

    //ADMOB INTERSTITIAL****************************************************************************************

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(
            interstitialId,
            adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError(
                        "interstitial ad failed to load an ad " + "with error : " + error
                    );
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

                interstitialAd = ad;
                RegisterEventHandlers(interstitialAd);
            }
        );
    }

    public void ShowInterstitialAd(InterstitialAdCompletedCallback callback)
    {
        onInterstitialAdCompletedCallback = callback;

        if (DataPersistenceManager.instance.gameData.hasGoldPass)
        {
            onInterstitialAdCompletedCallback?.Invoke();
            return;
        }
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
            onInterstitialAdCompletedCallback?.Invoke();
        }
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(
                String.Format("Interstitial ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode)
            );
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            LoadInterstitialAd();
            onInterstitialAdCompletedCallback?.Invoke();
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError(
                "Interstitial ad failed to open full screen content " + "with error : " + error
            );
            LoadInterstitialAd();
            onInterstitialAdCompletedCallback?.Invoke();
        };
    }

    //ADMOB REWARDED INTERSTITIAL****************************************************************************************

    public void LoadRewardInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        Debug.Log("Loading the rewarded interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedInterstitialAd.Load(
            rewardInterstitialId,
            adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError(
                        "Rewarded interstitial ad failed to load an ad " + "with error : " + error
                    );
                    return;
                }

                Debug.Log(
                    "Rewarded interstitial ad loaded with response : " + ad.GetResponseInfo()
                );

                rewardedInterstitialAd = ad;
                RegisterEventHandlers(rewardedInterstitialAd);
            }
        );
    }

    public void ShowRewardInterstitialAd(RewardInterstitialAdCompletedCallback callback)
    {
        onRewardInterstitialAdCompletedCallback = callback;
        const string rewardMsg =
            "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";

        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show(
                (Reward reward) =>
                {
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                    onRewardInterstitialAdCompletedCallback?.Invoke();
                }
            );
        }
        else
        {
            onRewardInterstitialAdCompletedCallback?.Invoke();
        }
    }

    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(
                String.Format(
                    "Rewarded interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode
                )
            );
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded interstitial ad recorded an impression.");
            onRewardInterstitialAdCompletedCallback?.Invoke();
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content closed.");
            LoadRewardInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError(
                "Rewarded interstitial ad failed to open full screen content "
                    + "with error : "
                    + error
            );
            LoadRewardInterstitialAd();
            onRewardInterstitialAdCompletedCallback?.Invoke();
        };
    }

    //ADMOB REWARDED ADS****************************************************************************************

    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(
            rewardedId,
            adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());

                rewardedAd = ad;
                RegisterEventHandlers(rewardedAd);
            }
        );
    }

    public void ShowRewardedAd(RewardAdCompletedCallback callback)
    {
        onRewardAdCompletedCallback = callback;
        const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(
                (Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                    onRewardAdCompletedCallback?.Invoke(reward);
                    onRewardAdCompletedCallback = null;
                    // RessourcesManager.Instance.StartWaitForReloading();
                }
            );
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
            // onRewardAdCompletedCallback?.Invoke(null);
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.LogError(
                String.Format("Rewarded ad paid {0} {1}.", adValue.Value, adValue.CurrencyCode)
            );
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
            //HERE WILL GO THE REWARDED VIDEO CALLBACK!!!!
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
            RessourcesManager.Instance.StartWaitForReloading();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError(
                "Rewarded ad failed to open full screen content " + "with error : " + error
            );
            LoadRewardedAd();
            RessourcesManager.Instance.StartWaitForReloading();
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != Loader.Scene.Lobby.ToString())
        {
            if (bannerView != null)
            {
                Debug.Log("Destroying banner view.");
                bannerView.Destroy();
                bannerView = null;
            }
        }
        else
        {
            LoadShowBanner();
        }
    }
}

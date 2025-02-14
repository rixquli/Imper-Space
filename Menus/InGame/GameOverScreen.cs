using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private Button backToMainMenuBtn;
    [SerializeField] private Button watchAdAndContinueBtn;
    [SerializeField] private Button payAndContinueBtn;

    private void Awake()
    {
        backToMainMenuBtn.onClick.AddListener(ShowResumGame);
        watchAdAndContinueBtn.onClick.AddListener(WatchAdAndContinue);
        payAndContinueBtn.onClick.AddListener(PayAndContinueBtn);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void ShowBackToMenuOnly()
    {
        watchAdAndContinueBtn.gameObject.SetActive(false);
        payAndContinueBtn.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void ShowResumGame()
    {
        UIController.Instance.HideGameOverScreen();
        UIController.Instance.ShowGameResumScreen();
    }
    private void WatchAdAndContinue()
    {
        //TODO: watch ad and continue
        if (AdmobManager.Instance == null)
        {
            Debug.LogError("AdmobManager is null");
            return;
        }
        AdmobManager.Instance.ShowRewardedAd((Reward reward) =>
        {
            UIController.Instance.ContinueGame();
        });
        // AdsManager.Instance.rewardedAds.ShowRewardedAd((UnityAdsShowCompletionState showCompletionState) =>
        // {
        //     Debug.Log("Callback invoked. Ad state: " + showCompletionState);
        //     UIController.Instance.ContinueGame();
        // });
    }
    private void PayAndContinueBtn()
    {
        //TODO: pay golds and continue
        UIController.Instance.ContinueGame();
    }
}

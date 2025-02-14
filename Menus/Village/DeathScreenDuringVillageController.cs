using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenDuringVillageController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button watchAdButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        watchAdButton.onClick.AddListener(OnWatchAdButtonClicked);
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    private void OnContinueButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnWatchAdButtonClicked()
    {
        AdmobManager.Instance.ShowRewardedAd(OnRewardedAdFinished);
    }

    private void OnRewardedAdFinished(Reward reward)
    {
        UIController.Instance.ContinueGame();
        gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private UDictionary<SoundMixers, Slider> sliders = new();

    [SerializeField] private Button closeButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        gameObject.SetActive(false);

        closeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
        if (continueButton != null)
            continueButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                UIController.Instance.ShowGameResumScreen();
            });
    }

    private void Start()
    {
        SoundMixerManager.Instance.SetNewSlider(sliders);
    }

    public void ShowSettingsMenu()
    {
        gameObject.SetActive(true);
    }

}

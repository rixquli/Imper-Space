using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StayOrExitController : MonoBehaviour
{
    [SerializeField] private Button stayButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TextMeshProUGUI goldText;


    private void Awake()
    {
        gameObject.SetActive(false);

        stayButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        });
        exitButton.onClick.AddListener(() =>
        {
            UIController.Instance.ShowGameResumScreen();
        });
    }

    public void ShowStayOrExitMenu()
    {
        Time.timeScale = 0;
        goldText.text = $"{Inventory.Instance.goldAmount} GOLD";
        gameObject.SetActive(true);
    }
}

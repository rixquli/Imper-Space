using UnityEngine;
using UnityEngine.UI;

public class CompanionShopController : MonoBehaviour
{
    [SerializeField]
    private Button closeButton;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);

        HidePanel();
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void HidePanel()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
}

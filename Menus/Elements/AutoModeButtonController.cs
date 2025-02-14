using UnityEngine;
using UnityEngine.UI;

public class AutoModeButtonController : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Image image;

    [SerializeField] Sprite sprite;

    // couleurs variables du sprite quand actif et inactif
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    private bool previousIsAutoModeActive = false;

    private void Awake()
    {
        image.sprite = sprite;
        image.color = inactiveColor;
        button.onClick.AddListener(() =>
        {
            StaticDataManager.IsAutoModeActive = !StaticDataManager.IsAutoModeActive;
            StaticDataManager.autoMode = AutoMode.TargetEnemy;
        });

    }

    private void Update()
    {
        if (previousIsAutoModeActive != StaticDataManager.IsAutoModeActive)
        {
            previousIsAutoModeActive = StaticDataManager.IsAutoModeActive;
            UpdateButton();
        }
    }

    private void UpdateButton()
    {
        if (
            GlobalAttackVillageManager.Instance != null
            || GlobalDefenseVillageManager.Instance != null
        )
        {
            if (StaticDataManager.IsAutoModeActive)
            {
                UIController.Instance.HidePlayerUI();
                UIController.Instance.ShowBigAutoModeButton();
            }
            else
            {
                UIController.Instance.ShowPlayerUI();
                UIController.Instance.ShowSmallAutoModeButton();
            }
        }
        image.color = StaticDataManager.IsAutoModeActive
        ? activeColor
        : inactiveColor;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class VillageAttackGameOverScreen : MonoBehaviour
{
    [SerializeField] private Button backToVilage;

    private void Awake()
    {
        backToVilage.onClick.AddListener(BackToVilage);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void BackToVilage()
    {
        UIController.Instance.HideVillageAttackGameOver();
        UIController.Instance.ShowVillageAttackGameResumScreen();
    }
}

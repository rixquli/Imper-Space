using UnityEngine;
using UnityEngine.UI;

public class VillageAttackVictoryScreen : MonoBehaviour
{
    [SerializeField] private Button resumGameButton;

    private void Awake()
    {
        resumGameButton.onClick.AddListener(ResumGame);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void ResumGame()
    {

        UIController.Instance.HideVillageAttackVictory();
        UIController.Instance.ShowVillageAttackGameResumScreen();
    }
}

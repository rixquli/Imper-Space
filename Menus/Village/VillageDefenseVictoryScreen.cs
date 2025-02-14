using UnityEngine;
using UnityEngine.UI;

public class VillageDefenseVictoryScreen : MonoBehaviour
{
    [SerializeField] private Button continueVillageAttack;

    private void Awake()
    {
        continueVillageAttack.onClick.AddListener(ContinueVillageAttack);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void ContinueVillageAttack()
    {
        Time.timeScale = 1;
        Loader.Load(Loader.Scene.Attack_Village);
    }
}

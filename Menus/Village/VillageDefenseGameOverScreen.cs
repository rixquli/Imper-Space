using UnityEngine;
using UnityEngine.UI;

public class VillageDefenseGameOverScreen : MonoBehaviour
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
        Loader.Load(Loader.Scene.Player_Village);
    }
}

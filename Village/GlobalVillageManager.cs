using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalVillageManager : MonoBehaviour
{
    public static GlobalVillageManager Instance;

    private void Awake()
    {
        if (RessourcesManager.Instance == null)
            SceneManager.LoadScene("ManagersScene", LoadSceneMode.Additive);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BackToMenu()
    {
        if (VillageMovingManager.Instance.isMoving) return;
        Loader.Load(Loader.Scene.Lobby);
    }
}

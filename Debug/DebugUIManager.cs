using UnityEngine;
using UnityEngine.UI;

public class DebugUIManager : MonoBehaviour
{
    public Button serverBtn;
    public Button hostBtn;
    public Button clientBtn;

    void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            // NetworkManager.Singleton.StartServer();
            StaticDataManager.StartGame();
        });
        hostBtn.onClick.AddListener(() =>
        {
            // NetworkManager.Singleton.StartHost();
            StaticDataManager.StartGame();
        });
        clientBtn.onClick.AddListener(() =>
        {
            // NetworkManager.Singleton.StartClient();
            StaticDataManager.StartGame();
        });
    }
}

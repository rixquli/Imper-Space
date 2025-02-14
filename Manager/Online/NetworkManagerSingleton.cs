using UnityEngine;

public class NetworkManagerSingleton : MonoBehaviour
{
    private static NetworkManagerSingleton _instance;

    public static NetworkManagerSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<NetworkManagerSingleton>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<NetworkManagerSingleton>();
                    singletonObject.name = typeof(NetworkManagerSingleton).ToString() + " (Singleton)";
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Assure-toi que NetworkManager persiste également

    }
}

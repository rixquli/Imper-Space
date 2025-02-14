using System.Threading.Tasks;
using Firebase.Database;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class FBDataBase : MonoBehaviour
{
    public GameData gameData;
    public string userId;
    DatabaseReference dbRef;

    private static FBDataBase _instance;

    public static FBDataBase Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject();
                _instance = singletonObject.AddComponent<FBDataBase>();
                singletonObject.name = typeof(FBDataBase).ToString() + " (Singleton)";
                DontDestroyOnLoad(singletonObject);
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
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignedIn;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void OnSignedIn()
    {
        userId = AuthenticationService.Instance.PlayerId;
        Debug.Log("User ID: " + userId);
    }

    public async Task<GiftCodeInDBReward> TryGiftCode(string code)
    {
        try
        {
            DataSnapshot snapshot = await dbRef.Child("codes").GetValueAsync();
            if (!snapshot.Exists) return null;

            string jsonStr = snapshot.GetRawJsonValue();
            // Wrap array in object structure
            string wrappedJson = "{\"codes\":" + jsonStr + "}";

            Debug.Log(jsonStr); // est vide
            var json = JsonUtility.FromJson<GiftCodeWrapper>(wrappedJson);
            if (json == null)
            {
                Debug.LogError("Failed to parse json");
                return null;
            }
            foreach (var item in json.codes)
            {
                if (item.code == code)
                {
                    if (item.expire == "0")
                    {
                        if (item.reward.type == "Gold")
                        {
                            RessourcesManager.Instance.AddGold(int.Parse(item.reward.amount));
                        }
                        else if (item.reward.type == "Gem")
                        {
                            RessourcesManager.Instance.AddGems(int.Parse(item.reward.amount));
                        }
                        return item.reward;
                    }
                    else
                    {
                        Debug.LogError("Code is expired");
                        return null;
                    }
                }
            }
        }
        catch (System.Exception error)
        {
            Debug.LogError("Failed to read data" + error);
        }
        return null;
    }

    public void SaveData(string gameData)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is null or empty");
            return;
        }
        dbRef.Child("users").Child(userId).SetRawJsonValueAsync(gameData);
    }

    public string LoadData()
    {
        if (userId == null || userId == "")
        {
            Debug.LogError("User ID is null or empty");
            return "";
        }
        Debug.LogError("User ID is NOTTT null or empty");
        string json = "";
        try
        {
            dbRef.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to read data");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        json = snapshot.GetRawJsonValue();
                    }
                }
            });
        }
        catch (System.Exception error)
        {
            Debug.LogError("Failed to read data" + error);
        }
        return json;
    }


}

[System.Serializable]
public class GiftCodeWrapper
{
    public GiftCodeInDb[] codes;
}

[System.Serializable]
public class GiftCodeInDb
{
    public string code;
    public string expire;
    public string id;
    public GiftCodeInDBReward reward;
}

[System.Serializable]
public class GiftCodeInDBReward
{
    public string amount;
    public string type;
}
using UnityEngine;

public class FBS : MonoBehaviour
{

    public void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
            Subscribe();
        });
    }

    void Subscribe()
    {
        Firebase.Messaging.FirebaseMessaging.SubscribeAsync("/topics/ImperSpace");
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }
}
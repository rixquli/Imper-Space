using System.Collections;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

public class AndroidNotificationsManager : MonoBehaviour
{
    private void Start()
    {
        RequestUserPermission();
        RegisterNotificationChannel();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            AndroidNotificationCenter.CancelAllNotifications();
            SendNotification("Daily Reward", "Don't forget to claim your daily reward!", 24);
            SendNotification(
                "Base Abandoned!",
                "Commander, your space station needs you! Return now to claim your rewards and restore order before it's too late!",
                48
            );
            SendNotification(
                "Your Station is Falling Apart!",
                "Commander, it's been 7 days! Your crew is struggling, and your base is in critical condition. Return now to save your station and claim a special reward!",
                24 * 7
            );
        }
    }

    public void RequestUserPermission()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }

    // register a notification channel
    public void RegisterNotificationChannel()
    {
        AndroidNotificationChannel channel = new AndroidNotificationChannel()
        {
            Id = "main",
            Name = "Main Chanel",
            Importance = Importance.Default,
            Description = "Main Notification Chanel",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    // set up notification template
    public void SendNotification(string title, string text, int delay)
    {
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = text;
        notification.FireTime = System.DateTime.Now.AddHours(delay);
        notification.SmallIcon = "icon_0";
        notification.LargeIcon = "icon_1";
        AndroidNotificationCenter.SendNotification(notification, "main");
    }
}

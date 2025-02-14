using System.Collections;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;

public class InAppUpdateManager : MonoBehaviour
{
    public static InAppUpdateManager Instance { get; private set; }
    private AppUpdateManager appUpdateManager;
    private bool isCheckingForUpdate = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
#if !UNITY_EDITOR
        InitializeAppUpdateManager();
#endif
    }

    private void InitializeAppUpdateManager()
    {
        try
        {
            appUpdateManager = new AppUpdateManager();
            CheckForUpdates();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize AppUpdateManager: {e.Message}");
        }
    }

    public void CheckForUpdates()
    {
#if !UNITY_EDITOR
        if (!isCheckingForUpdate && appUpdateManager != null)
        {
            StartCoroutine(CheckForUpdate());
        }
#endif
    }

    private IEnumerator CheckForUpdate()
    {
        if (isCheckingForUpdate || appUpdateManager == null)
        {
            yield break;
        }

        isCheckingForUpdate = true;

        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
            appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfo = appUpdateInfoOperation.GetResult();

            if (appUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                Debug.Log("Update Available");
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                StartCoroutine(StartImmediateUpdate(appUpdateInfo, appUpdateOptions));
            }
            else
            {
                Debug.Log("No Update Available");
            }
        }
        else
        {
            Debug.LogError($"Update check failed: {appUpdateInfoOperation.Error}");
        }

        isCheckingForUpdate = false;
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo, AppUpdateOptions appUpdateOptions)
    {
        if (appUpdateManager == null)
        {
            Debug.LogError("AppUpdateManager is null");
            yield break;
        }

        var startUpdateOperation = appUpdateManager.StartUpdate(appUpdateInfo, appUpdateOptions);
        yield return startUpdateOperation;

        if (!startUpdateOperation.IsDone)
        {
            Debug.LogError($"Update failed: {startUpdateOperation.Error}");
        }
        else
        {
            Debug.Log("Update completed successfully.");
        }
    }

    void OnDestroy()
    {
        if (appUpdateManager != null)
        {
            try
            {
                appUpdateManager = null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error disposing AppUpdateManager: {e.Message}");
            }
        }
    }
}

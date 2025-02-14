using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideoManager : MonoBehaviour
{
    private AsyncOperation loadLobbyScene;
    private AsyncOperation loadManagerScene;

    void Start()
    {
        var videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += EndReached;

        // Load the LittleManager scene first
        loadManagerScene = SceneManager.LoadSceneAsync(Loader.Scene.ManagersScene.ToString(), LoadSceneMode.Additive);
        loadManagerScene.completed += OnLittleManagerSceneLoaded;
    }

    private void OnLittleManagerSceneLoaded(AsyncOperation obj)
    {
        // Load the Lobby scene after LittleManager scene is loaded
        loadLobbyScene = SceneManager.LoadSceneAsync(Loader.Scene.Lobby.ToString());
        loadLobbyScene.allowSceneActivation = false;
    }

    void EndReached(VideoPlayer vp)
    {
        // Activate the Lobby scene when the video ends
        if (loadLobbyScene != null)
        {
            loadLobbyScene.allowSceneActivation = true;
        }
        else
        {
            Debug.LogError("loadLobbyScene is null");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events to avoid potential memory leaks
        var videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= EndReached;
        }
    }
}
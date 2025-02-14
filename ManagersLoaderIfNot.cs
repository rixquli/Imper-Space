using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class ManagersLoaderIfNot : MonoBehaviour
{
    private void Awake()
    {
        if (RessourcesManager.Instance == null)
        {
            SceneManager.LoadScene(Loader.Scene.ManagersScene.ToString(), LoadSceneMode.Additive);
        }
    }
}

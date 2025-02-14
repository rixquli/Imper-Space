using UnityEngine;
using UnityEngine.UI;

public class WorldCanvasManager : MonoBehaviour
{
  private static WorldCanvasManager instance;
  private Canvas _worldCanvas;
  private Canvas worldCanvas
  {
    get
    {
      if (_worldCanvas == null)
      {
        _worldCanvas = CreateWorldCanvas();
      }
      return _worldCanvas;
    }
    set
    {
      _worldCanvas = value;
    }
  }

  public static WorldCanvasManager Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindFirstObjectByType<WorldCanvasManager>();
        if (instance == null)
        {
          GameObject singletonObject = new GameObject();
          instance = singletonObject.AddComponent<WorldCanvasManager>();
          singletonObject.name = "WorldCanvasManager (Singleton)";
          DontDestroyOnLoad(singletonObject);
        }
      }
      return instance;
    }
  }

  private void Awake()
  {
    if (worldCanvas == null)
    {
      worldCanvas = CreateWorldCanvas();
    }
  }

  private Canvas CreateWorldCanvas()
  {
    GameObject worldCanvasObject = new GameObject();
    worldCanvasObject.name = "WorldCanvas";
    worldCanvas = worldCanvasObject.AddComponent<Canvas>();
    CanvasScaler canvasScaler = worldCanvasObject.AddComponent<CanvasScaler>();
    canvasScaler.dynamicPixelsPerUnit = 10;
    worldCanvasObject.transform.position = Vector3.zero;
    worldCanvasObject.transform.rotation = Quaternion.identity;
    worldCanvasObject.transform.localScale = Vector3.one;
    worldCanvasObject.layer = LayerMask.NameToLayer("UI");

    // met le mode de rendu en mode "World"
    worldCanvas.renderMode = RenderMode.WorldSpace;
    worldCanvas.worldCamera = Camera.main;
    return worldCanvas;
  }

  public void AddUIElementToCanvas(GameObject uiElement)
  {
    if (uiElement == null)
      return;
    uiElement.transform.SetParent(worldCanvas.transform, false);
  }
}

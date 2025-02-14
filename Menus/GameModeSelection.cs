using UnityEngine;
using UnityEngine.UI;

public class GameModeSelection : MonoBehaviour
{
    [SerializeField] private Button startInfinityMode;

    private void Awake()
    {
        startInfinityMode.onClick.AddListener(StartInfinityMode);
    }

    private void StartInfinityMode()
    {
        StaticDataManager.gameMode = GameMode.InfinityMode;
        StaticDataManager.IsSinglePlayer = true;
        Loader.Load(Loader.Scene.Game_Infinity_Mode);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button singlePlayerButton;

    private void Awake()
    {

    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class TutoPanelCard : MonoBehaviour
{
    [SerializeField] private GameObject buttonObject;
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private GameObject textParent;
    public Button button;

    private void Awake()
    {
        if (button == null)
        {
            Debug.LogError("Button is not assigned in the inspector!");
            return;
        }

        // gameObject.SetActive(false);
        button.onClick.AddListener(ButtonHandle);
    }

    private void ButtonHandle()
    {
        TutorialManager.Instance.HideTutorialBackground();
        gameObject.SetActive(false);
    }

    public void Deactivate()
    {
        if (gameObject != null)
            gameObject.SetActive(false);
    }

    public void Activate()
    {
        if (arrowObject != null)
        {
            arrowObject.SetActive(true); // Show arrow if assigned
        }

        if (textParent != null)
        {
            textParent.SetActive(true); // Show text if assigned
        }

        gameObject.SetActive(true);
        TutorialManager.Instance.ShowTutorialBackground();
    }
}

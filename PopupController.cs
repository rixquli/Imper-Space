using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    [SerializeField]
    private Button closeBtn;

    [SerializeField]
    private List<TextMeshProUGUI> textsToChange;

    private void Awake()
    {
        closeBtn.onClick.AddListener(HidePopup);
    }

    public void ShowPopup()
    {
        gameObject.SetActive(true);
    }

    public void HidePopup()
    {
        gameObject.SetActive(false);
    }

    public void SetTexts(int id, string texts)
    {
        if (id < 0 || id >= textsToChange.Count)
        {
            Debug.LogError("Invalid ID");
            return;
        }
        textsToChange[id].text = texts;
    }
}

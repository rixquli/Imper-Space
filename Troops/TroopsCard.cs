using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TroopsCard : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textAmount;
    private float onSelectedScale = 1.1f;
    private float onUnselectedScale = 1f;
    public TroopsToPlace data;

    public void Init(TroopsToPlace data, Sprite sprite, string name, int amount)
    {
        if (data != null) this.data = data;
        if (image != null) image.sprite = sprite;
        if (textName != null) textName.text = name;
        if (textAmount != null) SetAmount(amount.ToString());
    }

    public void SetAmount(string newText)
    {
        if (textAmount != null) textAmount.text = $"x{newText}";
    }

    public void SetText(string newText)
    {
        if (textName != null) textName.text = newText;
    }

    public void OnSelected()
    {
        transform.localScale = new Vector3(onSelectedScale, onSelectedScale, onSelectedScale);
    }
    public void OnUnselected()
    {
        transform.localScale = new Vector3(onUnselectedScale, onUnselectedScale, onUnselectedScale);
    }
}

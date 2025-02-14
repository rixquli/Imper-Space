using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class MaintainAspectRatio : MonoBehaviour
{
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Image image = GetComponent<Image>();

        // Assurez-vous que l'image a été définie
        if (image.sprite != null)
        {
            float width = image.sprite.rect.width;
            float height = image.sprite.rect.height;
            float aspectRatio = width / height;

            // Appliquer le ratio à RectTransform
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.y * aspectRatio, rectTransform.sizeDelta.y);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class RawImageScroller : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private float x, y;
    public bool canScroll = true;


    void Update()
    {
        if (canScroll) image.uvRect = new Rect(image.uvRect.position + new Vector2(x, y) * Time.deltaTime, image.uvRect.size);
    }
}

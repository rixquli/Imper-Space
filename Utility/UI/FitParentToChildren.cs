using UnityEngine;

public class FitParentToChildren : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformToCopy;
    public Mode mode = Mode.Horizontal;
    public float spaceBetween = 50f;
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;

    public enum Mode
    {
        Horizontal,
        Vertical
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (mode == Mode.Horizontal)
        {
            AdjustSizeHorizontal();

        }
        else if (mode == Mode.Vertical)
        {
            AdjustSizeVertical();
        }
    }

    private void AdjustSizeVertical()
    {
        if (rectTransformToCopy != null)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransformToCopy.rect.height);
            return;
        }
        else
        {
            float minHeight = parentRectTransform.rect.height;
            float contentHeight = CalculateContentHeight();

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(minHeight, contentHeight));
        }
    }

    private float CalculateContentHeight()
    {
        float totalHeight = 0f;

        for (int i = 0; i < rectTransform.childCount; i++)
        {
            RectTransform child = rectTransform.GetChild(i) as RectTransform;
            if (child != null && child.gameObject.activeSelf && !IsIgnoredElement(child))
            {
                totalHeight += child.rect.height + spaceBetween;
            }
        }

        return totalHeight + 50;
    }

    private void AdjustSizeHorizontal()
    {
        if (rectTransformToCopy != null)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransformToCopy.rect.width);
            return;
        }
        else
        {
            float minWidth = parentRectTransform.rect.width;
            float contentWidth = CalculateContentWidth();

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(minWidth, contentWidth));
        }
    }

    private float CalculateContentWidth()
    {
        float totalWidth = 0f;

        for (int i = 0; i < rectTransform.childCount; i++)
        {
            RectTransform child = rectTransform.GetChild(i) as RectTransform;
            if (child != null && child.gameObject.activeSelf && !IsIgnoredElement(child))
            {
                totalWidth += child.rect.width + spaceBetween;
            }
        }

        return totalWidth + 50;
    }

    private bool IsIgnoredElement(Transform child)
    {
        return child.GetComponent<IgnoreFitParentToChildren>() != null;
    }
}

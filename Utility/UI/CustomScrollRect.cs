using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScrollRect : ScrollRect
{
    public bool blockScrollWheel = false;

    // Override the OnScroll method to block scroll wheel input if necessary
    public override void OnScroll(PointerEventData data)
    {
        if (!blockScrollWheel)
        {
            base.OnScroll(data);
        }
    }

    // Method to scroll to a specific child element within the ScrollRect content
    public void ScrollToTargetChild(Transform targetChild)
    {
        // Ensure the ScrollRect content and the target child are not null
        if (content == null || targetChild == null)
        {
            Debug.LogError("ScrollRect content or target child is not assigned.");
            return;
        }

        content.localPosition = -targetChild.localPosition;
    }
}

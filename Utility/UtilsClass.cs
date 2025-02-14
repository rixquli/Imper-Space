using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class UtilsClass
{
    // Is Mouse over a UI Element? Used for ignoring World clicks through UI
    public static bool IsPointerOverUI()
    {
        // Get the current pointer device (e.g., mouse or touch)
        Pointer currentPointer = Pointer.current;

        if (currentPointer != null)
        {
            // Get the current pointer position from the Input System
            Vector2 pointerPosition = currentPointer.position.ReadValue();

            // Create a new PointerEventData for the current event system
            PointerEventData pe = new PointerEventData(EventSystem.current)
            {
                position = pointerPosition,
            };

            // Perform a raycast to see if we hit any UI elements
            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, hits);

            // Return true if any UI elements were hit, false otherwise
            return hits.Count > 0;
        }

        return false;
    }

    // Generate random normalized direction
    public static Vector3 GetRandomDir()
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;
    }

    public static Vector3 GetVectorFromAngle(int angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
            n += 360;

        return n;
    }

    public static int GetAngleFromVector(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
            n += 360;
        int angle = Mathf.RoundToInt(n);

        return angle;
    }

    public static Vector3 ApplyRotationToVector(Vector3 vec, Vector3 vecRotation)
    {
        return ApplyRotationToVector(vec, GetAngleFromVectorFloat(vecRotation));
    }

    public static Vector3 ApplyRotationToVector(Vector3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

    public static void ActivateCollision(GameObject parent, string[] layerNames)
    {
        foreach (var layerName in layerNames)
        {
            ActivateCollision(parent, layerName);
        }
    }

    public static void ActivateCollision(GameObject parent, string layerName)
    {
        Collider2D collider = FindChildCollidersOnLayer(parent, layerName);

        if (collider != null)
        {
            collider.isTrigger = false;
        }
    }

    public static void DesactivateCollision(GameObject parent, string[] layerNames)
    {
        foreach (var layerName in layerNames)
        {
            DesactivateCollision(parent, layerName);
        }
    }

    public static void DesactivateCollision(GameObject parent, string layerName)
    {
        Collider2D collider = FindChildCollidersOnLayer(parent, layerName);

        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    public static Collider2D FindChildCollidersOnLayer(GameObject parent, string[] layerName)
    {
        foreach (var layer in layerName)
        {
            Collider2D collider = FindChildCollidersOnLayer(parent, layer);
            if (collider != null)
                return collider;
        }
        return null;
    }

    public static Collider2D FindChildCollidersOnLayer(GameObject parent, string layerName)
    {
        // Get the layer index from the layer name
        int layer = LayerMask.NameToLayer(layerName);

        if (parent.gameObject.layer == layer)
        {
            // Get the collider component from the child
            Collider2D collider = parent.GetComponent<Collider2D>();

            // If the child has a collider, print its name
            if (collider != null)
            {
                return collider;
            }
        }

        // Iterate through each child of the parent
        foreach (Transform child in parent.transform)
        {
            // Check if the child's layer matches the specified layer
            if (child.gameObject.layer == layer)
            {
                // Get the collider component from the child
                Collider2D childCollider = child.GetComponent<Collider2D>();

                // If the child has a collider, print its name
                if (childCollider != null)
                {
                    return childCollider;
                }
            }
        }

        return null;
    }

    public static void SwitchLayerForCollision(
        GameObject parent,
        string[] layerNames,
        string newLayerName
    )
    {
        foreach (var layerName in layerNames)
        {
            Collider2D child = FindChildCollidersOnLayer(parent, layerNames);
            if (child != null && child.gameObject.layer == LayerMask.NameToLayer(layerName))
            {
                child.gameObject.layer = LayerMask.NameToLayer(newLayerName);
                return;
            }
        }
    }


    #region Text Animation
    public static void SetTextWithAnimation(TextMeshProUGUI textComponent, string newText, float duration = 0.5f, float punchScale = 1.2f)
    {
        if (textComponent == null) return;

        // Set new text
        textComponent.text = newText;

        // Reset scale before animation
        textComponent.transform.localScale = Vector3.one;

        // Animate scale with punch effect
        textComponent.transform
            .DOPunchScale(Vector3.one * (punchScale - 1f), duration, 1, 0.5f)
            .OnComplete(() => textComponent.transform.localScale = Vector3.one);
    }
    #endregion
}

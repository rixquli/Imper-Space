/*
    ------------------- Code Monkey -------------------

    Thank you for downloading the Code Monkey Utilities
    I hope you find them useful in your projects
    If you have any questions use the contact form
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
 * Various assorted utilities functions
 * */
public static class UtilsClass
{
    private static readonly Vector3 Vector3zero = Vector3.zero;
    private static readonly Vector3 Vector3one = Vector3.one;
    private static readonly Vector3 Vector3yDown = new Vector3(0, -1);

    public const int sortingOrderDefault = 5000;

    // Get Sorting order to set SpriteRenderer sortingOrder, higher position = lower sortingOrder
    public static int GetSortingOrder(
        Vector3 position,
        int offset,
        int baseSortingOrder = sortingOrderDefault
    )
    {
        return (int)(baseSortingOrder - position.y) + offset;
    }

    // Get Main Canvas Transform
    private static Transform cachedCanvasTransform;

    public static Transform GetCanvasTransform()
    {
        if (cachedCanvasTransform == null)
        {
            Canvas canvas = MonoBehaviour.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                cachedCanvasTransform = canvas.transform;
            }
        }
        return cachedCanvasTransform;
    }

    // Get Default Unity Font, used in text objects if no font given
    public static Font GetDefaultFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    // Create a Sprite in the World, no parent
    public static GameObject CreateWorldSprite(
        string name,
        Sprite sprite,
        Vector3 position,
        Vector3 localScale,
        int sortingOrder,
        Color color
    )
    {
        return CreateWorldSprite(null, name, sprite, position, localScale, sortingOrder, color);
    }

    // Create a Sprite in the World
    public static GameObject CreateWorldSprite(
        Transform parent,
        string name,
        Sprite sprite,
        Vector3 localPosition,
        Vector3 localScale,
        int sortingOrder,
        Color color
    )
    {
        GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.localScale = localScale;
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.color = color;
        return gameObject;
    }

    // Create a Sprite in the World with Button_Sprite, no parent
    public static Button_Sprite CreateWorldSpriteButton(
        string name,
        Sprite sprite,
        Vector3 localPosition,
        Vector3 localScale,
        int sortingOrder,
        Color color
    )
    {
        return CreateWorldSpriteButton(
            null,
            name,
            sprite,
            localPosition,
            localScale,
            sortingOrder,
            color
        );
    }

    // Create a Sprite in the World with Button_Sprite
    public static Button_Sprite CreateWorldSpriteButton(
        Transform parent,
        string name,
        Sprite sprite,
        Vector3 localPosition,
        Vector3 localScale,
        int sortingOrder,
        Color color
    )
    {
        GameObject gameObject = CreateWorldSprite(
            parent,
            name,
            sprite,
            localPosition,
            localScale,
            sortingOrder,
            color
        );
        gameObject.AddComponent<BoxCollider2D>();
        Button_Sprite buttonSprite = gameObject.AddComponent<Button_Sprite>();
        return buttonSprite;
    }

    // Creates a Text Mesh in the World and constantly updates it
    public static FunctionUpdater CreateWorldTextUpdater(
        Func<string> GetTextFunc,
        Vector3 localPosition,
        Transform parent = null
    )
    {
        TextMesh textMesh = CreateWorldText(GetTextFunc(), parent, localPosition);
        return FunctionUpdater.Create(
            () =>
            {
                textMesh.text = GetTextFunc();
                return false;
            },
            "WorldTextUpdater"
        );
    }

    // Create Text in the World
    public static TextMesh CreateWorldText(
        string text,
        Transform parent = null,
        Vector3 localPosition = default(Vector3),
        int fontSize = 40,
        Color? color = null,
        TextAnchor textAnchor = TextAnchor.UpperLeft,
        TextAlignment textAlignment = TextAlignment.Left,
        int sortingOrder = sortingOrderDefault
    )
    {
        if (color == null)
            color = Color.white;
        return CreateWorldText(
            parent,
            text,
            localPosition,
            fontSize,
            (Color)color,
            textAnchor,
            textAlignment,
            sortingOrder
        );
    }

    // Create Text in the World
    public static TextMesh CreateWorldText(
        Transform parent,
        string text,
        Vector3 localPosition,
        int fontSize,
        Color color,
        TextAnchor textAnchor,
        TextAlignment textAlignment,
        int sortingOrder
    )
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    // Create a Text Popup in the World, no parent
    public static void CreateWorldTextPopup(string text, Vector3 localPosition)
    {
        CreateWorldTextPopup(
            null,
            text,
            localPosition,
            40,
            Color.white,
            localPosition + new Vector3(0, 20),
            1f
        );
    }

    // Create a Text Popup in the World
    public static void CreateWorldTextPopup(
        Transform parent,
        string text,
        Vector3 localPosition,
        int fontSize,
        Color color,
        Vector3 finalPopupPosition,
        float popupTime
    )
    {
        TextMesh textMesh = CreateWorldText(
            parent,
            text,
            localPosition,
            fontSize,
            color,
            TextAnchor.LowerLeft,
            TextAlignment.Left,
            sortingOrderDefault
        );
        Transform transform = textMesh.transform;
        Vector3 moveAmount = (finalPopupPosition - localPosition) / popupTime;
        FunctionUpdater.Create(
            delegate ()
            {
                transform.position += moveAmount * Time.deltaTime;
                popupTime -= Time.deltaTime;
                if (popupTime <= 0f)
                {
                    UnityEngine.Object.Destroy(transform.gameObject);
                    return true;
                }
                else
                {
                    return false;
                }
            },
            "WorldTextPopup"
        );
    }

    // Create Text Updater in UI
    public static FunctionUpdater CreateUITextUpdater(
        Func<string> GetTextFunc,
        Vector2 anchoredPosition
    )
    {
        Text text = DrawTextUI(GetTextFunc(), anchoredPosition, 20, GetDefaultFont());
        return FunctionUpdater.Create(
            () =>
            {
                text.text = GetTextFunc();
                return false;
            },
            "UITextUpdater"
        );
    }

    // Draw a UI Sprite
    public static RectTransform DrawSprite(
        Color color,
        Transform parent,
        Vector2 pos,
        Vector2 size,
        string name = null
    )
    {
        RectTransform rectTransform = DrawSprite(null, color, parent, pos, size, name);
        return rectTransform;
    }

    // Draw a UI Sprite
    public static RectTransform DrawSprite(
        Sprite sprite,
        Transform parent,
        Vector2 pos,
        Vector2 size,
        string name = null
    )
    {
        RectTransform rectTransform = DrawSprite(sprite, Color.white, parent, pos, size, name);
        return rectTransform;
    }

    // Draw a UI Sprite
    public static RectTransform DrawSprite(
        Sprite sprite,
        Color color,
        Transform parent,
        Vector2 pos,
        Vector2 size,
        string name = null
    )
    {
        // Setup icon
        if (name == null || name == "")
            name = "Sprite";
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform goRectTransform = go.GetComponent<RectTransform>();
        goRectTransform.SetParent(parent, false);
        goRectTransform.sizeDelta = size;
        goRectTransform.anchoredPosition = pos;

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;

        return goRectTransform;
    }

    public static Text DrawTextUI(
        string textString,
        Vector2 anchoredPosition,
        int fontSize,
        Font font
    )
    {
        return DrawTextUI(textString, GetCanvasTransform(), anchoredPosition, fontSize, font);
    }

    public static Text DrawTextUI(
        string textString,
        Transform parent,
        Vector2 anchoredPosition,
        int fontSize,
        Font font
    )
    {
        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(parent, false);
        Transform textGoTrans = textGo.transform;
        textGoTrans.SetParent(parent, false);
        textGoTrans.localPosition = Vector3zero;
        textGoTrans.localScale = Vector3one;

        RectTransform textGoRectTransform = textGo.GetComponent<RectTransform>();
        textGoRectTransform.sizeDelta = new Vector2(0, 0);
        textGoRectTransform.anchoredPosition = anchoredPosition;

        Text text = textGo.GetComponent<Text>();
        text.text = textString;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.alignment = TextAnchor.MiddleLeft;
        if (font == null)
            font = GetDefaultFont();
        text.font = font;
        text.fontSize = fontSize;

        return text;
    }

    // Parse a float, return default if failed
    public static float Parse_Float(string txt, float _default)
    {
        float f;
        if (!float.TryParse(txt, out f))
        {
            f = _default;
        }
        return f;
    }

    // Parse a int, return default if failed
    public static int Parse_Int(string txt, int _default)
    {
        int i;
        if (!int.TryParse(txt, out i))
        {
            i = _default;
        }
        return i;
    }

    public static int Parse_Int(string txt)
    {
        return Parse_Int(txt, -1);
    }

    // Get Mouse Position in World with Z = 0f
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public static Vector3 GetMouseWorldPositionWithZ()
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

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

    // Returns 00-FF, value 0->255
    public static string Dec_to_Hex(int value)
    {
        return value.ToString("X2");
    }

    // Returns 0-255
    public static int Hex_to_Dec(string hex)
    {
        return Convert.ToInt32(hex, 16);
    }

    // Returns a hex string based on a number between 0->1
    public static string Dec01_to_Hex(float value)
    {
        return Dec_to_Hex((int)Mathf.Round(value * 255f));
    }

    // Returns a float between 0->1
    public static float Hex_to_Dec01(string hex)
    {
        return Hex_to_Dec(hex) / 255f;
    }

    // Get Hex Color FF00FF
    public static string GetStringFromColor(Color color)
    {
        string red = Dec01_to_Hex(color.r);
        string green = Dec01_to_Hex(color.g);
        string blue = Dec01_to_Hex(color.b);
        return red + green + blue;
    }

    // Get Hex Color FF00FFAA
    public static string GetStringFromColorWithAlpha(Color color)
    {
        string alpha = Dec01_to_Hex(color.a);
        return GetStringFromColor(color) + alpha;
    }

    // Sets out values to Hex String 'FF'
    public static void GetStringFromColor(
        Color color,
        out string red,
        out string green,
        out string blue,
        out string alpha
    )
    {
        red = Dec01_to_Hex(color.r);
        green = Dec01_to_Hex(color.g);
        blue = Dec01_to_Hex(color.b);
        alpha = Dec01_to_Hex(color.a);
    }

    // Get Hex Color FF00FF
    public static string GetStringFromColor(float r, float g, float b)
    {
        string red = Dec01_to_Hex(r);
        string green = Dec01_to_Hex(g);
        string blue = Dec01_to_Hex(b);
        return red + green + blue;
    }

    // Get Hex Color FF00FFAA
    public static string GetStringFromColor(float r, float g, float b, float a)
    {
        string alpha = Dec01_to_Hex(a);
        return GetStringFromColor(r, g, b) + alpha;
    }

    // Get Color from Hex string FF00FFAA
    public static Color GetColorFromString(string color)
    {
        float red = Hex_to_Dec01(color.Substring(0, 2));
        float green = Hex_to_Dec01(color.Substring(2, 2));
        float blue = Hex_to_Dec01(color.Substring(4, 2));
        float alpha = 1f;
        if (color.Length >= 8)
        {
            // Color string contains alpha
            alpha = Hex_to_Dec01(color.Substring(6, 2));
        }
        return new Color(red, green, blue, alpha);
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

    public static int GetAngleFromVector180(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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

    public static FunctionUpdater CreateMouseDraggingAction(Action<Vector3> onMouseDragging)
    {
        return CreateMouseDraggingAction(0, onMouseDragging);
    }

    public static FunctionUpdater CreateMouseDraggingAction(
        int mouseButton,
        Action<Vector3> onMouseDragging
    )
    {
        bool dragging = false;
        return FunctionUpdater.Create(() =>
        {
            if (Input.GetMouseButtonDown(mouseButton))
            {
                dragging = true;
            }
            if (Input.GetMouseButtonUp(mouseButton))
            {
                dragging = false;
            }
            if (dragging)
            {
                onMouseDragging(UtilsClass.GetMouseWorldPosition());
            }
            return false;
        });
    }

    public static FunctionUpdater CreateMouseClickFromToAction(
        Action<Vector3, Vector3> onMouseClickFromTo,
        Action<Vector3, Vector3> onWaitingForToPosition
    )
    {
        return CreateMouseClickFromToAction(0, 1, onMouseClickFromTo, onWaitingForToPosition);
    }

    public static FunctionUpdater CreateMouseClickFromToAction(
        int mouseButton,
        int cancelMouseButton,
        Action<Vector3, Vector3> onMouseClickFromTo,
        Action<Vector3, Vector3> onWaitingForToPosition
    )
    {
        int state = 0;
        Vector3 from = Vector3.zero;
        return FunctionUpdater.Create(() =>
        {
            if (state == 1)
            {
                if (onWaitingForToPosition != null)
                    onWaitingForToPosition(from, UtilsClass.GetMouseWorldPosition());
            }
            if (state == 1 && Input.GetMouseButtonDown(cancelMouseButton))
            {
                // Cancel
                state = 0;
            }
            if (Input.GetMouseButtonDown(mouseButton) && !UtilsClass.IsPointerOverUI())
            {
                if (state == 0)
                {
                    state = 1;
                    from = UtilsClass.GetMouseWorldPosition();
                }
                else
                {
                    state = 0;
                    onMouseClickFromTo(from, UtilsClass.GetMouseWorldPosition());
                }
            }
            return false;
        });
    }

    public static FunctionUpdater CreateMouseClickAction(Action<Vector3> onMouseClick)
    {
        return CreateMouseClickAction(0, onMouseClick);
    }

    public static FunctionUpdater CreateMouseClickAction(
        int mouseButton,
        Action<Vector3> onMouseClick
    )
    {
        return FunctionUpdater.Create(() =>
        {
            if (Input.GetMouseButtonDown(mouseButton))
            {
                onMouseClick(GetWorldPositionFromUI());
            }
            return false;
        });
    }

    public static FunctionUpdater CreateKeyCodeAction(KeyCode keyCode, Action onKeyDown)
    {
        return FunctionUpdater.Create(() =>
        {
            // if (Input.GetKeyDown(keyCode)) {
            //     onKeyDown();
            // }
            return false;
        });
    }

    // Get UI Position from World Position
    public static Vector2 GetWorldUIPosition(
        Vector3 worldPosition,
        Transform parent,
        Camera uiCamera,
        Camera worldCamera
    )
    {
        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        Vector3 uiCameraWorldPosition = uiCamera.ScreenToWorldPoint(screenPosition);
        Vector3 localPos = parent.InverseTransformPoint(uiCameraWorldPosition);
        return new Vector2(localPos.x, localPos.y);
    }

    public static Vector3 GetWorldPositionFromUIZeroZ()
    {
        Vector3 vec = GetWorldPositionFromUI(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    // Get World Position from UI Position
    public static Vector3 GetWorldPositionFromUI()
    {
        return GetWorldPositionFromUI(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetWorldPositionFromUI(Camera worldCamera)
    {
        return GetWorldPositionFromUI(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetWorldPositionFromUI(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public static Vector3 GetWorldPositionFromUI_Perspective()
    {
        return GetWorldPositionFromUI_Perspective(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetWorldPositionFromUI_Perspective(Camera worldCamera)
    {
        return GetWorldPositionFromUI_Perspective(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetWorldPositionFromUI_Perspective(
        Vector3 screenPosition,
        Camera worldCamera
    )
    {
        Ray ray = worldCamera.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 0f));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    // Screen Shake
    public static void ShakeCamera(float intensity, float timer)
    {
        Vector3 lastCameraMovement = Vector3.zero;
        FunctionUpdater.Create(
            delegate ()
            {
                timer -= Time.unscaledDeltaTime;
                Vector3 randomMovement =
                    new Vector3(
                        UnityEngine.Random.Range(-1f, 1f),
                        UnityEngine.Random.Range(-1f, 1f)
                    ).normalized * intensity;
                Camera.main.transform.position =
                    Camera.main.transform.position - lastCameraMovement + randomMovement;
                lastCameraMovement = randomMovement;
                return timer <= 0f;
            },
            "CAMERA_SHAKE"
        );
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

    public static bool AreListsEqual<T>(List<T> list1, List<T> list2)
    {
        HashSet<T> set1 = new HashSet<T>(list1);
        HashSet<T> set2 = new HashSet<T>(list2);

        return set1.SetEquals(set2);
    }

    public static bool AreDictionariesEqual<TKey, TValue>(
        IDictionary<TKey, TValue> dict1,
        IDictionary<TKey, TValue> dict2
    )
    {
        if (dict1 == null || dict2 == null)
        {
            return dict1 == dict2; // Both must be null to be considered equal
        }

        if (dict1.Count != dict2.Count)
        {
            return false;
        }

        foreach (var kvp in dict1)
        {
            if (!dict2.ContainsKey(kvp.Key) || dict2[kvp.Key] == null || kvp.Value == null || !dict2[kvp.Key].Equals(kvp.Value))
            {
                return false;
            }
        }

        return true;
    }

    #region Part to convert UDictionary to Dictionary and vice versa with type safe
    public static UDictionary<TKey, TValue> ConvertValues<TKey, TValue>(UDictionary<TKey, SerializableData<TValue>> source)
     where TValue : SerializableScriptableObject
    {
        var result = new UDictionary<TKey, TValue>();
        foreach (var kvp in source)
        {
            result.Add(kvp.Key, kvp.Value.Data);
        }
        return result;
    }

    public static UDictionary<TKey, TValue> ConvertValues<TKey, TValue>(UDictionary<SerializableData<TKey>, TValue> source)
     where TKey : SerializableScriptableObject
    {
        var result = new UDictionary<TKey, TValue>();
        foreach (var kvp in source)
        {
            result.Add(kvp.Key.Data, kvp.Value);
        }
        return result;
    }

    public static UDictionary<TKey, SerializableData<TValue>> ConvertToSerializableDataSecondSerialized<TKey, TValue>(UDictionary<TKey, TValue> source)
     where TValue : SerializableScriptableObject
    {
        var result = new UDictionary<TKey, SerializableData<TValue>>();
        foreach (var kvp in source)
        {
            result.Add(kvp.Key, new SerializableData<TValue>(kvp.Value));
        }
        return result;
    }

    public static UDictionary<SerializableData<TKey>, TValue> ConvertToSerializableDataFirstSerialized<TKey, TValue>(UDictionary<TKey, TValue> source)
     where TKey : SerializableScriptableObject
    {
        var result = new UDictionary<SerializableData<TKey>, TValue>();
        foreach (var kvp in source)
        {
            result.Add(new SerializableData<TKey>(kvp.Key), kvp.Value);
        }
        return result;
    }
    #endregion

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

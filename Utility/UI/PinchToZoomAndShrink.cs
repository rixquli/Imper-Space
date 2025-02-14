using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PinchToZoomAndShrink : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private InputAction touchContact0; // Use the InputAction for pinch

    [SerializeField]
    private InputAction touchContact1; // Use the InputAction for pinch

    [SerializeField]
    private InputAction touchPos0; // Use the InputAction for pinch

    [SerializeField]
    private InputAction touchPos1; // Use the InputAction for pinch

    [SerializeField]
    private InputAction scrollAction; // Use the InputAction for mouse scroll

    [SerializeField]
    private CustomScrollRect scrollRect; // Reference to the custom scroll rect

    private bool _isDragging;
    private float _currentScale;
    public float minScale = 0.5f,
        maxScale = 2.0f;
    private float _temp;
    private float _scalingRate = .5f;
    private int touchCount = 0;

    private void Start()
    {
        _currentScale = transform.localScale.x;

        touchContact0.Enable();
        touchContact1.Enable();

        touchContact0.performed += _ => touchCount++;
        touchContact1.performed += _ => touchCount++;
        touchContact0.canceled += _ =>
        {
            touchCount--;
            _temp = 0;
        };
        touchContact1.canceled += _ =>
        {
            touchCount--;
            _temp = 0;
        };

        touchPos0.Enable();
        touchPos1.Enable();
        touchPos1.performed += OnPinch;

        scrollAction.Enable();
        scrollAction.performed += OnScroll;
    }

    private void OnDestroy()
    {
        touchContact0.performed -= _ => touchCount++;
        touchContact1.performed -= _ => touchCount++;
        touchContact0.canceled -= _ =>
        {
            touchCount--;
            _temp = 0;
        };
        touchContact1.canceled -= _ =>
        {
            touchCount--;
            _temp = 0;
        };
        touchContact0.Disable();
        touchContact1.Disable();

        touchPos1.performed -= OnPinch;
        touchPos0.Disable();
        touchPos1.Disable();

        scrollAction.performed -= OnScroll;
        scrollAction.Disable();
    }

       private void Update()
    {
        if (_isDragging)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;
            if (primaryTouch.press.isPressed)
            {
                Vector2 touchDelta = primaryTouch.delta.ReadValue();
                if (touchDelta != Vector2.zero)
                {
                    transform.position += new Vector3(touchDelta.x, touchDelta.y, 0);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count == 1)
        {
            _isDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
    }

    private void OnPinch(InputAction.CallbackContext context)
    {
        if (touchCount < 2)
            return;
        float distance = Vector2.Distance(
            touchPos0.ReadValue<Vector2>(),
            touchPos1.ReadValue<Vector2>()
        );
        if (_temp == 0)
            _temp = distance;

        if (_temp > distance)
        {
            if (_currentScale > minScale)
            {
                _currentScale -= Time.deltaTime * _scalingRate;
            }
        }
        else if (_temp < distance)
        {
            if (_currentScale < maxScale)
            {
                _currentScale += Time.deltaTime * _scalingRate;
            }
        }

        _temp = distance;
        transform.localScale = new Vector3(
            _currentScale / transform.GetComponent<RectTransform>().sizeDelta.x,
            _currentScale / transform.GetComponent<RectTransform>().sizeDelta.x,
            _currentScale / transform.GetComponent<RectTransform>().sizeDelta.x
        );
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        // Block the scroll view from reacting to the scroll wheel input
        scrollRect.blockScrollWheel = true;

        float scrollValue = context.ReadValue<Vector2>().y;
        if (scrollValue > 0)
        {
            if (_currentScale < maxScale)
            {
                _currentScale += Time.deltaTime * _scalingRate;
            }
        }
        else if (scrollValue < 0)
        {
            if (_currentScale > minScale)
            {
                _currentScale -= Time.deltaTime * _scalingRate;
            }
        }

        transform.localScale = new Vector3(_currentScale, _currentScale, _currentScale);

        // Re-enable the scroll view interaction after a short delay
        Invoke("EnableScrollRect", 0.1f);
    }

    private void EnableScrollRect()
    {
        scrollRect.blockScrollWheel = false;
    }
}

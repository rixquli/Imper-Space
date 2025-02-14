using UnityEngine;
using UnityEngine.EventSystems;

public class VillageCameraController : MonoBehaviour
{
    public Transform PlayerTransform;
    public float DistanceFromPlayer = -500f;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float moveSpeed = 50;

    [SerializeField]
    private float moveSmooth = 50;

    private float zoomSpeed = 75;
    private float zoomSmooth = 5f;

    private VillageCameraControl inputs;

    private bool isMoving = false;
    private bool isZooming = false;

    private float zoom = 10;

    [SerializeField]
    private float minZoom = 5;

    [SerializeField]
    private float maxZoom = 100;
    private readonly float zoomBaseValue = 10;
    private float zoomBaseDistance = 0;
    private float zoomValueBeforeZooming = 0;

    [SerializeField]
    private float maxHorizontalZoomPosition = 200;

    [SerializeField]
    private float maxVerticalZoomPosition = 125;

    private Transform root;
    private Transform pivot;
    private Transform target;

    public bool blockMoving = false;

    private void Awake()
    {
        inputs = new();
        root = new GameObject("CameraHelper").transform;
        pivot = new GameObject("CameraPivot").transform;
        target = new GameObject("CameraTarget").transform;
        _camera.orthographic = true;
        _camera.nearClipPlane = 0;
    }

    private void Start()
    {
        Initialize(Vector3.zero);
    }

    public void Initialize(Vector3 center)
    {
        _camera.orthographicSize = zoom;

        isMoving = false;
        pivot.SetParent(root);
        target.SetParent(pivot);

        root.position = center;

        pivot.localPosition = Vector3.zero;

        target.localPosition = new Vector3(0, 0, -10);
        pivot.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        if (PlayerTransform != null && !StaticDataManager.IsAutoModeActive)
        {
            transform.position = PlayerTransform.position + Vector3.forward * DistanceFromPlayer;
            _camera.orthographicSize = zoomBaseValue;
            return;
        }

#if UNITY_EDITOR
        if (!Input.touchSupported)
        {
            float mouseScroll = inputs.Main.MouseScroll.ReadValue<float>();
            if (mouseScroll > 0)
            {
                zoom = Mathf.Clamp(zoom - 2000f * Time.deltaTime, minZoom, maxZoom);
            }
            else if (mouseScroll < 0)
            {
                zoom = Mathf.Clamp(zoom + 2000f * Time.deltaTime, minZoom, maxZoom);
            }
        }
#endif

        if (isZooming)
        {
            Vector2 touch0 = inputs.Main.TouchPosition0.ReadValue<Vector2>();
            Vector2 touch1 = inputs.Main.TouchPosition1.ReadValue<Vector2>();

            touch0.x /= Screen.width;
            touch1.x /= Screen.width;
            touch0.y /= Screen.height;
            touch1.y /= Screen.height;

            float currentDistance = Vector2.Distance(touch0, touch1);
            float deltaDistance = currentDistance - zoomBaseDistance;

            // Adjust zoom using distance difference
            zoom = Mathf.Clamp(
                zoomValueBeforeZooming - (deltaDistance * zoomSpeed),
                minZoom,
                maxZoom
            );
        }
        else if (isMoving)
        {
            Vector2 move = inputs.Main.MoveDelta.ReadValue<Vector2>();
            // Si le vecteur et null ne fais rien et si isMovingBuilding est vrai et qu'il n'y a qu'un seul touch alors ne fais rien
            if (move != Vector2.zero && (!blockMoving))
            {
                // On peut utiliser la diagonale de l'écran pour uniformiser le mouvement
                float screenDiagonal = Mathf.Sqrt(
                    Screen.width * Screen.width + Screen.height * Screen.height
                );

                move.x = move.x / screenDiagonal;
                move.y = move.y / screenDiagonal;
                float moveSpeedMulriplierByZoom = zoom / zoomBaseValue;

                root.position = GetCameraPositionWithBounds(
                    root.position
                        - root.right.normalized * move.x * moveSpeed * moveSpeedMulriplierByZoom
                );
                root.position = GetCameraPositionWithBounds(
                    root.position
                        - root.up.normalized * move.y * moveSpeed * moveSpeedMulriplierByZoom
                );
                // root.position -= root.right.normalized * move.x * moveSpeed * moveSpeedMulriplierByZoom;
                // root.position -= root.up.normalized * move.y * moveSpeed * moveSpeedMulriplierByZoom;
            }
        }
        if (_camera.transform.position != target.position)
        {
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                target.position,
                moveSmooth * Time.deltaTime
            );
        }
        if (_camera.orthographicSize != zoom)
        {
            _camera.orthographicSize = Mathf.Lerp(
                _camera.orthographicSize,
                zoom,
                zoomSmooth * Time.deltaTime
            );
            root.position = GetCameraPositionWithBounds(root.position);
        }
    }

    private float GetClampPositionForZoomPosition(float targetPosition, float maxZoomPosition)
    {
        return Mathf.Clamp(
            targetPosition,
            -maxZoomPosition + zoom / maxZoom * maxZoomPosition,
            maxZoomPosition - zoom / maxZoom * maxZoomPosition
        );
    }

    private Vector3 GetCameraPositionWithBounds(Vector3 targetPosition)
    {
        Vector3 newPosition = new Vector3(
            GetClampPositionForZoomPosition(targetPosition.x, maxHorizontalZoomPosition),
            GetClampPositionForZoomPosition(targetPosition.y, maxVerticalZoomPosition),
            targetPosition.z
        );
        return newPosition;
    }

    private void OnEnable()
    {
        inputs.Enable();
        inputs.Main.Move.started += _ => PressStarted();
        inputs.Main.Move.canceled += _ => MoveCanceled();
        inputs.Main.TouchZoom.started += _ => ZoomStarted();
        inputs.Main.TouchZoom.canceled += _ => ZoomCanceled();
    }

    private void OnDisable()
    {
        inputs.Disable();
        inputs.Main.Move.started -= _ => PressStarted();
        inputs.Main.Move.canceled -= _ => MoveCanceled();
        inputs.Main.TouchZoom.started -= _ => ZoomStarted();
        inputs.Main.TouchZoom.canceled -= _ => ZoomCanceled();
    }

    private void PressStarted()
    {
        if (!isMoving && UtilsClass.IsPointerOverUI())
            return;

        isMoving = true;
    }

    private void MoveCanceled()
    {
        isMoving = false;
    }

    private void ZoomStarted()
    {
        if (!isZooming)
        {
            Vector2 touch0 = inputs.Main.TouchPosition0.ReadValue<Vector2>();
            Vector2 touch1 = inputs.Main.TouchPosition1.ReadValue<Vector2>();

            touch0.x /= Screen.width;
            touch1.x /= Screen.width;
            touch0.y /= Screen.height;
            touch1.y /= Screen.height;

            zoomBaseDistance = Vector2.Distance(touch0, touch1);
            zoomValueBeforeZooming = zoom;
            isZooming = true;
        }
    }

    private void ZoomCanceled()
    {
        // if (!inputs.Main.TouchPosition0.IsPressed() || !inputs.Main.TouchPosition1.IsPressed())
        // {
        // zoomStartTimer = 0;
        isZooming = false;
        // }
    }
}

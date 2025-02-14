using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FollowPlayer : MonoBehaviour
{
    public static FollowPlayer Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Transform PlayerTransform;
    private bool isTraveling = false;
    public float DistanceFromPlayer = -500f;

    [Header("Dash Effects")]
    public float normalFOV = 10f;
    public float dashFOV = 11.5f;
    public float shakeIntensity = 0.01f;
    public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // public UnityEngine.Rendering.PostProcessing.PostProcessVolume postProcessVolume;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerTransform || isTraveling) return;
        transform.position = PlayerTransform.position + Vector3.forward * DistanceFromPlayer;
    }

    public void Traveling(Vector2 point, float time, bool hasToDesactivateTravelingMode = false)
    {
        isTraveling = true;
        StopAllCoroutines();
        StartCoroutine(TravelingCoroutine(point, time, hasToDesactivateTravelingMode));
    }

    public void TravelingToPlayer(float time)
    {
        StopAllCoroutines();
        StartCoroutine(TravelingCoroutine(PlayerTransform.position, time, true));
    }

    public void StopTraveling()
    {
        isTraveling = false;
    }

    private IEnumerator TravelingCoroutine(Vector2 point, float time, bool hasToDesactivateTravelingMode = false)
    {
        Vector3 startPosition = new Vector3(transform.position.x, transform.position.y, DistanceFromPlayer);
        Vector3 targetPoint = new Vector3(point.x, point.y, DistanceFromPlayer);
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            // Calculate the proportion of time that has passed
            float t = elapsedTime / time;

            // Interpolate between the start position and the target point
            transform.position = Vector3.Lerp(startPosition, targetPoint, t);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Yield until the next frame
            yield return null;
        }

        // Ensure the final position is exactly the target point
        transform.position = targetPoint;
        if (hasToDesactivateTravelingMode) isTraveling = false;
    }

    public void SimulateDash(Vector2 direction, float dashTime)
    {
        StopAllCoroutines();
        DOTween.Kill(transform);
        DOTween.Kill(mainCamera);
        StartCoroutine(DashCoroutine(direction, dashTime));
    }

    private IEnumerator DashCoroutine(Vector2 direction, float dashTime)
    {
        Vector3 startPosition = transform.position;
        Vector3 playerPosition = PlayerTransform.position + Vector3.forward * DistanceFromPlayer;
        Vector3 dashTarget = playerPosition + new Vector3(direction.x, direction.y, 0) * dashTime * 15f;
        float initialSize = mainCamera.orthographicSize;

        // Séquence de dash
        Sequence dashSequence = DOTween.Sequence();

        // Animation de position avec la courbe personnalisée
        // dashSequence.Insert(0, transform.DOMove(playerPosition, dashTime).SetEase(dashCurve));

        // Animation du FOV
        dashSequence.Insert(0, DOTween.To(() => mainCamera.orthographicSize,
            x => mainCamera.orthographicSize = x,
            dashFOV, dashTime * 0.5f).SetEase(Ease.OutQuad));

        dashSequence.Insert(dashTime * 0.5f, DOTween.To(() => mainCamera.orthographicSize,
            x => mainCamera.orthographicSize = x,
            normalFOV, dashTime * 0.5f).SetEase(Ease.InQuad));

        // Effet de tremblement
        // transform.DOShakePosition(dashTime * 0.8f, shakeIntensity, 10, 90, false, true);

        yield return dashSequence.WaitForCompletion();
    }
}


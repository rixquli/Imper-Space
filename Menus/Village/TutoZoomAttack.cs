using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutoZoomAttack : MonoBehaviour
{
    private const float MovementThreshold = 0.1f;
    private Vector2 initialTouchPosition1;
    private Vector2 initialTouchPosition2;
    private bool isTwoFingerTouch = false;
    private bool hasTriedToZoomOut = false;

    private void Awake()
    {
        if (VillagesDataManager.Instance.villageNumber != 1)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Start the coroutine to check for zoom out attempt
        StartCoroutine(CheckZoomOutAttempt());
    }

    void Update()
    {
        if (Touchscreen.current == null)
        {
            return; // Assurez-vous que Touchscreen.current n'est pas null
        }

        if (Touchscreen.current.touches.Count >= 2)
        {
            var touch1 = Touchscreen.current.touches[0];
            var touch2 = Touchscreen.current.touches[1];

            if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began && touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                initialTouchPosition1 = touch1.position.ReadValue();
                initialTouchPosition2 = touch2.position.ReadValue();
                isTwoFingerTouch = true;
            }

            if (isTwoFingerTouch && touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 currentTouchPosition1 = touch1.position.ReadValue();
                Vector2 currentTouchPosition2 = touch2.position.ReadValue();

                float distance1 = Vector2.Distance(initialTouchPosition1, currentTouchPosition1);
                float distance2 = Vector2.Distance(initialTouchPosition2, currentTouchPosition2);

                if (distance1 < MovementThreshold && distance2 < MovementThreshold)
                {
                    gameObject.SetActive(false);
                    hasTriedToZoomOut = true;
                }
            }

            if (touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || touch2.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                isTwoFingerTouch = false;
            }
        }
    }

    private IEnumerator CheckZoomOutAttempt()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // If the user has not tried to zoom out, deactivate the gameObject
        if (!hasTriedToZoomOut)
        {
            gameObject.SetActive(false);
        }
    }
}
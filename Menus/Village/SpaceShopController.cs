using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceShopController : MonoBehaviour
{
    // Reference to the InputAction for tapping
    public InputAction clickAction;
    public InputAction positionAction;

    private void OnEnable()
    {
        // Enable the input action
        clickAction.Enable();
        positionAction.Enable();

        clickAction.performed += OnTap;
    }

    private void OnDisable()
    {
        // Disable the input action
        clickAction.performed -= OnTap;
        clickAction.Disable();
        positionAction.Disable();
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        // Get the world position from either touch or mouse input
        Vector2 worldPosition = GetPointerWorldPosition();

        // Cast a ray from the camera to the world position
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);

        // Iterate through all hits to check if the ray hit the specific game object
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // If the specific game object was hit, trigger the desired action
                AllyBotManager.Instance.ShowCompanionShopPanel();
                break; // Exit the loop once we've found the correct object
            }
        }
    }

    // Utility function to get the current pointer position (touch or mouse)
    private Vector2 GetPointerWorldPosition()
    {
        Vector2 screenPosition = positionAction.ReadValue<Vector2>();
        return Camera.main.ScreenToWorldPoint(screenPosition);
    }
}

using UnityEngine;

public class ShieldSystem : MonoBehaviour
{
    public VariableJoystick joystick;
    public GameObject shield;
    private float currentRotation;

    void Update()
    {
        if (joystick == null) return;
        Vector2 direction = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (direction != Vector2.zero)
        {
            // Calculate the angle based on the direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

            // Update current rotation
            currentRotation = angle;

            // Apply the calculated angle to the shield's rotation
        }
        shield.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
}

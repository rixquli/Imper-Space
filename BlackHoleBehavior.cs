using UnityEngine;

public class BlackHoleBehavior : MonoBehaviour
{
    public float gravityStrength = 10f;
    public float circularForce = 5f;
    public float maxDistance = 5f; // Distance maximum d'effet
    public float minDistanceToDamage = 1f; // Distance minimum pour infliger des dégâts
    public float minSize = 0.1f; // Taille minimum du trou noir
    private float size = 0.1f; // Taille du trou noir
    public float maxSize = 1f; // Taille maximum du trou noir

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 directionToCenter = (Vector2)transform.position - rb.position;
            float distance = directionToCenter.magnitude;
            float forceMultiplier = 1f / distance * size;
            Vector2 normalizedDirection = directionToCenter.normalized;

            if (rb.bodyType == RigidbodyType2D.Kinematic)
            {
                // Store previous velocity and dampen it based on distance
                Vector2 previousVelocity = rb.linearVelocity * (distance / maxDistance * size);

                // Calculate attraction movement
                Vector2 movement = normalizedDirection * gravityStrength * forceMultiplier * Time.fixedDeltaTime;

                // Calculate final position including previous momentum
                Vector2 nextPosition = rb.position + movement + (previousVelocity * Time.fixedDeltaTime);

                // Add circular movement
                Vector2 perpendicularForce = Vector2.Perpendicular(normalizedDirection);
                Vector2 circularMovement = perpendicularForce * circularForce * forceMultiplier * Time.fixedDeltaTime;

                // Apply final position
                rb.MovePosition(nextPosition + circularMovement);
            }
            else
            {
                // For non-kinematic objects, use AddForce
                rb.AddForce(normalizedDirection * gravityStrength * forceMultiplier);
                Vector2 perpendicularForce = Vector2.Perpendicular(normalizedDirection);
                rb.AddForce(perpendicularForce * circularForce * forceMultiplier);
            }

            if (distance < minDistanceToDamage)
            {
                IEntity entity = other.GetComponent<IEntity>();
                if (entity != null)
                {
                    if (entity.TakeDamage(1000))
                    {
                        Grow();
                    }
                }
            }
        }
    }

    private void Grow()
    {
        transform.localScale = Vector3.one * Mathf.Clamp(transform.localScale.x + 0.1f, minSize, maxSize);
        size = transform.localScale.x;
    }
}
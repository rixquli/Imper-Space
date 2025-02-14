using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public enum FiringMode
    {
        Single, Burst, Automatic
    }

    public float health = 100;
    public float shield = 100;
    public float ammo = 100;
    public readonly bool isPlayer = true;

    public GameObject shieldObject;

    public FiringMode firingMode = FiringMode.Single;

    // Start is called before the first frame update
    public void TakeDamage()
    {
        health = Mathf.Max(health - 10, 0);
    }
    public void TakeShieldDamage()
    {
        shield = Mathf.Max(shield - 10, 0);
        if (shield <= 0)
        {
            shieldObject.SetActive(false);
        }
    }
}

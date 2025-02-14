using UnityEngine;

public class BonusBehavior : MonoBehaviour
{
    public enum BonusType
    {
        X2,
        X3,
        Speed,
        FullShield,
        MissileCooldown,
        Invincibility,
    }

    public BonusType bonusType;

    [SerializeField]
    private AudioClip pickUpSound;
    [SerializeField]
    private AudioClip spawnSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponentInParent<PlayerEntity>() != null)
        {
            other.gameObject.GetComponentInParent<PlayerEntity>().ApplyBonus(bonusType);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SoundFXManager.Instance.PlaySoundFXClip(spawnSound, transform, 0.5f);
    }

    private void OnDestroy()
    {
        SoundFXManager.Instance.PlaySoundFXClip(pickUpSound, transform, 0.5f);
    }
}

using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip explosionSound;
    public float volume = 4f;

    public void OnBegin()
    {
        SoundFXManager.Instance.PlaySoundFXClip(explosionSound, transform, volume);
    }
    public void OnFinish()
    {
        Destroy(gameObject);
    }
}

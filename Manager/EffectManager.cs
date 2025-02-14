using UnityEngine;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    public GameObject explosionPrefab;
    private Tween spriteTween;

    void Awake()
    {
        Instance = this;
    }
    private void OnDestroy()
    {
        spriteTween.Kill();
    }

    public void Explosion(Vector2 position, float scale, float volumeMultiplier)
    {
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.transform.localScale = explosion.transform.localScale * scale;
        explosion.GetComponent<ExplosionManager>().volume = volumeMultiplier;

        spriteTween = explosion.transform.DOScale(new Vector3(1.5f * scale, 1.5f * scale, 1.5f * scale), 0.5f);
    }
    public void Explosion(Vector2 position)
    {
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        spriteTween = explosion.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.5f);
    }
}

using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class DestroyedShipBehaviour : MonoBehaviour, IDamageable
{
  private UDictionary<GameObject, Vector2> shipPartsWithOriginPosition = new();
  [SerializeField]
  private float shakeIntensity = 0.5f; // Intensité de la secousse pour simuler la vitesse

  [SerializeField]
  private float shakeDuration = 2f; // Durée de chaque secousse

  [SerializeField]
  private UDictionary<float, GameObject> percentagePerBonuToSpawn = new();
  [SerializeField] private Transform partsParent;

  private Vector2 direction;

  [Header("Health Stats")]
  [SerializeField]
  private float baseMaxHealth = 100;
  [SerializeField]
  private float maxHealth = 500;
  [SerializeField]
  private float health = 500;

  [Header("Health Object")]
  [SerializeField]
  private GameObject healthBarUIPrefab;
  private GameObject healthBarUIObject;
  private EntityHealthBar healthBarUI;

  [Header("Audio Part")]
  [SerializeField]
  private AudioClip[] damageSoundsClip;

  private bool IsDead = false;


  private void Awake()
  {
    float multiplier = Mathf.Max(Mathf.Pow(1.1f, StaticDataManager.maxEnemiesLevel - 1), 1);
    health = maxHealth = baseMaxHealth * multiplier;

    healthBarUIObject = Instantiate(healthBarUIPrefab);
    healthBarUI = healthBarUIObject.GetComponentInChildren<EntityHealthBar>();
    healthBarUI.UpdateHealthBar(health / maxHealth);

    HealthAndLevelBarManager.instance.Add(transform, healthBarUIObject.transform, 5f);
  }

  private void Start()
  {
    foreach (Transform child in partsParent)
    {
      shipPartsWithOriginPosition.Add(child.gameObject, child.localPosition);
    }

    StartCoroutine(ShakeShip());

    direction = Random.insideUnitCircle.normalized;

    Destroy(gameObject, 60 * 10);
  }

  private void Update()
  {
    transform.position += (Vector3)direction * Time.deltaTime * .5f; // TODO: Add speed variable
  }

  // Detecter les collisions avec un objet de la layer "obstacle" et changer la direction
  // Changer la direction comme un rebond sur un mur
  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
    {
      direction = Vector2.Reflect(direction, collision.transform.right);
    }
  }

  // Start is called before the first frame update
  public bool TakeDamage(float damage)
  {
    health = Mathf.Max(health - damage, 0);
    healthBarUI.UpdateHealthBar(health / maxHealth);
    SoundFXManager.Instance.PlayRandomSoundFXClip(damageSoundsClip, transform, 1f);

    if (health <= 0 && !IsDead)
    {
      SpawnBonus();
      EffectManager.Instance.Explosion(transform.position);
      Destroy(gameObject);
      return true;
    }
    return false;
  }

  private void SpawnBonus()
  {
    // From Dictionary to List
    List<KeyValuePair<float, GameObject>> list = new List<KeyValuePair<float, GameObject>>(percentagePerBonuToSpawn);
    foreach (var percentage in list)
    {
      float random = Random.Range(0, 100);
      if (random <= percentage.Key)
      {
        Instantiate(percentage.Value, (Vector2)transform.position + new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), Quaternion.identity);
      }
    }
  }

  private IEnumerator ShakeShip()
  {
    while (true)
    {

      foreach (var shipPart in shipPartsWithOriginPosition)
      {
        var objectTransform = shipPart.Key.transform;
        var originPos = shipPart.Value;
        objectTransform.DOLocalMoveY(
                          originPos.y
                              + Random.Range(-shakeIntensity / 2, shakeIntensity / 2),
                          shakeDuration
                      )
                      .SetEase(Ease.InOutSine);
        objectTransform.DOLocalMoveX(
                originPos.x
                    + Random.Range(-shakeIntensity / 2, shakeIntensity / 2),
                shakeDuration
            )
            .SetEase(Ease.InOutSine);
      }
      yield return new WaitForSeconds(shakeDuration);
    }
  }
}

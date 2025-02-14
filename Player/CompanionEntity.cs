using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CompanionEntity : MonoBehaviour, IEntity, ICanShoot, IPlayerEntity, IEnemy
{
    [Header("Base stats")]
    public float baseMaxHealth;
    public float baseMaxShield;
    public float baseAttackDamage;

    [Header("Stats during the game")]
    public float health = 100;
    public float maxHealth = 100;

    public float shield = 100;
    public float maxShield = 100;
    public float speed = 3;
    public float attackDamage { get; set; } = 10;
    public float bulletPerSecond { get; set; } = 2;

    [HideInInspector] // For IEntity
    public bool isPlayer { get; set; } = true;

    [Header("Prefab for Missile and Laser")]
    public GameObject laserPrefab;
    public GameObject missilePrefab;
    public GameObject LaserPrefab
    {
        get { return laserPrefab; }
        set { laserPrefab = value; }
    }
    public GameObject MissilePrefab
    {
        get { return missilePrefab; }
        set { missilePrefab = value; }
    }
    public float missileAttackDamage { get; set; }
    public float laserAttackDamage { get; set; }
    public event Action OnDeath;

    [Header("Audio Part")]
    [SerializeField]
    private AudioClip[] damageSoundsClip;

    [SerializeField]
    private AudioClip[] shieldDamageSoundsClip;

    [SerializeField]
    private AudioClip shieldRepairedSoundsClip;

    public GameObject shieldObject;
    public SpriteRenderer shieldSpriteRenderer;

    public FiringMode firingMode { get; set; } = FiringMode.Automatic;

    public float healthRegenRate; // Health points per second

    public float shieldRegenRate; // Shield points per second

    [SerializeField]
    private float regenDelay = 5; // Time to wait before starting regeneration

    [SerializeField]
    private float invincibilityTime = 5; // Time to wait before starting regeneration

    private Coroutine healthRegenCoroutine;
    private Coroutine shieldRegenCoroutine;

    [SerializeField]
    private GameObject healthBarUIPrefab;
    private GameObject healthBarUIObject;
    private EntityHealthBar healthBarUI;

    [SerializeField]
    private GameObject levelUIPrefab;
    private GameObject levelUIObject;

    public int currentLevel = 1;
    public int Level
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            float multiplier = Mathf.Max(Mathf.Pow(1.1f, value - 1), 1);

            maxHealth = baseMaxHealth * multiplier;
            health = maxHealth;
            maxShield = baseMaxShield * multiplier;
            shield = maxShield;
            attackDamage = baseAttackDamage * multiplier;
            missileAttackDamage = attackDamage;
            laserAttackDamage = attackDamage;

            UpdateLv();
        }
    }

    public bool IsDead { get; private set; } = false;
    public float AttackDamage
    {
        get { return attackDamage; }
        set { attackDamage = value; }
    }
    public bool IsPlayer
    {
        get { return isPlayer; }
        set { isPlayer = value; }
    }

    private bool IsInvincible = false;
    private GameObject damageableGameObject;

    private CompanionMouvement companionMouvement;

    private void Awake()
    {
        companionMouvement = GetComponent<CompanionMouvement>();

        Level = Mathf.FloorToInt(UnityEngine.Random.Range(StaticDataManager.minEnemiesLevel, StaticDataManager.maxEnemiesLevel));

        if (healthBarUIPrefab != null)
        {
            healthBarUIObject = Instantiate(healthBarUIPrefab);
            healthBarUI = healthBarUIObject.GetComponentInChildren<EntityHealthBar>();

            healthBarUI.UpdateHealthBar(health / maxHealth);
            healthBarUI.image.color = EntityHealthBar.allyBarColor;
        }
        if (levelUIPrefab != null)
        {
            levelUIObject = Instantiate(levelUIPrefab);
            levelUIObject.transform.position = (Vector2)levelUIObject.transform.position + new Vector2(0, .5f);
            TextMeshProUGUI textComponent = levelUIObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"lv. {Level}";
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found in the instantiated gadget item.");
            }
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Damageable"))
            {
                damageableGameObject = child.gameObject;
                break;
            }
        }

        GoToEnemySide();
        if (healthBarUIObject != null && levelUIObject != null)
            HealthAndLevelBarManager.instance.Add(transform, healthBarUIObject.transform, levelUIObject.transform);
    }

    public void Initialize(Transform position)
    {
        if (companionMouvement != null)
        {
            companionMouvement.spawnPoint = position;
        }
    }

    // private void Update()
    // {
    // if (healthBarUIObject.transform)
    //     healthBarUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2f);
    // if (levelUIObject.transform)
    //     levelUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2.5f);
    // }

    public void OnDestroy()
    {
        if (healthBarUIObject != null)
        {
            Destroy(healthBarUIObject);
        }
        if (levelUIObject != null)
        {
            Destroy(levelUIObject);
        }
    }

    public void GoToPlayerSide()
    {
        if (IsPlayer)
            return;
        if (healthBarUI != null)
            healthBarUI.image.color = EntityHealthBar.allyBarColor;
        IsPlayer = true;
        damageableGameObject.tag = "Player";
        UtilsClass.SwitchLayerForCollision(
            gameObject,
            new string[] { "Player", "Enemy" },
            "Player"
        );
    }

    public void GoToEnemySide()
    {
        if (!IsPlayer)
            return;
        if (healthBarUI != null)
            healthBarUI.image.color = EntityHealthBar.enemyBarColor;
        IsPlayer = false;
        damageableGameObject.tag = "Enemy";
        UtilsClass.SwitchLayerForCollision(gameObject, new string[] { "Player", "Enemy" }, "Enemy");
    }

    // Start is called before the first frame update
    public bool TakeDamage(float damage)
    {
        return ApplyDamage(damage);
    }

    private bool ApplyDamage(float damage)
    {
        if (IsDead || IsInvincible)
            return false;
        health = Mathf.Max(health - damage, 0);
        if (SoundFXManager.Instance != null)
            SoundFXManager.Instance.PlayRandomSoundFXClip(damageSoundsClip, transform, 1f);
        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(health / maxHealth);

        if (health <= 0)
        {
            OnDeath?.Invoke();
            Die();
            return true;
        }

        // Stop any existing health regeneration coroutine
        if (healthRegenCoroutine != null)
        {
            StopCoroutine(healthRegenCoroutine);
        }

        // Start the health regeneration coroutine
        healthRegenCoroutine = StartCoroutine(RegenHealth());
        return false;
    }

    public void TakeShieldDamage(float damage)
    {
        if (IsDead || IsInvincible)
            return;
        shield = Mathf.Max(shield - damage, 0);
        if (SoundFXManager.Instance != null)
            SoundFXManager.Instance.PlayRandomSoundFXClip(shieldDamageSoundsClip, transform, 1f);

        if (shield <= 0)
        {
            shieldObject.SetActive(false);
        }

        // Stop any existing shield regeneration coroutine
        if (shieldRegenCoroutine != null)
        {
            StopCoroutine(shieldRegenCoroutine);
        }

        // Start the shield regeneration coroutine
        shieldRegenCoroutine = StartCoroutine(RegenShield());
    }

    // Coroutine to regenerate health
    private IEnumerator RegenHealth()
    {
        // Wait for the regeneration delay
        yield return new WaitForSeconds(regenDelay);

        while (health < maxHealth)
        {
            health = Mathf.Min(health + healthRegenRate * Time.deltaTime, maxHealth);
            if (healthBarUI != null)
                healthBarUI.UpdateHealthBar(health / maxHealth);
            yield return null;
        }

        // Ensure health is set exactly to maxHealth if overshot
        health = maxHealth;
        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(health / maxHealth);
    }

    // Coroutine to regenerate shield
    private IEnumerator RegenShield()
    {
        // Wait for the regeneration delay
        yield return new WaitForSeconds(regenDelay);

        while (shield < maxShield)
        {
            shield = Mathf.Min(shield + shieldRegenRate * Time.deltaTime, maxShield);
            yield return null;
        }

        // Ensure shield is set exactly to maxShield if overshot
        shield = maxShield;

        // Reactivate shield object if it was deactivated
        if (!shieldObject.activeSelf && shield > 0)
        {
            ReactivateShield();
        }
    }

    private void ReactivateShield()
    {
        shieldObject.SetActive(true);
        if (SoundFXManager.Instance != null)
            SoundFXManager.Instance.PlaySoundFXClip(shieldRepairedSoundsClip, transform, 2f);
        FadeInShield();
    }

    // Method to fade in the shield object using DOTween
    private void FadeInShield()
    {
        Color initialColor = shieldSpriteRenderer.color;
        initialColor.a = 0;
        shieldSpriteRenderer.color = initialColor;

        // Use DOTween to fade alpha to 1 over fadeDuration seconds
        shieldSpriteRenderer
            .DOColor(new Color(initialColor.r, initialColor.g, initialColor.b, 1f), 1f)
            .SetEase(Ease.OutBack);
    }

    private void Die()
    {
        if (EffectManager.Instance != null)
            EffectManager.Instance.Explosion(transform.position);
        Destroy(gameObject);
        IsDead = true;
        //TODO: Demander verification
        // Invoke(nameof(Respawn), 5f);
        // Destroy(gameObject);
    }

    public void Respawn()
    {
        StartCoroutine(InvincibilityRegen());
    }

    IEnumerator InvincibilityRegen()
    {
        IsInvincible = true;
        float elapsedTime = 0f; // Track the elapsed time

        float healthIncrementPerSecond = (maxHealth - health) / invincibilityTime;
        float shieldIncrementPerSecond = (maxShield - shield) / invincibilityTime;

        // Regenerate over invincibilityTime seconds
        while (elapsedTime < invincibilityTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            health = Mathf.Min(health + healthIncrementPerSecond * deltaTime, maxHealth);
            shield = Mathf.Min(shield + shieldIncrementPerSecond * deltaTime, maxShield);
            healthBarUI.UpdateHealthBar(health / maxHealth);

            yield return null; // Wait for the next frame
        }

        // Ensure health and shield are set exactly to max values after the period
        health = maxHealth;
        shield = maxShield;
        healthBarUI.UpdateHealthBar(health / maxHealth);

        ReactivateShield();

        IsInvincible = false;
        IsDead = false;
    }

    private void UpdateLv()
    {
        if (levelUIObject == null)
        {
            return;
        }
        TextMeshProUGUI textComponent = levelUIObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"lv. {Level.ToString()}";
        }
    }

    public void OnBulletDestroy(Bullet.DamageType damageType)
    {
        // Do nothing
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerEntity : MonoBehaviour, IEntity, ICanShoot, IPlayerEntity
{
    #region Fields and Properties
    [Header("Player Animation")]
    [SerializeField] private Animator playerAnimator;
    [Header("Player Sprite")]
    public Sprite sprite;

    [Header("Base Stats")]
    public float baseMaxHealth;
    public float baseMaxShield;
    public float baseSpeed = 7f;
    public float baseAttackDamage = 10f;
    public float baseHealthRegenRate = 5f;
    public float baseShieldRegenRate = 5f;
    public float baseMissileAttackDamage = 5f;
    public float baseLaserAttackDamage = 5f;
    public float bulletPerSecond { get; set; } = 2f;
    public float baseDashSpeed = 20f;
    public float dashRegenRate = 10f;

    [Header("Stats during the game")]
    public float health;
    public float maxHealth;
    public float healthRegenRate;
    public float shield;
    public float maxShield;
    public float shieldRegenRate;
    public float speed;
    public float dashSpeed;
    public float maxDashEnergyAmount = 100f;
    public float dashEnergyAmount = 100f;
    public int availableDash = 2;
    public float dashDuration = 0.5f;

    [field: SerializeField]
    public float missileAttackDamage { get; set; }

    [field: SerializeField]
    public float laserAttackDamage { get; set; }

    [field: SerializeField]
    public float attackDamage { get; set; }

    public bool isPlayer { get; set; } = true;
    public bool IsDead { get; private set; } = false;
    public event Action OnDeath;

    public FiringMode firingMode { get; set; } = FiringMode.Automatic;

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

    [Header("Audio")]
    [SerializeField]
    private AudioClip[] damageSoundsClip;

    [SerializeField]
    private AudioClip[] shieldDamageSoundsClip;

    [SerializeField]
    private AudioClip shieldRepairedSoundsClip;

    [Header("GameObject Elements")]
    public GameObject shieldObject;
    public GameObject fullShieldObject;
    public SpriteRenderer shieldSpriteRenderer;
    public ProgressBar healthBar;
    public SpriteRenderer playerImageEmplacement;

    [SerializeField]
    private float regenDelay = 5f;

    [SerializeField]
    private float invincibilityTime = 5f;

    private bool IsInvincible = false;
    private bool isFullShieldActive = false;
    private Coroutine healthRegenCoroutine;
    private Coroutine shieldRegenCoroutine;

    [Serializable]
    public struct PlayerStat
    {
        public XPManager.Stats stat;
        public int level;
    }

    public PlayerStat[] StatsArray;
    private readonly Dictionary<XPManager.Stats, int> StatsDictionary = new();

    [Header("Companions")]
    [SerializeField]
    private Transform[] companionsEmplacements;
    public bool IsPlayer
    {
        get { return true; }
        set { }
    }

    public DashEnergyController dashEnergyController;

    public UDictionary<BonusBehavior.BonusType, float> bonusActiveTimeRemain = new();

    // For MissileButtonManager
    public float missilleCooldown = 5f;
    public float missilleCurrentCooldawn = 0;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        SetupDictionary();
        SetupCompanionEmplacements();
        PlayAnimationOnce("playerentrance");
        SetupBonusDictionary();
    }

    private void Update()
    {
        UpdateBonus();
    }

    #endregion

    #region Initialization Methods

    public void InitializeStats()
    {
        maxHealth = baseMaxHealth;
        maxShield = baseMaxShield;
        healthRegenRate = baseHealthRegenRate;
        health = maxHealth;
        shield = maxShield;
        shieldRegenRate = baseShieldRegenRate;
        speed = baseSpeed;
        dashSpeed = baseDashSpeed;
        missileAttackDamage = baseMissileAttackDamage;
        laserAttackDamage = baseLaserAttackDamage;
        attackDamage = baseAttackDamage;

        UpdatePlayerSprite();
        UpdateStats();
    }

    private void SetupDictionary()
    {
        StatsDictionary.Clear();

        foreach (var playerStat in StatsArray)
        {
            StatsDictionary.Add(playerStat.stat, playerStat.level);
        }

        XPManager.Instance.SetStats(StatsDictionary);
        XPManager.Instance.OnStatsChange += OnStatsChangeHandler;
    }

    private void SetupCompanionEmplacements()
    {
        if (AllyBotManager.Instance == null)
            return;
        foreach (Transform transform in companionsEmplacements)
        {
            AllyBotManager.Instance.companionTransform[transform] = false;
        }
    }

    private void UpdatePlayerSprite()
    {
        if (sprite)
        {
            playerImageEmplacement.sprite = sprite;
        }
    }

    #endregion

    #region Bonus Methods

    private void SetupBonusDictionary()
    {
        foreach (BonusBehavior.BonusType bonusType in Enum.GetValues(typeof(BonusBehavior.BonusType)))
        {
            bonusActiveTimeRemain.Add(bonusType, 0f);
        }
    }

    public void ApplyBonus(BonusBehavior.BonusType bonusType)
    {
        switch (bonusType)
        {
            case BonusBehavior.BonusType.X2:
                bonusActiveTimeRemain[BonusBehavior.BonusType.X2] += 15f;
                break;
            case BonusBehavior.BonusType.X3:
                bonusActiveTimeRemain[BonusBehavior.BonusType.X3] += 15f;
                break;
            case BonusBehavior.BonusType.Speed:
                bonusActiveTimeRemain[BonusBehavior.BonusType.Speed] += 15f;
                break;
            case BonusBehavior.BonusType.FullShield:
                ActiveBonusShield();
                break;
            case BonusBehavior.BonusType.MissileCooldown:
                missilleCurrentCooldawn = 0;
                bonusActiveTimeRemain[BonusBehavior.BonusType.MissileCooldown] += 15f;
                break;
            case BonusBehavior.BonusType.Invincibility:
                bonusActiveTimeRemain[BonusBehavior.BonusType.Invincibility] += 5f;
                break;
            default:
                break;
        }
    }

    private void UpdateBonus()
    {
        foreach (BonusBehavior.BonusType bonusType in Enum.GetValues(typeof(BonusBehavior.BonusType)))
        {
            // Exeption: Si le x3 et le x2 sont actifs, d'abord diminuer le x3
            if (bonusType == BonusBehavior.BonusType.X2 && bonusActiveTimeRemain[BonusBehavior.BonusType.X3] > 0)
            {
                continue;
            }
            else if (bonusActiveTimeRemain[bonusType] > 0)
            {
                bonusActiveTimeRemain[bonusType] = Mathf.Max(bonusActiveTimeRemain[bonusType] - Time.deltaTime, 0);
            }
        }
    }

    #endregion


    #region Shooting Methods

    public void OnBulletDestroy(Bullet.DamageType damageType)
    {
        if (damageType == Bullet.DamageType.Health)
        {
            dashEnergyAmount = Mathf.Min(dashEnergyAmount + dashRegenRate, maxDashEnergyAmount);
            dashEnergyController.SetEnergyBar(dashEnergyAmount);
        }
    }

    #endregion

    #region Stat Management

    private void OnStatsChangeHandler(XPManager.Stats stat, int value)
    {
        if (StatsDictionary.ContainsKey(stat))
        {
            StatsDictionary[stat] = value;
            UpdateStats();
        }
        else
        {
            Debug.LogWarning($"Stat {stat} does not exist in StatsDictionary.");
        }
    }

    public void UpdateStats()
    {
        foreach (var kvp in StatsDictionary)
        {
            float multiplier = kvp.Value * 0.25f + 1f; // 1 point = 25% increase

            switch (kvp.Key)
            {
                case XPManager.Stats.Speed:
                    speed = baseSpeed * multiplier;
                    break;
                case XPManager.Stats.AttackDamage:
                    UpdateAttackDamage(multiplier);
                    break;
                case XPManager.Stats.Health:
                    UpdateHealth(multiplier);
                    break;
                case XPManager.Stats.HealthRegenSpeed:
                    healthRegenRate = baseHealthRegenRate * multiplier;
                    break;
                case XPManager.Stats.Shield:
                    UpdateShield(multiplier);
                    break;
                case XPManager.Stats.ShieldRegenSpeed:
                    shieldRegenRate = baseShieldRegenRate * multiplier;
                    break;
                default:
                    Debug.LogWarning($"Unhandled stat: {kvp.Key}");
                    break;
            }
        }
    }

    private void UpdateAttackDamage(float multiplier)
    {
        attackDamage = baseAttackDamage * multiplier;
        laserAttackDamage = baseLaserAttackDamage * multiplier;
        missileAttackDamage = baseMissileAttackDamage * multiplier;
    }

    private void UpdateHealth(float multiplier)
    {
        maxHealth = baseMaxHealth * multiplier;
        health = Mathf.Min(health * multiplier, maxHealth);
    }

    private void UpdateShield(float multiplier)
    {
        maxShield = baseMaxShield * multiplier;
        shield = Mathf.Min(shield * multiplier, maxShield);
    }

    #endregion

    #region Health and Shield

    public bool TakeDamage(float damage)
    {
        if (IsDead || IsInvincible)
            return false;

        health = Mathf.Max(health - damage, 0);
        SoundFXManager.Instance.PlayRandomSoundFXClip(damageSoundsClip, transform, 1f);

        if (health <= 0)
        {
            OnDeath?.Invoke();
            StartCoroutine(Die());
            return true;
        }
        else
        {
            RestartCoroutine(ref healthRegenCoroutine, RegenHealth());
        }
        return false;
    }

    public void TakeShieldDamage(float damage)
    {
        if (IsDead || IsInvincible)
            return;

        shield = Mathf.Max(shield - damage, 0);
        SoundFXManager.Instance.PlayRandomSoundFXClip(shieldDamageSoundsClip, transform, 1f);

        if (shield <= 0)
        {
            if (isFullShieldActive)
            {
                DeactiveBonusShield();
            }
            else
            {
                shieldObject.SetActive(false);
            }
        }
        else
        {
            RestartCoroutine(ref shieldRegenCoroutine, RegenShield());
        }
    }

    private void RestartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(routine);
    }

    private IEnumerator RegenHealth()
    {
        yield return new WaitForSeconds(regenDelay);

        while (health < maxHealth)
        {
            health = Mathf.Min(health + healthRegenRate * Time.deltaTime, maxHealth);
            yield return null;
        }

        health = maxHealth; // Ensure health is fully restored
    }

    private IEnumerator RegenShield()
    {
        yield return new WaitForSeconds(regenDelay);

        while (shield < maxShield)
        {
            shield = Mathf.Min(shield + shieldRegenRate * Time.deltaTime, maxShield);
            yield return null;
        }

        shield = maxShield; // Ensure shield is fully restored
        if (!isFullShieldActive)
        {
            ReactivateShield();
        }
    }

    private void ReactivateShield()
    {
        shieldObject.SetActive(true);
        SoundFXManager.Instance.PlaySoundFXClip(shieldRepairedSoundsClip, transform, 2f);
        FadeInShield();
    }

    private void FadeInShield()
    {
        Color initialColor = shieldSpriteRenderer.color;
        initialColor.a = 0;
        shieldSpriteRenderer.color = initialColor;

        shieldSpriteRenderer
            .DOColor(new Color(initialColor.r, initialColor.g, initialColor.b, 1f), 1f)
            .SetEase(Ease.OutBack);
    }

    private void ActiveBonusShield()
    {
        if (isFullShieldActive)
            return;
        isFullShieldActive = true;
        shield = maxShield * 1.5f; //TODO: Change shield value with variable
        shieldObject.SetActive(false);
        fullShieldObject.SetActive(true);
    }

    private void DeactiveBonusShield()
    {
        if (!isFullShieldActive)
            return;
        isFullShieldActive = false;
        shield = maxShield;
        ReactivateShield();
        fullShieldObject.SetActive(false);
    }

    #endregion

    #region Death and Respawn

    private IEnumerator Die()
    {
        IsDead = true;
        StopAllCoroutines();

        EffectManager.Instance.Explosion(transform.position);

        UIController.Instance.ShowGameOverScreen();

        yield return new WaitForSeconds(0.1f);
    }

    public void Respawn()
    {
        IsDead = false;

        StartCoroutine(InvincibilityRegen());
    }

    private IEnumerator InvincibilityRegen()
    {
        IsInvincible = true;

        float elapsedTime = 0f;
        float healthIncrementPerSecond = (maxHealth - health) / invincibilityTime;
        float shieldIncrementPerSecond = (maxShield - shield) / invincibilityTime;

        while (elapsedTime < invincibilityTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            health = Mathf.Min(health + healthIncrementPerSecond * deltaTime, maxHealth);
            shield = Mathf.Min(shield + shieldIncrementPerSecond * deltaTime, maxShield);

            yield return null; // Wait for the next frame
        }

        health = maxHealth; // Ensure health is fully restored
        shield = maxShield; // Ensure shield is fully restored

        ReactivateShield();
        IsInvincible = false;
    }

    #endregion

    #region Animation

    public void PlayAnimationOnce(string animationName)
    {
        playerAnimator.Play(animationName);
    }

    // Appelé lorsque l'animation est terminée
    public void OnAnimationEnd()
    {
        // Appliquer la position et la rotation finales
        transform.SetPositionAndRotation(playerAnimator.transform.position, playerAnimator.transform.rotation);

        // Désactiver l'Animator pour éviter le reset
        playerAnimator.enabled = false;
    }


    #endregion
}

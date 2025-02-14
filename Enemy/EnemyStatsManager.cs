using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum LifeReasons
{
    Wandering,
    AttackPlayerBase,
    AttackPlayerBaseInClassicMode,
}

public class EnemyStatsManager : MonoBehaviour, IEnemy, IEntity, ICountable
{
    [Header("Global Parameters")]
    public LifeReasons lifeReason = LifeReasons.Wandering;

    [Header("Boss Parameters")]
    [SerializeField] private bool isBoss = false;
    [SerializeField] private string bossName = "Boss";

    [Header("XP Stats")]
    public float xpAmount;

    [Header("Base Stats at level 1")]
    public float baseMaxHealth;
    public float baseMaxShield;
    public float baseAttackDamage;
    public float baseMissileSpeedMultiplier = 1f;

    [Header("Stats modified during the game")]
    public float health;
    public float maxHealth;

    public float shield;
    public float maxShield;

    public float attackDamage;
    private Tween spriteTween;
    public float AttackDamage
    {
        get { return attackDamage; }
        set { attackDamage = value; }
    }

    private int currentLevel = 3;
    public int Level
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            float multiplier = Mathf.Max(Mathf.Pow(1.1f, value - 1), 1);
            if (StaticDataManager.levelNumber < 3) // for the first 3 levels, the enemy are weaker
            {
                multiplier = 0.5f;
            }

            maxHealth = baseMaxHealth * multiplier;
            health = maxHealth;
            maxShield = baseMaxShield * multiplier;
            shield = maxShield;
            attackDamage = baseAttackDamage * multiplier;
            UpdateLv();
        }
    }

    [Header("Audio Part")]
    [SerializeField]
    private AudioClip[] damageSoundsClip;

    [SerializeField]
    private AudioClip[] shieldDamageSoundsClip;

    [Header("Status variable")]
    public bool isPlayer = false;
    public bool IsPlayer
    {
        get { return isPlayer; }
        set { isPlayer = value; }
    }

    public bool IsDead { get; private set; } = false;

    public bool canSeePlayer = false;
    public bool canAttackPlayer = false;

    [Header("reference to itself component and object")]
    public GameObject shieldObject;

    public EnemyShootSystem shootSystem;
    public Transform tragetTransform;

    [Header("LootTable")]
    public List<LootTableAsset> lootTable = new();
    [Header("Bonus LootTable")]
    public List<ProbabilityGameObject> gameObjectLootTable = new();

    [Header("Health Object")]
    [SerializeField]
    private GameObject healthBarUIPrefab;
    private GameObject healthBarUIObject;
    private EntityHealthBar healthBarUI;

    [Header("Level Object")]
    [SerializeField]
    private GameObject levelUIPrefab;
    private GameObject levelUIObject;

    public event Action OnDeath;

    private void Awake()
    {
        int randomLevel = Random.Range(
            StaticDataManager.minEnemiesLevel,
            StaticDataManager.maxEnemiesLevel + 1
        );
        Level = randomLevel;

        if (healthBarUIPrefab != null)
        {
            healthBarUIObject = Instantiate(healthBarUIPrefab);
            healthBarUI = healthBarUIObject.GetComponentInChildren<EntityHealthBar>();
            healthBarUI.UpdateHealthBar(health / maxHealth);
        }
        if (levelUIPrefab != null)
        {
            levelUIObject = Instantiate(levelUIPrefab);

            TextMeshProUGUI textComponent = levelUIObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                if (isBoss)
                {
                    textComponent.text = $"{bossName} lv. {Level.ToString()}";
                }
                else
                {
                    textComponent.text = $"lv. {Level.ToString()}";
                }
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found in the instantiated gadget item.");
            }
        }
        GoToEnemySide();

        if (shield <= 0)
        {
            shieldObject.SetActive(false);
        }
        if (healthBarUIObject != null && levelUIObject != null)
            HealthAndLevelBarManager.instance.Add(transform, healthBarUIObject.transform, levelUIObject.transform);
    }

    private void Start()
    {
        OnSpawnAdToCounterListener();
    }

    public void GoToPlayerSide()
    {
        if (IsPlayer)
            return;
        if (healthBarUI != null)
            healthBarUI.image.color = EntityHealthBar.allyBarColor;
        IsPlayer = true;
        gameObject.tag = "Player";
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
        healthBarUI.image.color = EntityHealthBar.enemyBarColor;
        IsPlayer = false;
        gameObject.tag = "Enemy";
        UtilsClass.SwitchLayerForCollision(gameObject, new string[] { "Player", "Enemy" }, "Enemy");
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
        spriteTween.Kill();
        OnDeath?.Invoke();
    }

    // Start is called before the first frame update
    public bool TakeDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0);
        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(health / maxHealth);
        SoundFXManager.Instance.PlayRandomSoundFXClip(damageSoundsClip, transform, 1f);

        if (health <= 0 && !IsDead)
        {
            StartCoroutine(Die());
            return true;
        }
        return false;
    }

    public void TakeShieldDamage(float damage)
    {
        shield = Mathf.Max(shield - damage, 0);
        SoundFXManager.Instance.PlayRandomSoundFXClip(shieldDamageSoundsClip, transform, 1f);

        if (shield <= 0)
        {
            shieldObject.SetActive(false);
        }
    }

    IEnumerator Die()
    {
        IsDead = true;
        foreach (var line in lootTable)
        {
            float random = Random.Range(0.0f, 100f);

            if (random <= line.probability)
            {
                Vector3 position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
                InstantiateItemOnGround(line.item.itemId, position + transform.position);
            }
        }
        // From Dictionary to List
        foreach (var percentage in gameObjectLootTable)
        {
            float random = Random.Range(0, 100);
            if (random <= percentage.probability)
            {
                Instantiate(percentage.gameObject, (Vector2)transform.position + new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f)), Quaternion.identity);
            }
        }

        EffectManager.Instance.Explosion(transform.position);
        if (XPManager.Instance != null)
            XPManager.Instance.AddXP(xpAmount);

        yield return null;
        DespawnAndDestroy();
    }

    private void InstantiateItemOnGround(int itemId, Vector2 position)
    {
        ItemData item = ItemDatabase.Instance.GetItemById(itemId);
        if (item != null)
        {
            GameObject itemOnFloor = Instantiate(
                ItemDatabase.Instance.prefab,
                position,
                Quaternion.identity
            );
            ItemOnFloorManager itemManager = itemOnFloor.GetComponent<ItemOnFloorManager>();

            if (itemManager != null)
            {
                itemManager.item = item;
            }
            else
            {
                Debug.LogError(
                    "ItemOnFloorManager component is missing on the instantiated prefab."
                );
            }

            itemOnFloor.transform.localScale = Vector3.zero;
            spriteTween = itemOnFloor
                .transform.DOScale(1, 0.5f)
                .From(0)
                .SetEase(Ease.OutBack)
                .SetDelay(1f);
        }
        else
        {
            Debug.LogError("Item not found");
        }
    }

    private void DespawnAndDestroy()
    {
        // Only the server can despawn and destroy the object
        Destroy(gameObject);
    }

    private void UpdateLv()
    {
        if (levelUIObject == null)
            return;
        TextMeshProUGUI textComponent = levelUIObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"lv. {Level.ToString()}";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in the instantiated gadget item.");
        }
    }

    public void OnSpawnAdToCounterListener()
    {
        if (EnemyCounterManager.Instance == null)
            return;
        EnemyCounterManager.Instance.AddListenerForEnemyCounter(this);
    }

    public void OnBulletDestroy(Bullet.DamageType damageType)
    {
        throw new NotImplementedException();
    }
}

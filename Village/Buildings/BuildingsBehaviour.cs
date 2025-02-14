using System;
using TMPro;
using UnityEngine;

public class BuildingsBehaviour : MonoBehaviour, IDraggable, IGadgetable, IEnemy, IEntity
{
    [Header("Building Settings")]
    public float baseMaxHealth;
    public float health;
    public float maxHealth;

    public bool CanBeDragged = true;
    public bool canBeDragged
    {
        get { return CanBeDragged; }
        set { CanBeDragged = value; }
    }
    public bool canBePickup = true;
    public bool CanBePickup
    {
        get { return canBePickup; }
        set { canBePickup = value; }
    }
    public bool canBeMove = true;
    public bool CanBeMove
    {
        get { return canBeMove; }
        set { canBeMove = value; }
    }
    public bool disableDragObject = false;
    public bool DisableDragObject
    {
        get { return disableDragObject; }
        set { disableDragObject = value; }
    }
    public bool isPurchaseable = false;
    public bool IsPurchaseable
    {
        get { return isPurchaseable; }
        set { isPurchaseable = value; }
    }
    public bool isBought = true;
    public bool IsBought
    {
        get { return isBought; }
        set { isBought = value; }
    }

    public int currentLevel = 1;
    public virtual int Level
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            float multiplier = Mathf.Max(Mathf.Pow(1.1f, value - 1), 1);

            float multiplierBaseOnAverageEnemyLevel = Mathf.Max(
                Mathf.Pow(
                    1.1f,
                    (StaticDataManager.minEnemiesLevel + StaticDataManager.maxEnemiesLevel) / 2 - 1
                ),
                1
            );

            maxHealth = baseMaxHealth * multiplier * multiplierBaseOnAverageEnemyLevel;
            health = maxHealth;

            UpdateLv();
        }
    }

    public GadgetsData gadgetsData;
    public GadgetsData GadgetsData
    {
        get { return gadgetsData; }
    }

    public float baseAttackDamage = 20f;
    protected float attackDamage = 10f;
    public float AttackDamage
    {
        get { return attackDamage; }
        set { attackDamage = value; }
    }
    public bool isPlayer = false;
    public bool IsPlayer
    {
        get { return isPlayer; }
        set { isPlayer = value; }
    }

    public bool isDead = false;
    public bool IsDead
    {
        get { return isDead; }
        set { isDead = value; }
    }

    public bool canBeRotated { get; set; } = false;
    public bool canOpenRecruitPanel { get; set; } = false;

    protected bool isDragging = false;

    [SerializeField]
    protected GameObject healthBarUIPrefab;
    protected GameObject healthBarUIObject;
    protected EntityHealthBar healthBarUI;

    [SerializeField]
    protected GameObject levelUIPrefab;
    protected GameObject levelUIObject;

    public event Action OnDeath;

    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        if (healthBarUIPrefab != null)
        {
            healthBarUIObject = Instantiate(healthBarUIPrefab);
            healthBarUI = healthBarUIObject.GetComponentInChildren<EntityHealthBar>();
            healthBarUI.image.color = EntityHealthBar.enemyBarColor;
        }

        if (levelUIPrefab != null)
        {
            levelUIObject = Instantiate(levelUIPrefab);
        }

        Level = currentLevel;

        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(health / maxHealth);

        if (levelUIObject != null)
        {
            TextMeshProUGUI textComponent = levelUIObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"lv. {Level}";
            }
        }

        GoToEnemySide();
    }

    protected virtual void Update()
    {
        if (healthBarUIObject?.transform)
            healthBarUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2f);
        if (levelUIObject?.transform)
            levelUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2.5f);
    }

    protected virtual void OnDestroy()
    {
        if (healthBarUIObject != null)
        {
            Destroy(healthBarUIObject);
        }
        if (levelUIObject != null)
        {
            Destroy(levelUIObject);
        }
        OnDeath?.Invoke();
    }

    protected void OnEnable()
    {
        if (healthBarUIObject != null)
        {
            healthBarUIObject.SetActive(true);
        }
        if (levelUIObject != null)
        {
            levelUIObject.SetActive(true);
        }
    }

    protected void OnDisable()
    {
        if (healthBarUIObject != null)
        {
            healthBarUIObject.SetActive(false);
        }
        if (levelUIObject != null)
        {
            levelUIObject.SetActive(false);
        }
    }

    public virtual void GoToPlayerSide()
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
        canBeDragged = true;
    }

    public virtual void GoToEnemySide()
    {
        if (!IsPlayer)
            return;
        if (healthBarUI != null)
            healthBarUI.image.color = EntityHealthBar.enemyBarColor;
        IsPlayer = false;
        gameObject.tag = "Enemy";
        UtilsClass.SwitchLayerForCollision(gameObject, new string[] { "Player", "Enemy" }, "Enemy");
        canBeDragged = false;
    }

    public virtual void SpawnAsPreview()
    {
        UtilsClass.DesactivateCollision(gameObject, new string[] { "Player", "Enemy" });
        GoToPlayerSide();
        isDragging = true;
    }

    public virtual void SpawnToVillage()
    {
        GoToPlayerSide();
    }

    public void OnDrag()
    {
        isDragging = true;
    }

    public void OnDragEnd()
    {
        isDragging = false;
        UtilsClass.ActivateCollision(gameObject, new string[] { "Player", "Enemy" });
    }

    public virtual bool TakeDamage(float damage)
    {
        health = Mathf.Max(health - damage, 0);
        healthBarUI.UpdateHealthBar(health / maxHealth);

        if (health <= 0 && !IsDead)
        {
            Die();
            return true;
        }
        return false;
    }

    protected virtual void Die()
    {
        IsDead = true;
        if (EffectManager.Instance != null)
            EffectManager.Instance.Explosion(transform.position);
        Destroy(gameObject);
    }

    public void TakeShieldDamage(float damage) { }

    public virtual void Upgrade()
    {
        Level++;
    }

    protected void UpdateLv()
    {
        if (
            levelUIObject != null
            && levelUIObject.GetComponentInChildren<TextMeshProUGUI>() != null
        )
        {
            levelUIObject.GetComponentInChildren<TextMeshProUGUI>().text =
                $"lv. {Level.ToString()}";
        }

        if (
            spriteRenderer != null
            && gadgetsData != null
            && gadgetsData.upgradeStateAndSprite.TryGetValue(Level, out Sprite sprite)
        )
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public virtual void Buy()
    {
        IsBought = true;
        DisableDragObject = false;
    }

    public void OnBulletDestroy(Bullet.DamageType damageType)
    {
        throw new NotImplementedException();
    }
}

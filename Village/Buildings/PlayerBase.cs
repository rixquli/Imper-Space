using TMPro;
using UnityEngine;

public class PlayerBase : BuildingsBehaviour
{
    [SerializeField]
    private TurretManager turretManager;

    public bool IsPlayerBase = false;

    private bool hasAlreadyShown = false;
    public bool hasWentToEnemySide = false;
    [SerializeField] private GameObject BarrierObject;

    public bool isInvincible = false;

    [Header("For Campaign")]
    [SerializeField] private bool ifDestroyShowGameOver = false;

    public override int Level
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
            turretManager.Level = value;
        }
    }

    protected override void Awake()
    {
        if (healthBarUIPrefab != null)
        {
            healthBarUIObject = Instantiate(healthBarUIPrefab);
            healthBarUI = healthBarUIObject.GetComponentInChildren<EntityHealthBar>();
            healthBarUI.image.color = EntityHealthBar.allyBarColor;
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

        if (turretManager != null)
        {
            turretManager.OnDeath += Die;
        }

    }

    protected void Start()
    {
        turretManager.Level = Level;
        if (IsPlayerBase)
            GoToPlayerSide();
        else
            GoToEnemySide();
    }

    public override void GoToPlayerSide()
    {
        base.GoToPlayerSide();
        if (healthBarUI != null)
            healthBarUI.image.color = EntityHealthBar.allyBarColor;
        turretManager.GoToPlayerSide();
        IsPlayerBase = true;
    }

    public override void GoToEnemySide()
    {
        base.GoToEnemySide();
        if (healthBarUI != null)
            healthBarUI.image.color = EntityHealthBar.enemyBarColor;
        turretManager.GoToEnemySide();
        hasWentToEnemySide = true;
    }

    public void ShowBarrier()
    {
        if (BarrierObject != null)
        {
            isInvincible = true;
            BarrierObject.SetActive(true);
        }
    }

    public void HideBarrier()
    {
        if (BarrierObject != null)
        {
            isInvincible = false;
            BarrierObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        if (healthBarUIObject?.transform)
            healthBarUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2f);
        if (levelUIObject?.transform)
            levelUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2.5f);

        if (GlobalAttackVillageManager.Instance != null)
        {
            if (
                hasWentToEnemySide
                && !hasAlreadyShown
                && (turretManager.IsDead || turretManager.IsPlayer)
            )
            {
                hasAlreadyShown = true;
                GlobalAttackVillageManager.Instance.EnemyBaseDestroyed();
            }
        }
        else if (GlobalDefenseVillageManager.Instance != null)
        {
            if (!hasAlreadyShown && (turretManager.IsDead || !turretManager.IsPlayer))
            {
                hasAlreadyShown = true;
                UIController.Instance.ShowVillageDefenseGameOver();
            }
        }
    }

    public override void Upgrade()
    {
        base.Upgrade();
        turretManager.Level = Level;
    }

    protected override void Die()
    {
        IsDead = true;
        EffectManager.Instance.Explosion(transform.position);
        turretManager.IsDead = true;
        if (ifDestroyShowGameOver)
        {
            UIController.Instance.ShowPlayerBaseGameOverScreen();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LaserTurretController : BuildingsBehaviour
{
    public GameObject spriteObject;
    public float rotationSpeed = 90f; // Vitesse de rotation en degrés par seconde
    public float deceleration = 3f;
    public bool canTurn = true; // Cette variable peut être définie dans l'inspecteur ou par d'autres scripts

    public float targetDetectionDistance = 10f; // Distance minimale pour atteindre le point de destination
    public float claimDetectionDistance = 10f; // Distance minimale pour atteindre le point de destination
    private Vector2 direction;
    private int allies = 0;
    private int enemies = 0;
    public LaserSystem laserSystem;
    private Rigidbody2D rb;
    public float progressionToEnemy = 100;
    public float progressionToAlly = 0;
    public int enemyProgressionRate = 40; // point par seconde
    public int allyProgressionRate = 80; // point par seconde
    public float delayAmount = 1f; //delay in seconds
    public GameObject areaEffectObject;
    public GameObject areaEffectBackgroundObject;


    public float recoveryTime = 10f;
    private List<Transform> companions = new();

    private Color enemyColor = new Color(1f, 0f, 0f, 41f / 100f);
    private Color allyColor = new Color(0f, 0f, 1f, 41f / 100f);

    public override int Level
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            float multiplier = Mathf.Max(Mathf.Pow(1.5f, value - 1), 1);

            float multiplierBaseOnAverageEnemyLevel = Mathf.Max(
                Mathf.Pow(
                    1.1f,
                    (StaticDataManager.minEnemiesLevel + StaticDataManager.maxEnemiesLevel) / 2 - 1
                ),
                1
            );

            maxHealth = baseMaxHealth * multiplier * multiplierBaseOnAverageEnemyLevel;
            health = maxHealth;

            attackDamage = baseAttackDamage * multiplier * multiplierBaseOnAverageEnemyLevel;

            // AddSelfCompanion();

            UpdateLv();
        }
    }

    public Vector2 targetPoint;
    public Vector2 TargetPoint
    {
        get { return targetPoint; }
        set { targetPoint = value; }
    }

    public LayerMask layerMask;

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        base.Awake();
    }

    void Start()
    {
        if (progressionToAlly >= 100)
        {
            progressionToEnemy = 0;
            GoToPlayerSide();
        }
        else if (progressionToEnemy >= 100)
        {
            progressionToAlly = 0;
            GoToEnemySide();
        }

        /* Zone de capture
        // Dans le village, les tourelles ne peuvent pas être capturées
        if (StaticDataManager.levelNumber == -1)
        {
            // Désactiver la capture de la zone
            HideCaptureArea();
        }
        else
        {
            StartCoroutine(UpdateProgression());
        }*/
    }

    protected override void Update()
    {
        if (healthBarUIObject.transform)
            healthBarUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2f);
        if (levelUIObject.transform)
            levelUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 2.5f);

        if (isDragging || (IsPurchaseable && !IsBought))
            return;

        UpdateAreaEffect();

        if (IsDead)
            return;

        if (IsPlayer && progressionToAlly > 0)
        {
            HandleAttack("Enemy");
        }
        else if (!IsPlayer && progressionToEnemy > 0)
        {
            HandleAttack("Player");
        }
        else if (laserSystem.targetTransform != null)
        {
            laserSystem.SetTarget(null);
        }
    }

    protected override void OnDestroy()
    {
        foreach (Transform companion in companions)
        {
            if (companion != null)
                Destroy(companion.gameObject);
        }
        base.OnDestroy();
    }

    private void HideCaptureArea()
    {
        areaEffectObject.SetActive(false);
        areaEffectBackgroundObject.SetActive(false);
    }

    void HandleAttack(string targetTag)
    {
        GetNearestTargets(targetTag);

        if (laserSystem.targetTransform != null)
        {
            StopAndAim();
        }
        else
        {
            RotateInPlace();
        }
    }

    public override bool TakeDamage(float damage)
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

    /* Enlever la possibilité de récupérer la tourelle, qu'elle se régénere
       private new IEnumerator Die()
       {
           IsDead = true;
           if (EffectManager.Instance != null)
               EffectManager.Instance.Explosion(transform.position);

           if (StaticDataManager.levelNumber == -1)
           {
               Destroy(gameObject);
           }

           yield return new WaitForSeconds(recoveryTime);

           Recovery();
       }
       */

    private void Recovery()
    {
        IsDead = false;
        health = maxHealth;
        healthBarUI.UpdateHealthBar(health / maxHealth);
    }

    private void GetNearestTargets(string targetTag)
    {
        // Utiliser Physics2D.OverlapCircleAll pour trouver tous les joueurs dans le rayon de détection
        Collider2D[] colliders = Physics2D
            .OverlapCircleAll(
                transform.position,
                targetDetectionDistance,
                1 << LayerMask.NameToLayer(targetTag)
            );

        Transform closestPlayer = null;
        float minDistance = targetDetectionDistance;

        // Parcourir tous les colliders trouvés dans la zone de détection
        foreach (var collider in colliders)
        {
            Transform playerTransform = collider.transform;

            // Calculer la distance entre la position et le joueur
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // Si cette distance est inférieure à la distance minimale trouvée jusqu'à présent
            if (distance < minDistance)
            {
                IEntity entity =
                    collider.GetComponent<IEntity>() ?? collider.GetComponentInParent<IEntity>();
                if (entity != null && entity.IsDead)
                    continue;

                minDistance = distance;
                closestPlayer = playerTransform; // Mettre à jour le joueur le plus proche
            }
        }

        laserSystem.SetTarget(closestPlayer);
    }

    void StopAndAim()
    {
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);
        direction = laserSystem.targetTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.SetRotation(angle);
    }

    void RotateInPlace()
    {
        float rotationAmount = rotationSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation + rotationAmount);
    }

    private void AddSelfCompanion()
    {
        if (currentLevel < 2 || !isPlayer)
            return;
        // Loop through each level from 2 to current level
        for (int i = 2; i <= currentLevel; i++)
        {
            // If the number of companions is less than the required number for the current level, add companions
            if (companions.Count < i - 1)
            {
                companions.Add(AllyBotManager.Instance.AddCompanionToTurret(transform));
            }
        }
    }

    public override void GoToPlayerSide()
    {
        if (IsPlayer)
            return;
        healthBarUI.image.color = EntityHealthBar.allyBarColor;
        IsPlayer = true;
        gameObject.tag = "Player";
        UtilsClass.SwitchLayerForCollision(
            gameObject,
            new string[] { "Player", "Enemy" },
            "Player"
        );
        canBeDragged = true;
        progressionToEnemy = 0;
        progressionToAlly = 100;

        // AddSelfCompanion();
    }

    public override void GoToEnemySide()
    {
        if (!IsPlayer)
            return;
        healthBarUI.image.color = EntityHealthBar.enemyBarColor;
        IsPlayer = false;
        gameObject.tag = "Enemy";
        UtilsClass.SwitchLayerForCollision(gameObject, new string[] { "Player", "Enemy" }, "Enemy");
        canBeDragged = false;
        progressionToEnemy = 100;
        progressionToAlly = 0;
    }

    void CheckCapturingArea()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position,
            claimDetectionDistance,
            Vector2.right,
            layerMask
        );

        allies = 0;
        enemies = 0;

        foreach (RaycastHit2D hit in hits)
        {
            GameObject obj = hit.collider.gameObject;
            IEntity entity = obj.GetComponent<IEntity>() ?? obj.GetComponentInParent<IEntity>();

            if (entity != null && !entity.IsDead && obj.GetComponent<IGadgetable>() == null)
            {
                if (obj.CompareTag("Player"))
                {
                    allies++;
                }
                else if (obj.CompareTag("Enemy"))
                {
                    enemies++;
                }
            }
        }

        // Mettre à jour la variable de progression selon le nombre d'ennemis
        if (enemies > allies)
        {
            float multiplicator = enemies - allies;
            if (IsDead)
                multiplicator *= 1.5f;
            // S'il y a plus d'ennemis que d'alliés, augmenter la progression vers l'ennemi
            if (progressionToAlly > 0)
            {
                progressionToAlly -= multiplicator * enemyProgressionRate; // Diminuer la progression vers l'allié par seconde
            }
            else
            {
                progressionToEnemy += multiplicator * enemyProgressionRate; // Augmenter la progression vers l'ennemi par seconde
            }
        }
        else if (allies > enemies)
        {
            float multiplicator = allies - enemies;
            if (IsDead)
                multiplicator *= 1.5f;
            // S'il y a plus d'alliés que d'ennemis, augmenter la progression vers l'allié
            if (progressionToEnemy > 0)
            {
                progressionToEnemy -= multiplicator * allyProgressionRate; // Diminuer la progression vers l'ennemi par seconde
            }
            else
            {
                progressionToAlly += multiplicator * allyProgressionRate; // Augmenter la progression vers l'allié par seconde
            }
        }
        else if (enemies == 0 && allies == 0)
        {
            if (isPlayer)
            {
                progressionToAlly += allyProgressionRate * 1.5f;
            }
            else
            {
                progressionToEnemy += enemyProgressionRate * 1.5f;
            }
        }

        // Vérifier les limites des progressions
        progressionToAlly = Mathf.Clamp(progressionToAlly, 0, 100);
        progressionToEnemy = Mathf.Clamp(progressionToEnemy, 0, 100);

        if (progressionToAlly >= 100)
        {
            GoToPlayerSide();
        }
        else if (progressionToEnemy >= 100)
        {
            GoToEnemySide();
        }
    }

    private IEnumerator UpdateProgression()
    {
        while (true)
        {
            yield return new WaitForSeconds(delayAmount);
            CheckCapturingArea();
        }
    }

    void UpdateAreaEffect()
    {
        SpriteRenderer spriteRenderer = areaEffectObject.GetComponent<SpriteRenderer>();

        if (progressionToEnemy > 0)
        {
            if (!spriteRenderer.color.Compare(enemyColor))
            {
                spriteRenderer.color = enemyColor;
            }
            areaEffectObject.transform.localScale = Vector3.Lerp(
                areaEffectObject.transform.localScale,
                new Vector3(20 * progressionToEnemy / 100, 20 * progressionToEnemy / 100, 0),
                Time.deltaTime
            );
        }
        else if (progressionToAlly > 0)
        {
            if (!spriteRenderer.color.Compare(allyColor))
            {
                spriteRenderer.color = allyColor;
            }
            areaEffectObject.transform.localScale = Vector2.Lerp(
                areaEffectObject.transform.localScale,
                new Vector3(20 * progressionToAlly / 100, 20 * progressionToAlly / 100, 0),
                Time.deltaTime
            );
        }
    }

    public override void SpawnAsPreview()
    {
        UtilsClass.DesactivateCollision(gameObject, new string[] { "Player", "Enemy" });
        progressionToEnemy = 0;
        progressionToAlly = 100;
        GoToPlayerSide();
        UpdateAreaEffect();
        isDragging = true;
    }

    public override void SpawnToVillage()
    {
        progressionToEnemy = 0;
        progressionToAlly = 100;
        GoToPlayerSide();
        UpdateAreaEffect();
    }
}

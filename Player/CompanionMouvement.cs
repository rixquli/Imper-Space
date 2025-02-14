using System.Collections;
using System.Linq;
using Unity.Burst;
using UnityEngine;

public class CompanionMouvement : MonoBehaviour, INeedNearestEnemies
{
    public enum CompanionType
    {
        Player,
        Turret,
        VillageAttckWithPlayer,
    }

    public enum VillageAttckWithPlayerType
    {
        Classic,
        CasseMur,
        BotDropper,
        BotDropped,
        LanceLaser,
    }

    [SerializeField]
    private CompanionType companionType = CompanionType.Player;

    [SerializeField]
    private VillageAttckWithPlayerType villageAttckWithPlayerType =
        VillageAttckWithPlayerType.Classic;

    private LayerMask obstacleLayerMask;

    public CompanionEntity entity;
    public bool IsPlayer
    {
        get
        {
            return entity.IsPlayer;
        }
        set { }
    }
    public float deceleration = 3f;
    private float avoidanceDistance = 3f;
    public Rigidbody2D rb;
    public Transform spawnPoint;

    private ShootSystem shootSystem;

    [Header("For Bot Dropper")]
    [SerializeField]
    private GameObject botPrefab;
    private int botCount = 0;
    private float timeBetweenBotDrop = 5f;

    [Header("For Laser Guy")]
    [SerializeField]
    private LaserSystem laserSystem;

    void Awake()
    {
        entity = GetComponent<CompanionEntity>() ?? GetComponentInParent<CompanionEntity>();
        shootSystem = GetComponent<ShootSystem>() ?? GetComponentInParent<ShootSystem>();
    }

    void Start()
    {
        rb.freezeRotation = true;
        if (
            companionType == CompanionType.VillageAttckWithPlayer
            && villageAttckWithPlayerType == VillageAttckWithPlayerType.BotDropper
        )
        {
            PoolManager.Instance.CreatePool("botDropped", botPrefab, 5);
            StartCoroutine(nameof(BotDropper));
        }

    }

    public void Update()
    {
        if (entity.IsDead)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }
        UpdateWithoutNearestEnemies();
    }

    private void GoTo(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            // Déplacement direct en utilisant la position
            // RaycastHit2D hit = Physics2D.Raycast(
            //     transform.position,
            //     direction,
            //     avoidanceDistance,
            //     this.entity.isPlayer
            //         ? 1 << LayerMask.NameToLayer("Enemy")
            //         : 1 << LayerMask.NameToLayer("Player")
            // );

            // if (hit.collider != null)
            // {
            //     rb.linearVelocity = Vector2.Lerp(
            //         rb.linearVelocity,
            //         Vector2.zero,
            //         deceleration * Time.deltaTime
            //     );
            // }
            // else
            // {
            // Si aucun obstacle, continuer vers le joueur;
            rb.linearVelocity = direction * entity.speed;
            // }

            // Rotation du joueur
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
        }
        else
        {
            // Arrêter le mouvement si aucune entrée du joystick
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }
    }

    private void GoTo(Vector2 direction, float angle)
    {
        if (direction != Vector2.zero)
        {
            // Déplacement direct en utilisant la position
            // Vérification des obstacles devant l'ennemi
            // RaycastHit2D hit = Physics2D.Raycast(
            //     transform.position,
            //     direction,
            //     avoidanceDistance,
            //     this.entity.isPlayer
            //         ? 1 << LayerMask.NameToLayer("Player")
            //         : 1 << LayerMask.NameToLayer("Enemy")
            // );

            // if (hit.collider != null)
            // {
            //     rb.linearVelocity = Vector2.Lerp(
            //         rb.linearVelocity,
            //         Vector2.zero,
            //         deceleration * Time.deltaTime
            //     );
            // }
            // else
            // {
            // Si aucun obstacle, continuer vers le joueur
            rb.linearVelocity = direction * entity.speed;
            // }
        }
        else
        {
            // Arrêter le mouvement si aucune entrée du joystick
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }

        rb.SetRotation(angle);
    }

    private IEnumerator BotDropper()
    {
        yield return new WaitForSeconds(timeBetweenBotDrop);
        while (true)
        {
            GameObject bot = Instantiate(botPrefab, transform.position, Quaternion.identity);
            //PoolManager.Instance.GetFromPool("botDropped", transform.position);
            if (bot.TryGetComponent(out IEnemy enemy))
            {
                if (entity.isPlayer)
                {
                    enemy.GoToPlayerSide();
                } // sinon il va automatiquement dans le camp des ennemis
            }

            botCount++;

            yield return new WaitForSeconds(timeBetweenBotDrop);
        }
    }

    // Affiche le rayon de détection des obstacles dans la scène pour visualiser


    #region Auto Mode Algo

    private Transform _currentTargettedEnemy = null;
    private IGadgetable _currentTargettedEnemyGadgetable = null;
    private IEntity _currentTargettedEnemyEntity = null;

    private Transform currentTargettedEnemy
    {
        get { return _currentTargettedEnemy; }
        set
        {
            _currentTargettedEnemy = value;
            if (value != null)
            {
                _currentTargettedEnemyGadgetable =
                    value.GetComponent<IGadgetable>() ?? value.GetComponentInParent<IGadgetable>();
                _currentTargettedEnemyEntity =
                    value.GetComponent<IEntity>() ?? value.GetComponentInParent<IEntity>();
            }
            else
            {
                _currentTargettedEnemyGadgetable = null;
                _currentTargettedEnemyEntity = null;
            }
        }
    }

    [SerializeField]
    private float playerAttackDistance = 10f;
    private Coroutine isCoroutineAttacking = null;
    private Coroutine coroutineRunningAroundEnemy = null;
    private bool isCoroutineRunningAroundSpawnPoint = false;
    private Vector2 targetPointAroundEnemy = new();
    private Vector2 targetPointAroundSpawnPoint = new();
    private readonly float reachDistance = 5f;


    private enum State
    {
        Idle,
        Attacking,
        MovingToEnemy,
        RunningAroundEnemy,
    }

    private State currentState = State.Idle;


    public void UpdateWithNearestEnemies(Transform[] transformColliders)
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState(transformColliders);
                break;
            case State.Attacking:
                HandleAttackingState(transformColliders);
                break;
        }
    }
    public void UpdateWithoutNearestEnemies()
    {
        if (entity.IsDead)
        {
            currentState = State.Idle;
            StopAllCoroutines();
            return;
        }

        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.MovingToEnemy:
                HandleMovingToEnemyState();
                break;
            case State.Attacking:
                HandleAttackingState();
                break;
        }

        CheckIfCanAttack();
    }

    private void HandleIdleState(Transform[] transformColliders)
    {
        if (
           currentTargettedEnemy == null
            || _currentTargettedEnemyEntity?.IsDead == true
            || _currentTargettedEnemyEntity?.IsPlayer == this.entity.isPlayer)
        {

            currentTargettedEnemy = CheckNearestEnemy(transformColliders);
        }
    }
    private void HandleIdleState()
    {
        if (companionType == CompanionType.VillageAttckWithPlayer)
        {
            currentState = currentTargettedEnemy != null ? State.MovingToEnemy : State.Idle;
            if (currentTargettedEnemy == null)
            {
                GoTo(Vector2.zero);
            }
            return;
        }

        Vector2 direction = Vector2.zero;
        if (spawnPoint != null)
        {
            direction = spawnPoint.position - transform.position;
            if (currentTargettedEnemy != null && direction.magnitude < playerAttackDistance * 1.5f)
            {
                currentState = State.MovingToEnemy;
                return;
            }
        }

        if (companionType == CompanionType.Player)
        {
            PrepareForGoigTo(direction);
        }
        else if (companionType == CompanionType.Turret)
        {
            if (!isCoroutineRunningAroundSpawnPoint)
                StartCoroutine(GenerateNewTargetPointAroundSpawnPointWithDelay());
            direction = targetPointAroundSpawnPoint - (Vector2)transform.position;
            PrepareForGoigTo(direction);
        }

        if (coroutineRunningAroundEnemy != null)
        {
            StopCoroutine(coroutineRunningAroundEnemy);
            coroutineRunningAroundEnemy = null;
        }
        // if (isCoroutineRunningCheckNonGadgetEnemyNearby)
        // {
        //     isCoroutineRunningCheckNonGadgetEnemyNearby = false;
        // }
    }

    private void PrepareForGoigTo(Vector2 direction)
    {
        if (direction.magnitude > reachDistance)
        {
            float distance = direction.magnitude;
            float speedMultiplier = 1f;
            if (distance > 15f)
            {
                speedMultiplier = 1f + (distance - 15f) / 10f;
            }

            Vector2 adjustedSpeed = direction.normalized * speedMultiplier;
            GoTo(companionType == CompanionType.Player ? adjustedSpeed : direction.normalized);
        }
        else
        {
            GoTo(Vector2.zero);
        }
    }

    private void HandleMovingToEnemyState()
    {
        if (
            currentTargettedEnemy == null
            || _currentTargettedEnemyEntity.IsDead
        )
        {
            currentState = State.Idle;
            return;
        }

        Vector2 direction = currentTargettedEnemy.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ajouter un décalage aléatoire pour éviter que toutes les entités se placent au même endroit
        Vector2 randomOffset = new Vector2(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f)
        );

        Vector2 targetPosition = (Vector2)currentTargettedEnemy.position + randomOffset;

        direction = targetPosition - (Vector2)transform.position;

        if (direction.magnitude < playerAttackDistance)
        {
            currentState = State.Attacking;
        }
        else
        {
            GoTo(direction.normalized, angle);
        }

        if (coroutineRunningAroundEnemy != null)
        {
            StopCoroutine(coroutineRunningAroundEnemy);
            coroutineRunningAroundEnemy = null;
        }

        // if (isCoroutineRunningCheckNonGadgetEnemyNearby)
        // {
        //     isCoroutineRunningCheckNonGadgetEnemyNearby = false;
        // }
        // if (isCoroutineRunningCheckEnemyNearbyWithDelay)
        // {
        //     isCoroutineRunningCheckEnemyNearbyWithDelay = false;
        // }
    }

    private void HandleAttackingState()
    {
        if (
            currentTargettedEnemy == null
            || _currentTargettedEnemyEntity.IsPlayer == entity.isPlayer
        )
        {
            currentState = State.Idle;
            return;
        }

        Vector2 direction = currentTargettedEnemy.position - transform.position;

        if (direction.magnitude > playerAttackDistance * 2)
        {
            currentState = State.Idle;
            StopAllCoroutines();
            return;
        }

        // if (!isCoroutineRunningCheckNonGadgetEnemyNearby)
        // {
        //     isCoroutineRunningCheckNonGadgetEnemyNearby = true;
        // }

        if (coroutineRunningAroundEnemy == null)
        {
            coroutineRunningAroundEnemy = StartCoroutine(GenerateNewTargetPointAroundEnemyWithDelay());
        }

        Vector2 directionToTarget = targetPointAroundEnemy - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (directionToTarget.magnitude > avoidanceDistance)
        {
            GoTo(directionToTarget.normalized, angle);
        }
        else
        {
            GoTo(Vector2.zero, angle);
        }

    }
    private void HandleAttackingState(Transform[] transformColliders)
    {
        Transform nonGadgetEnemyNearbyChecked = CheckNonGadgetEnemyNearby(transformColliders);
        if (nonGadgetEnemyNearbyChecked != null)
        {
            currentTargettedEnemy = nonGadgetEnemyNearbyChecked;
            currentState = State.MovingToEnemy;
            return;
        }
    }

    private void CheckIfCanAttack()
    {
        if (currentState == State.Attacking)
        {
            if (
                companionType == CompanionType.VillageAttckWithPlayer
                && villageAttckWithPlayerType == VillageAttckWithPlayerType.LanceLaser
            )
            {
                if (currentTargettedEnemy != null)
                {
                    laserSystem.SetTarget(currentTargettedEnemy);
                }
                return;
            }
            else
            {
                if (isCoroutineAttacking == null)
                {
                    isCoroutineAttacking = StartCoroutine(AutomaticShoot());
                }
            }
        }
        else if (isCoroutineAttacking != null)
        {
            StopCoroutine(isCoroutineAttacking);
            isCoroutineAttacking = null;
        }
        else if (laserSystem != null)
        {
            laserSystem.StopTargeting();
        }
    }

    IEnumerator AutomaticShoot()
    {
        while (true)
        {
            // Action à exécuter toutes les secondes
            // Attendre l'intervalle de temps spécifié
            yield return new WaitForSeconds(1 / entity.bulletPerSecond);
            if (currentTargettedEnemy == null)
                continue;
            Vector2 direction = currentTargettedEnemy.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            shootSystem.Shoot(angle);
        }
    }

    private Transform CheckNonGadgetEnemyNearby(Transform[] transformColliders)
    {
        if (currentTargettedEnemy != null)
        {
            IGadgetable currentTargettedGadget =
                currentTargettedEnemy.GetComponent<IGadgetable>()
                ?? currentTargettedEnemy.GetComponentInParent<IGadgetable>();
            if (currentTargettedGadget == null)
                return null;
        }

        float radius = playerAttackDistance;

        Transform closestEnemy = null;
        float minDistance = radius;
        Vector2 position = transform.position;

        foreach (var transformCollider in transformColliders)
        {
            if (transformCollider != null && transformCollider.parent != null && IsPlayerBaseAndIsInvincible(transformCollider.parent.gameObject))
            {
                continue;
            }

            float distance = Vector2.Distance(
                position,
                transformCollider.position
            );

            if (distance < minDistance)
            {
                IEntity entity =
                    transformCollider.GetComponent<IEntity>() ?? transformCollider.GetComponentInParent<IEntity>();
                IGadgetable gadget =
                    transformCollider.GetComponent<IGadgetable>()
                    ?? transformCollider.GetComponentInParent<IGadgetable>();

                if (
                    companionType == CompanionType.VillageAttckWithPlayer
                    && villageAttckWithPlayerType == VillageAttckWithPlayerType.CasseMur
                    && gadget == null
                )
                {
                    continue;
                }

                if (entity != null && !entity.IsDead && gadget == null)
                {
                    minDistance = distance;
                    closestEnemy = transformCollider;
                }
            }
        }

        return closestEnemy;
    }

    private Transform CheckNearestEnemy(Transform[] transformColliders)
    {
        float radius = playerAttackDistance + 1.5f;

        // si le companion ne trouve aucun ennemi dans son rayon est qu'il est dans un village alors il cherche l'ennemi le plus proche
        Transform closestEnemy = null;
        float minDistance = companionType == CompanionType.VillageAttckWithPlayer ? float.MaxValue : reachDistance;
        Vector2 position = transform.position;

        // Filtrer les hitColliders pour ne conserver que ceux qui ont un composant IGadgetable si le compagnon est de type CasseMur
        foreach (var transformCollider in
        villageAttckWithPlayerType == VillageAttckWithPlayerType.CasseMur ? transformColliders
            .Where(e =>
                e.GetComponent<IGadgetable>() != null
                || e.GetComponentInParent<IGadgetable>() != null
            ).ToArray() :
            transformColliders)
        {
            if (transformCollider != null && transformCollider.parent != null && IsPlayerBaseAndIsInvincible(transformCollider.parent.gameObject))
            {
                continue;
            }
            // Calculate the distance once
            float distance = Vector2.Distance(position, transformCollider.position);

            if (distance < minDistance)
            {
                // Cache the IEntity reference
                IEntity entity = transformCollider.GetComponent<IEntity>() ?? transformCollider.GetComponentInParent<IEntity>();

                if (entity != null && !entity.IsDead)
                {
                    minDistance = distance;
                    closestEnemy = transformCollider;
                }
            }
        }

        return closestEnemy;
    }

    private IEnumerator GenerateNewTargetPointAroundEnemyWithDelay()
    {
        while (true)
        {
            GenerateNewTargetPointAroundPlayer();
            yield return new WaitForSeconds(2f);
        }
    }

    private void GenerateNewTargetPointAroundPlayer()
    {
        if (currentTargettedEnemy != null)
        {
            Vector2 playerPosition = currentTargettedEnemy.position;
            Vector2 offset =
                UnityEngine.Random.insideUnitCircle.normalized
                * UnityEngine.Random.Range(5, playerAttackDistance);
            targetPointAroundEnemy = playerPosition + offset;
        }
    }

    private IEnumerator GenerateNewTargetPointAroundSpawnPointWithDelay()
    {
        isCoroutineRunningAroundSpawnPoint = true;
        while (currentState == State.Idle)
        {
            GenerateNewTargetPointAroundSpawnPoint();
            yield return new WaitForSeconds(2f);
        }
        isCoroutineRunningAroundSpawnPoint = false;
    }

    private void GenerateNewTargetPointAroundSpawnPoint()
    {
        // Générer un nouveau point de destination aléatoire dans la zone de jeu
        Vector2 offset =
            UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(10, 15);
        if (spawnPoint != null)
            targetPointAroundSpawnPoint = (Vector2)spawnPoint.position + offset;
    }

    private bool IsPlayerBaseAndIsInvincible(GameObject gameObject)
    {
        if (gameObject != null && gameObject.TryGetComponent(out PlayerBase playerBase))
        {
            return playerBase.isInvincible;
        }
        return false;
    }
    #endregion
}

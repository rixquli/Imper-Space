using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerMouvement : MonoBehaviour
{
    private PlayerEntity entity;
    public VariableJoystick mouvementJoystick;
    public float deceleration = 3f;
    public Rigidbody2D rb;

    private ShootSystem shootSystem;

#if UNITY_EDITOR
    [SerializeField] private InputAction spaceAction;
#endif

    // For double tap
    private const float JoystickMovementThreshold = 0.001f; // the minimum to be considered as a click or a mouvement
    private float lastClickTime;
    private bool wasPressed;
    private const float DOUBLE_CLICK_TIME = 0.3f; // Adjust this value to change sensitivity
    private bool isDashing = false;
    private float dashTimer;
    private const float dashDuration = 0.5f;
    private Vector2 dashVelocity;
    private Vector2 joystickMovementVelocity;

    void Awake()
    {
        entity = GetComponent<PlayerEntity>() ?? GetComponentInParent<PlayerEntity>();
        shootSystem = GetComponent<ShootSystem>() ?? GetComponentInParent<ShootSystem>();
#if UNITY_EDITOR
        // Initialize input action
        spaceAction = new InputAction(binding: "<Keyboard>/space");

        // Subscribe to the action
        spaceAction.performed += ctx => Dash();

        // Enable the action
        spaceAction.Enable();
#endif
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        // Cleanup
        spaceAction.Disable();
        spaceAction.Dispose();
    }
#endif

    void Start()
    {
        rb.freezeRotation = true;
    }

    public void Update()
    {
        if (mouvementJoystick == null)
            return;

        UpdateAutoMode();

        if (entity.IsDead)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);
            return;
        }

        Vector2 direction = new Vector2(mouvementJoystick.Horizontal, mouvementJoystick.Vertical);
        bool isPressed = direction.magnitude > JoystickMovementThreshold;

        // Double click detection
        if (isPressed != wasPressed)
        {
            if (isPressed)
            {
                float timeSinceLastClick = Time.time - lastClickTime;
                if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
                {
                    // Handle double click here
                    Dash();
                }
                lastClickTime = Time.time;
            }
            wasPressed = isPressed;
        }

        // Deactivate auto mode if joystick is used
        if (direction != Vector2.zero)
            StaticDataManager.IsAutoModeActive = false;

        // Else handle single click/movement
        GoTo(direction);
    }

    public void Dash()
    {
        if (entity.IsDead || isDashing)
            return;

        float energyPerDahsh = 100 / entity.availableDash;
        if (entity.dashEnergyAmount >= energyPerDahsh)
        {
            entity.dashEnergyAmount -= energyPerDahsh;
            entity.dashEnergyController.SetEnergyBar(entity.dashEnergyAmount, entity.dashDuration);
        }
        else
        {
            return;
        }

        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        dashVelocity = direction * entity.dashSpeed;
        isDashing = true;
        dashTimer = entity.dashDuration;
        if (FollowPlayer.Instance != null)
            FollowPlayer.Instance.SimulateDash(direction, entity.dashDuration);
    }

    private void GoTo(Vector2 direction)
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
            dashVelocity = Vector2.Lerp(dashVelocity, Vector2.zero, Time.deltaTime / dashDuration);
        }
        else
        {
            dashVelocity = Vector2.Lerp(dashVelocity, Vector2.zero, Time.deltaTime / dashDuration);
        }

        if (direction != Vector2.zero)
        {
            // Déplacement direct en utilisant la position
            joystickMovementVelocity = direction * entity.speed;
            if (entity.bonusActiveTimeRemain[BonusBehavior.BonusType.Speed] > 0)
            {
                joystickMovementVelocity *= 2f; //TODO: Change this value with variable
            }
            rb.linearVelocity = joystickMovementVelocity + dashVelocity;

            // Rotation du joueur
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
        }
        else
        {
            // Arrêter le mouvement si aucune entrée du joystick
            joystickMovementVelocity = Vector2.Lerp(joystickMovementVelocity, Vector2.zero, deceleration * Time.deltaTime);
            rb.linearVelocity = dashVelocity + joystickMovementVelocity;
        }
    }

    private void GoTo(Vector2 direction, float angle)
    {
        if (direction != Vector2.zero)
        {
            // Déplacement direct en utilisant la position
            Vector2 moveVelocity = direction * entity.speed;
            rb.linearVelocity = moveVelocity;

            // Rotation du joueur
        }
        else
        {
            // Arrêter le mouvement si aucune entrée du joystick
            rb.linearVelocity = dashVelocity + Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }

        rb.SetRotation(angle);
    }

    #region Auto Mode Algo

    private Transform currentTargettedEnemy = null;
    private float playerAttackDistance = 10f;
    private Coroutine coroutineAttacking = null;
    private bool isCoroutineRunningAroundEnemy = false;
    private bool isCoroutineRunningWithinTheBase = false;
    private Vector2 targetPointAroundEnemy = new();
    private Vector2 targetPointWithinTheBase = new();
    private Vector2 baseTopLeftCorner = new Vector2(-30f, 20f);
    private Vector2 baseBottomRightCorner = new Vector2(30f, 0f);
    private float reachDistance = 4f;

    private enum State
    {
        Idle,
        Attacking,
        MovingToEnemy,
    }

    private State currentState = State.Idle;

    private void UpdateAutoMode()
    {
        if (!StaticDataManager.IsAutoModeActive || entity.IsDead)
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

    private void HandleIdleState()
    {
        currentTargettedEnemy =
            StaticDataManager.autoMode == AutoMode.TargetEnemy
                ? CheckNearestEnemy()
                : CheckNonGadgetEnemyNearbyTheBase();

        if (currentTargettedEnemy != null)
        {
            currentState = State.MovingToEnemy;
        }

        isCoroutineRunningAroundEnemy = false;

        if (StaticDataManager.autoMode != AutoMode.ProtectBase)
            return;

        if (!isCoroutineRunningWithinTheBase)
        {
            StartCoroutine(GenerateNewTargetPointWithinTheBaseWithDelay());
        }

        Vector2 direction = targetPointWithinTheBase - (Vector2)transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (direction.magnitude > reachDistance)
        {
            GoTo(direction.normalized, angle);
        }
        else
        {
            GoTo(Vector2.zero, angle);
        }
    }

    private void HandleMovingToEnemyState()
    {
        if (currentTargettedEnemy == null || currentTargettedEnemy.CompareTag("Player"))
        {
            currentState = State.Idle;
            return;
        }

        Vector2 direction = currentTargettedEnemy.position - transform.position;

        if (direction.magnitude < playerAttackDistance)
        {
            currentState = State.Attacking;
        }
        else
        {
            GoTo(direction.normalized);
        }

        isCoroutineRunningAroundEnemy = false;
    }

    private void HandleAttackingState()
    {
        if (currentTargettedEnemy == null || currentTargettedEnemy.CompareTag("Player"))
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

        Transform nearbyEnemy = CheckNonGadgetEnemyNearby();
        if (nearbyEnemy != null)
        {
            currentTargettedEnemy = nearbyEnemy;
            currentState = State.MovingToEnemy;
            return;
        }

        if (!isCoroutineRunningAroundEnemy)
        {
            StartCoroutine(GenerateNewTargetPointAroundEnemyWithDelay());
        }

        Vector2 directionToTarget = targetPointAroundEnemy - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (directionToTarget.magnitude > reachDistance)
        {
            GoTo(directionToTarget.normalized, angle);
        }
        else
        {
            GoTo(Vector2.zero, angle);
        }
    }

    private void CheckIfCanAttack()
    {
        if (!entity.IsDead && currentState == State.Attacking)
        {
            if (coroutineAttacking == null)
            {
                coroutineAttacking = StartCoroutine(AutomaticShoot());
            }
        }
        else
        {
            if (coroutineAttacking != null)
            {
                StopCoroutine(coroutineAttacking);
                coroutineAttacking = null;
            }
        }
    }

    IEnumerator AutomaticShoot()
    {
        while (!entity.IsDead && currentState == State.Attacking)
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

    private Transform CheckNonGadgetEnemyNearbyTheBase()
    {
        // Vérifie si l'ennemi actuel ciblé a un gadget
        if (currentTargettedEnemy != null)
        {
            IGadgetable currentTargettedGadget =
                currentTargettedEnemy.GetComponent<IGadgetable>()
                ?? currentTargettedEnemy.GetComponentInParent<IGadgetable>();
            if (currentTargettedGadget == null)
            {
                return null;
            }
        }

        // Récupère tous les ennemis
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDistance = float.MaxValue;
        Transform closestEnemy = null;

        // Parcourt tous les ennemis
        foreach (var enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            IEnemy entity = enemy.GetComponent<IEnemy>() ?? enemy.GetComponentInParent<IEnemy>();
            IGadgetable gadget =
                enemy.GetComponent<IGadgetable>() ?? enemy.GetComponentInParent<IGadgetable>();

            // Vérifie si l'ennemi est valide, vivant et sans gadget
            if (enemyTransform != null && entity != null && !entity.IsDead && gadget == null)
            {
                // Calcule la distance entre l'ennemi et le joueur
                float distance = Vector2.Distance(enemyTransform.position, transform.position);

                // Vérifie si l'ennemi est dans les limites de la base ou à portée d'attaque
                bool isWithinBase =
                    enemyTransform.position.x > baseTopLeftCorner.x
                    && enemyTransform.position.x < baseBottomRightCorner.x
                    && enemyTransform.position.y < baseTopLeftCorner.y
                    && enemyTransform.position.y > baseBottomRightCorner.y;
                bool isWithinAttackDistance = distance < playerAttackDistance;

                if (isWithinBase || isWithinAttackDistance)
                {
                    // Met à jour l'ennemi le plus proche
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestEnemy = enemyTransform;
                    }
                }
            }
        }

        // Renvoie l'ennemi le plus proche si à portée d'attaque, sinon renvoie null
        return closestEnemy;
    }

    private Transform CheckNonGadgetEnemyNearby()
    {
        IGadgetable currentTargettedGadget =
            currentTargettedEnemy.GetComponent<IGadgetable>()
            ?? currentTargettedEnemy.GetComponentInParent<IGadgetable>();
        if (currentTargettedGadget == null)
            return null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDistance = playerAttackDistance;
        Transform closestEnemy = null;

        foreach (var enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            IEnemy entity = enemy.GetComponent<IEnemy>() ?? enemy.GetComponentInParent<IEnemy>();
            IGadgetable gadget =
                enemy.GetComponent<IGadgetable>() ?? enemy.GetComponentInParent<IGadgetable>();

            if (enemyTransform != null && entity != null && !entity.IsDead && gadget == null)
            {
                Vector2 enemyDirection =
                    (Vector2)enemyTransform.position - (Vector2)transform.position;
                float distance = enemyDirection.magnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemyTransform;
                }
            }
        }

        return minDistance < playerAttackDistance ? closestEnemy : null;
    }

    private Transform CheckNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (var enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            IEnemy entity = enemy.GetComponent<IEnemy>() ?? enemy.GetComponentInParent<IEnemy>();

            if (enemyTransform != null && entity != null && !entity.IsDead)
            {
                Vector2 enemyDirection =
                    (Vector2)enemyTransform.position - (Vector2)transform.position;
                float distance = enemyDirection.magnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemyTransform;
                }
            }
        }

        return closestEnemy;
    }

    private IEnumerator GenerateNewTargetPointWithinTheBaseWithDelay()
    {
        isCoroutineRunningWithinTheBase = true;

        while (currentState == State.Idle)
        {
            GenerateNewTargetPointWithinTheBase();
            yield return new WaitForSeconds(3f);
        }
        isCoroutineRunningWithinTheBase = false;
    }

    private void GenerateNewTargetPointWithinTheBase()
    {
        // Générer un point cible aléatoire dans les limites de la base (remplacez par votre logique)
        float x = Random.Range(baseTopLeftCorner.x, baseBottomRightCorner.x);
        float y = Random.Range(baseBottomRightCorner.y, baseTopLeftCorner.y);
        targetPointWithinTheBase = new Vector2(x, y);
    }

    private IEnumerator GenerateNewTargetPointAroundEnemyWithDelay()
    {
        isCoroutineRunningAroundEnemy = true;
        while (currentState == State.Attacking)
        {
            GenerateNewTargetPointAroundEnemy();
            yield return new WaitForSeconds(3f);
        }
        isCoroutineRunningAroundEnemy = false;
    }

    private void GenerateNewTargetPointAroundEnemy()
    {
        if (currentTargettedEnemy)
        {
            Vector2 playerPosition = currentTargettedEnemy.position;
            Vector2 offset =
                UnityEngine.Random.insideUnitCircle.normalized
                * UnityEngine.Random.Range(5, playerAttackDistance);
            targetPointAroundEnemy = playerPosition + offset;
        }
    }
    #endregion
}

public enum AutoMode
{
    TargetEnemy,
    ProtectBase,
}

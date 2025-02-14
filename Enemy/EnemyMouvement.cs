using System;
using System.Collections;
using UnityEngine;

public class EnemyMouvement : MonoBehaviour, IAttackPlayerBase
{
    public float speed = 5f;
    public float deceleration = 3f;
    public Rigidbody2D rb;

    public float reachDistance = 2f; // Distance minimale pour atteindre le point de destination
    public float playerDetectionDistance = 15f; // Distance minimale pour atteindre le point de destination
    public float playerAttackDistance = 5f; // Distance minimale pour atteindre le point de destination
    public float circularSpeed = 5f;
    public float speedRotateAroundPalyer = 0.25f;

    private Vector2 targetPoint; // Point de destination pour le bot
    private bool isCoroutineRunning = false;
    private bool isCoroutineRunningAroundPlayer = false;
    public EnemyStatsManager enemy;
    private Vector2 direction;

    [SerializeField] private float newTargetPointAroundPlayerFrequency = 4f;

    private bool isTargettingPlayerBase = false;
    private bool canUseSpeedMultiplier = false;

    private Vector3 spawnPosition;

    void Start()
    {
        rb.freezeRotation = true;
        spawnPosition = transform.position;
        GenerateNewTargetPoint(); // Générer un nouveau point de destination au démarrage si c'est un bot
    }

    void Destroy()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        CheckPlayerNearby();
        if (!enemy.canSeePlayer && !enemy.canAttackPlayer)
        {
            if (isTargettingPlayerBase)
                targetPoint = new(0, 0);
            MoveToTarget();
        }
        else if (enemy.canSeePlayer)
        {
            RotateAroundPlayer();
        }
    }

    void MoveToTarget()
    {
        direction = targetPoint - (Vector2)transform.position;

        if (direction.magnitude < reachDistance)
        {
            // Générer un nouveau point de destination lorsque le bot atteint le point actuel
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.deltaTime);

            if (!isCoroutineRunning)
            {
                StartCoroutine(GenerateNewTargetPointWithDelay());
            }
        }
        else
        {
            if (direction != Vector2.zero)
            {
                float speedMultiplier = 1f;
                if (canUseSpeedMultiplier && direction.magnitude > reachDistance * 2)
                {
                    // si c'est un companion du joueur reviens plus vite aupres de lui
                    float distance = direction.magnitude;

                    // Si la distance est supérieure à 15, augmenter progressivement le multiplicateur
                    if (distance > 15f)
                    {
                        speedMultiplier = Math.Max(1, 1f + (distance - 15f * 2) / 10f); // Ajustez 10f selon la rapidité souhaitée
                    }
                }
                // Déplacement direct en utilisant la position
                rb.linearVelocity = direction.normalized * speed * speedMultiplier;

                // Rotation du joueur
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rb.SetRotation(angle);
            }
        }
    }

    void RotateAroundPlayer()
    {
        if (enemy.tragetTransform != null)
        {
            float radius = Vector2.Distance(enemy.tragetTransform.position, targetPoint);
            direction = targetPoint - (Vector2)transform.position;
            float angleS = Time.time * speedRotateAroundPalyer;
            Vector2 offset = new Vector2(Mathf.Sin(angleS), Mathf.Cos(angleS)) * radius;
            Vector2 newPos = (Vector2)enemy.tragetTransform.position + offset;
            Vector2 trajectoire = newPos - rb.position;

            // Debug.LogError("Before Coroutine Start");

            if (!isCoroutineRunningAroundPlayer)
            {
                StartCoroutine(GenerateNewTargetPointAroundPlayerWithDelay());
            }
            if (direction.magnitude < reachDistance)
            {
                // Générer un nouveau point de destination lorsque le bot atteint le point actuel
                rb.linearVelocity = Vector2.Lerp(
                    rb.linearVelocity,
                    Vector2.zero,
                    deceleration * Time.deltaTime
                );
            }
            else
            {
                if (direction != Vector2.zero)
                {
                    rb.linearVelocity = Vector2.Lerp(
                        rb.linearVelocity,
                        trajectoire.normalized * circularSpeed,
                        Time.deltaTime
                    );
                    // rb.velocity = trajectoire.normalized * speed;

                    // Déplacement direct en utilisant la position
                    // rb.velocity = direction.normalized * speed;
                }
            }
            Vector2 looktoPlayer = (Vector2)enemy.tragetTransform.position - (Vector2)transform.position;

            // Rotation du joueur
            float angle = Mathf.Atan2(looktoPlayer.y, looktoPlayer.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
        }
    }

    IEnumerator GenerateNewTargetPointWithDelay()
    {
        isCoroutineRunning = true; // Marquer la coroutine comme en cours
        // Attendre 1 seconde avant de générer un nouveau point de destination
        yield return new WaitForSeconds(1f);
        GenerateNewTargetPoint();
        isCoroutineRunning = false; // Marquer la coroutine comme terminée
    }

    void GenerateNewTargetPoint()
    {
        // Générer un nouveau point de destination aléatoire dans la zone de jeu
        targetPoint = new Vector2(
            UnityEngine.Random.Range(spawnPosition.x - 10f, spawnPosition.x + 10f),
            UnityEngine.Random.Range(spawnPosition.y - 10f, spawnPosition.y + 10f)
        );
    }

    IEnumerator GenerateNewTargetPointAroundPlayerWithDelay()
    {
        isCoroutineRunningAroundPlayer = true; // Marquer la coroutine comme en cours
        // Attendre 1 seconde avant de générer un nouveau point de destination
        yield return new WaitForSeconds(newTargetPointAroundPlayerFrequency);
        GenerateNewTargetPointAroundPlayer();
        isCoroutineRunningAroundPlayer = false; // Marquer la coroutine comme terminée
    }

    void GenerateNewTargetPointAroundPlayer()
    {
        if (enemy.tragetTransform)
        {
            Vector2 playerPosition = enemy.tragetTransform.position;
            Vector2 offset =
                UnityEngine.Random.insideUnitCircle.normalized
                * UnityEngine.Random.Range(5, playerDetectionDistance / 2);
            targetPoint = playerPosition + offset;
        }
    }

    void CheckPlayerNearby()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDistance = playerDetectionDistance + 1;
        Transform closestPlayerTransform = null;
        Vector2 minPlayerDirection = Vector2.zero;

        foreach (var player in players)
        {
            Transform currentTragetTransform = player.transform;
            Vector2 playerDirection =
                (Vector2)currentTragetTransform.position - (Vector2)transform.position;
            float distance = playerDirection.magnitude;

            if (distance < playerDetectionDistance && distance < minDistance)
            {
                IEntity entity =
                    player.GetComponent<IEntity>() ?? player.GetComponentInParent<IEntity>();

                if (currentTragetTransform != null && entity != null && !entity.IsDead)
                {
                    minDistance = distance;
                    minPlayerDirection = playerDirection;
                    closestPlayerTransform = currentTragetTransform;
                }
            }
        }

        if (closestPlayerTransform != null)
        {
            enemy.tragetTransform = closestPlayerTransform; // Mettre à jour le enemy.tragetTransform avec le joueur le plus proche
            if (minDistance < playerAttackDistance)
            {
                enemy.canAttackPlayer = true;
            }
            else
            {
                enemy.canAttackPlayer = false;
            }
            direction = minPlayerDirection;
            enemy.canSeePlayer = true;
            enemy.shootSystem.canAttackTarget = true;
        }
        else
        {
            enemy.tragetTransform = null;
            enemy.canSeePlayer = false;
            enemy.canAttackPlayer = false;
            enemy.shootSystem.canAttackTarget = false;
        }
    }

    public void TargetAttackPlayerBase(bool canUseSpeedMultiplier)
    {
        this.canUseSpeedMultiplier = canUseSpeedMultiplier;
        isTargettingPlayerBase = true;
        targetPoint = new(0, 0);
    }
}

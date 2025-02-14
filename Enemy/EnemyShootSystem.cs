using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyShootSystem : MonoBehaviour
{
    public BulletData bulletData;
    public float bulletPerSecond = 1f;
    public bool canAttackTarget = false;

    public Transform spriteTransform;

    private float originalScale;

    private IEnemy enemy;
    private EnemyStatsManager enemyStatsManager;
    private Tween spriteTween;


    [Header("Shooting system let null if not have")]
    [Header("If have single system")]
    public Transform spawnBulletPosition;

    [Header("If have multiple system")]
    public Transform[] spawnBulletsPositions;

    [Header("If have laser system")]
    [SerializeField] private LaserSystem laserSystem;


    //DEBUG
    private bool coroutineRunning = false;

    private void Awake()
    {
        originalScale = spriteTransform.localScale.x;
        enemy = GetComponent<IEnemy>();
        enemyStatsManager = GetComponent<EnemyStatsManager>();
    }

    IEnumerator RepeatAction()
    {
        while (true)
        {
            // Action à exécuter toutes les secondes
            // Attendre l'intervalle de temps spécifié
            yield return new WaitForSeconds(1 / bulletPerSecond);
            Shoot();
        }
    }
    //FIN DEBUG

    void Update()
    {
        if (!enemy.IsDead && canAttackTarget)
        {
            if (laserSystem != null)
            {
                laserSystem.SetTarget(enemyStatsManager.tragetTransform);
            }
            else if (!coroutineRunning)
            {
                coroutineRunning = true;
                StartCoroutine(RepeatAction());
            }
        }
        else
        {
            StopAllCoroutines();
            coroutineRunning = false;
            if (laserSystem != null)
            {
                laserSystem.StopTargeting();
            }
        }
    }

    void Destroy()
    {
        StopAllCoroutines();
        coroutineRunning = false;
    }


    public void ShootSinglePlayer()
    {
        if (spawnBulletPosition != null)
        {
            ShootSinglePlayer(spawnBulletPosition.position);
        }
        if (spawnBulletsPositions != null)
        {
            foreach (Transform spawnBulletPosition in spawnBulletsPositions)
            {
                ShootSinglePlayer(spawnBulletPosition.position);
            }
        }
    }

    public void ShootSinglePlayer(Vector2 spawnBulletPosition)
    {

        // Instancier la balle
        GameObject bullet = Instantiate(bulletData.prefab, spawnBulletPosition, transform.rotation);

        // Obtenir le composant Rigidbody2D
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // Calculer la direction de tir en fonction de l'angle actuel de l'objet
        float angleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 shootDirection = new(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // Appliquer la rotation et la vélocité
        rb.rotation = transform.eulerAngles.z;

        // Initialiser le script Bullet
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            rb.linearVelocity = shootDirection * bulletScript.bulletSpeed;
            bulletScript.Initialize(enemy.IsPlayer, 5f, enemy.AttackDamage, enemyStatsManager.baseMissileSpeedMultiplier);

            // bulletScript.InitializeClientRpc( enemy.IsPlayer, 5f, enemy.AttackDamage);
        }
    }

    private void Shoot()
    {
        ShootSinglePlayer();

        if (spriteTransform)
        {
            // Store a reference to the tween so we can stop it later
            spriteTween = spriteTransform.DOScale(originalScale, 0.5f).From(originalScale * 0.9f).SetEase(Ease.OutBack);
        }
    }

    private void OnDestroy()
    {
        // Stop the tween when the enemy dies
        if (spriteTween != null)
        {
            spriteTween.Kill();
        }
    }
}

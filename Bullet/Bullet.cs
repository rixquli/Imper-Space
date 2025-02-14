using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public enum BulletType
{
    Classic,
    Missile
}

public class Bullet : MonoBehaviour
{
    public enum DamageType
    {
        NoDamage,
        Shield,
        Health,
        Bullet
    }
    [Header("Data")]
    public BulletType bulletType = BulletType.Classic; // Type de balle
    public bool shooterisPlayer; // Identifiant du joueur qui a tiré
    public float lifespan = 5.0f;
    public float attackDamage = 1f;
    public float bulletSpeed = 50f;
    public float rotationSpeed = 0.1f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip[] shootSound;
    [SerializeField]
    private AudioClip destroySound;

    [Header("Explosion For Missiles")]
    [SerializeField] private GameObject explosion;

    public Rigidbody2D rb;

    private Transform currentTarget;
    private Tween tween;

    private bool hadTouch = false;

    public Action<DamageType> OnBulletDestroy;

    private float missileSpeedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

        Destroy(gameObject, lifespan);
    }

    private void OnDestroy()
    {
        tween.Kill();
    }

    private void Update()
    {
        if (bulletType == BulletType.Missile)
        {
            if (currentTarget == null)
            {
                currentTarget = CheckNonGadgetEnemyNearby();
                if (currentTarget == null) return; // Pas de cible à suivre, donc arrêter l'exécution.
            }

            Vector2 direction = (Vector2)currentTarget.position - rb.position;
            direction.Normalize();

            // Calculer l'angle cible basé sur la direction
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Interpoler la rotation actuelle vers la rotation cible
            float angle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.deltaTime);

            rb.rotation = angle;

            // Mettre à jour la vitesse du missile dans la direction de la rotation actuelle
            rb.linearVelocity = transform.right * bulletSpeed * missileSpeedMultiplier;
        }
    }



    // Méthode pour initialiser la balle
    public void Initialize(bool shooter, float lifeSpan, float attackDamage, float missileSpeedMultiplier = 1f)
    {
        shooterisPlayer = shooter;
        this.attackDamage = Mathf.Round(this.attackDamage * attackDamage);
        this.missileSpeedMultiplier = missileSpeedMultiplier;

        Vector3 initialScale = transform.localScale / 2;
        Vector3 targetScale = transform.localScale;
        if (transform != null)
        {
            tween = transform.DOScale(targetScale, 1f).From(initialScale).SetEase(Ease.OutBack);
        }
        if (SoundFXManager.Instance != null)
            SoundFXManager.Instance.PlayRandomSoundFXClip(shootSound, transform, 0.1f);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hadTouch) return;

        float damage = attackDamage;

        if (collision.gameObject.CompareTag("Player"))
        {
            IEntity entity = collision.gameObject.GetComponent<IEntity>() ?? collision.gameObject.GetComponentInParent<IEntity>();
            if (entity != null && entity.IsPlayer != shooterisPlayer)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                entity.TakeDamage(damage);
                OnBulletDestroy?.Invoke(DamageType.Health);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("PlayerShield"))
        {
            IEntity entity = collision.gameObject.GetComponent<IEntity>() ?? collision.gameObject.GetComponentInParent<IEntity>();
            if (entity != null && entity.IsPlayer != shooterisPlayer)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                entity.TakeShieldDamage(damage);
                OnBulletDestroy?.Invoke(DamageType.Shield);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            IEnemy enemy = collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null && enemy.IsPlayer != shooterisPlayer)
            {
                // Infliger des dégâts à l'ennemi (sauf au tireur)
                enemy.TakeDamage(damage);
                OnBulletDestroy?.Invoke(DamageType.Health);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("EnemyShield"))
        {
            IEnemy enemy = collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null && enemy.IsPlayer != shooterisPlayer)
            {
                // Infliger des dégâts au bouclier de l'ennemi (sauf au tireur)
                enemy.TakeShieldDamage(damage);
                OnBulletDestroy?.Invoke(DamageType.Shield);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("EnemyBarrier"))
        {
            OnBulletDestroy?.Invoke(DamageType.NoDamage);
            DespawnAndDestroy();
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null && bullet.shooterisPlayer != shooterisPlayer)
            {
                OnBulletDestroy?.Invoke(DamageType.Bullet);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("Damageable"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                OnBulletDestroy?.Invoke(DamageType.Health);
                DespawnAndDestroy();
            }
        }
        else if (!collision.gameObject.CompareTag("ItemOnFloor") && !collision.gameObject.CompareTag("Bonus"))
        {
            // Si l'objet n'a pas de tag ou si le tag n'est pas l'un des quatre spécifiés
            OnBulletDestroy?.Invoke(DamageType.NoDamage);
            DespawnAndDestroy();
        }
    }



    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }


    private void DespawnAndDestroy()
    {
        hadTouch = true;
        if (bulletType == BulletType.Missile) EffectManager.Instance.Explosion(transform.position, 0.5f, 1 / 4f);
        Destroy(gameObject);
        if (SoundFXManager.Instance != null)
            SoundFXManager.Instance.PlaySoundFXClip(destroySound, transform, 0.05f);
    }

    private Transform CheckNonGadgetEnemyNearby()
    {

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(shooterisPlayer ? "Enemy" : "Player");

        float minDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (var enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            IEntity entity = enemy.GetComponent<IEntity>() ?? enemy.GetComponentInParent<IEntity>();
            IGadgetable gadget = enemy.GetComponent<IGadgetable>() ?? enemy.GetComponentInParent<IGadgetable>();

            if (enemyTransform != null && entity != null && !entity.IsDead && gadget == null)
            {
                Vector2 enemyDirection = (Vector2)enemyTransform.position - (Vector2)transform.position;
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
}

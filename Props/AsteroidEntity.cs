using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AsteroidEntity : MonoBehaviour
{
    [Header("Bonus to spawn, the key is the percentage of chance to spawn the bonus\nSum of all percentage must be under 100")]
    [SerializeField]
    private UDictionary<float, GameObject> bonuToSpawn = new();

    public float damage = 75f;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerEntity player = collision.gameObject.GetComponentInParent<PlayerEntity>();
            if (player != null)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                player.TakeDamage(damage);

                DespawnAndDestroy();
            }
            CompanionEntity companion = collision.gameObject.GetComponentInParent<CompanionEntity>();
            if (companion != null)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                companion.TakeDamage(damage);

                DespawnAndDestroy();
            }

            //if it's an enemy but with the player
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>() ?? collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                enemy.TakeDamage(damage);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("PlayerShield"))
        {
            PlayerEntity player = collision.gameObject?.GetComponentInParent<PlayerEntity>();
            if (player != null)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                player.TakeShieldDamage(damage);
                DespawnAndDestroy();
            }

            CompanionEntity companion = collision.gameObject.GetComponentInParent<CompanionEntity>();
            if (companion != null)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                companion.TakeShieldDamage(damage);

                DespawnAndDestroy();
            }

            //if it's an enemy but whith the player
            IEnemy enemy = collision.gameObject.GetComponent<IEnemy>() ?? collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null)
            {
                // Infliger des dégâts au joueur (sauf au tireur)
                enemy.TakeShieldDamage(damage);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            IEnemy enemy = collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null)
            {
                // Infliger des dégâts à l'ennemi (sauf au tireur)
                enemy.TakeDamage(damage);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("EnemyShield"))
        {
            IEnemy enemy = collision.gameObject.GetComponentInParent<IEnemy>();
            if (enemy != null)
            {
                // Infliger des dégâts au bouclier de l'ennemi (sauf au tireur)
                enemy.TakeShieldDamage(damage);
                DespawnAndDestroy();
            }
        }
        else if (collision.gameObject.CompareTag("Damageable"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                DespawnAndDestroy();
            }
        }
        else if (!collision.gameObject.CompareTag("ItemOnFloor") && !collision.gameObject.CompareTag("Bonus"))
        {
            // Si l'objet n'a pas de tag ou si le tag n'est pas l'un des quatre spécifiés
            DespawnAndDestroy();
        }
    }

    private void SpawnLoot()
    {
        int random = Random.Range(0, 100);
        float percentage = 0;
        foreach (KeyValuePair<float, GameObject> bonus in bonuToSpawn)
        {
            percentage += bonus.Key;
            if (random <= percentage)
            {
                Instantiate(bonus.Value, (Vector2)transform.position + new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)), Quaternion.identity);
                return;
            }
        }
    }


    private void DespawnAndDestroy()
    {
        SpawnLoot();
        EffectManager.Instance.Explosion(transform.position, 0.75f, 1 / 2f);
        Destroy(gameObject);
    }
}

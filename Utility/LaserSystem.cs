using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaserSystem : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    public bool canAttackTarget = false;
    public IEntity targetEntity;
    public Transform targetTransform;
    private Coroutine damageCoroutine;
    private IEnemy selfEntity;

    private void Awake()
    {
        selfEntity = GetComponent<IEnemy>();
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target;
        targetEntity = target?.GetComponent<IEntity>() ?? target?.GetComponentInParent<IEntity>();
    }

    public void StopTargeting()
    {
        targetTransform = null;
        targetEntity = null;
    }

    private void Update()
    {
        bool shouldActivateLaser = targetTransform != null && targetEntity != null && !selfEntity.IsDead && !targetEntity.IsDead;

        if (shouldActivateLaser)
        {
            if (lineRenderer.GetPosition(0) != transform.position || lineRenderer.GetPosition(1) != targetTransform.position)
            {
                lineRenderer.SetPosition(0, transform.position + new Vector3(0, 0, 9));
                lineRenderer.SetPosition(1, targetTransform.position + new Vector3(0, 0, 9));
            }

            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(DamageOverTime());
            }
        }
        else
        {
            if (lineRenderer.GetPosition(0) != Vector3.zero || lineRenderer.GetPosition(1) != Vector3.zero)
            {
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.zero);
            }

            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private void CheckCollision()
    {
        float damage = selfEntity.AttackDamage;

        if (targetTransform == null) return;

        GameObject collisionObject = targetTransform.gameObject;
        bool isPlayer = selfEntity.IsPlayer;

        if (collisionObject == null) return;

        Transform[] children = collisionObject.transform.parent == null ? collisionObject.GetComponentsInChildren<Transform>() : collisionObject.transform.parent.GetComponentsInChildren<Transform>();
        float minDistance = float.MaxValue;
        GameObject closestChild = null;

        foreach (Transform child in children)
        {
            float distance = Vector3.Distance(transform.position, child.position);

            if (distance < minDistance && child.gameObject.layer == LayerMask.NameToLayer("Damageable"))
            {
                minDistance = distance;
                closestChild = child.gameObject;
            }
        }

        if (closestChild != null)
        {
            TryApplyDamage(closestChild, damage, isPlayer);
        }
    }

    private bool TryApplyDamage(GameObject collisionObject, float damage, bool isPlayer)
    {
        if (collisionObject == null || collisionObject.layer != LayerMask.NameToLayer("Damageable")) return false;

        if (collisionObject.CompareTag("Player") || collisionObject.CompareTag("PlayerShield"))
        {
            ApplyDamageToPlayerOrCompanion(collisionObject, damage, isPlayer, collisionObject.CompareTag("PlayerShield"));
            return true;
        }
        else if (collisionObject.CompareTag("Enemy") || collisionObject.CompareTag("EnemyShield"))
        {
            ApplyDamageToEnemy(collisionObject, damage, isPlayer, collisionObject.CompareTag("EnemyShield"));
            return true;
        }

        return false;
    }

    private void ApplyDamageToPlayerOrCompanion(GameObject collisionObject, float damage, bool isPlayer, bool isShield)
    {
        PlayerEntity player = collisionObject.GetComponentInParent<PlayerEntity>();
        if (player != null && player.isPlayer != isPlayer)
        {
            if (isShield)
            {
                player.TakeShieldDamage(damage);
            }
            else
            {
                player.TakeDamage(damage);
            }
            return;
        }

        CompanionEntity companion = collisionObject.GetComponentInParent<CompanionEntity>();
        if (companion != null && companion.isPlayer != isPlayer)
        {
            if (isShield)
            {
                companion.TakeShieldDamage(damage);
            }
            else
            {
                companion.TakeDamage(damage);
            }
            return;
        }

        IEnemy enemy = collisionObject.GetComponent<IEnemy>() ?? collisionObject.GetComponentInParent<IEnemy>();
        if (enemy != null && enemy.IsPlayer != isPlayer)
        {
            if (isShield)
            {
                enemy.TakeShieldDamage(damage);
            }
            else
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    private void ApplyDamageToEnemy(GameObject collisionObject, float damage, bool isPlayer, bool isShield)
    {
        IEnemy enemy = collisionObject.GetComponentInParent<IEnemy>();
        if (enemy != null && enemy.IsPlayer != isPlayer)
        {
            if (isShield)
            {
                enemy.TakeShieldDamage(damage);
            }
            else
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    IEnumerator DamageOverTime()
    {
        while (true)
        {
            // Lance un raycast entre la position de l'ennemi et la position de la cible
            // RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, targetTransform.position - transform.position, 50, LayerMask.GetMask("Damageable"));
            CheckCollision();
            yield return new WaitForSeconds(1 / 10);
        }
    }
}

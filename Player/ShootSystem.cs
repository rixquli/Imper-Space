using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ICanShoot))]
public class ShootSystem : MonoBehaviour
{
    [Header("General Settings")]
    public GameObject ShootDirectionObject;
    private const float ShootDirectionObjectWidth = 15f;
    public VariableJoystick joystick;

    [Header("Firing Modes")]
    public int burstAmount = 3;
    public float burstDelay = 0.1f;

    private Coroutine timerCoroutine;
    private Coroutine automaticFiringCoroutine;
    private bool isMissileButtonSetup = false;
    private float elapsedTime = 0f;
    private Vector2 joystickDirection
    {
        get => new Vector2(joystick.Horizontal, joystick.Vertical);
    }
    private bool hasShot = false;
    private PlayerEntity player;
    private IPlayerEntity playerEntity;
    private IEntity entity;
    private ICanShoot canShoot;
    private bool automaticFiringCoroutineRunning = false;
    private float originalScale;

    // Constantes pour les valeurs magiques
    private const float JoystickMovementThreshold = 0.001f;
    private const float MinimumElapsedTimeForShot = 0.25f;
    private float lastSingleShotTime = 0f;
    private const float MULTIPLIER_BULLET_PER_SECOND_WHEN_CLICK = 3f;

    private Tween tween;

    private void Awake()
    {
        originalScale = transform.localScale.x;
        player = GetComponent<PlayerEntity>();
        playerEntity = GetComponent<IPlayerEntity>();
        entity = GetComponent<IEntity>();
        canShoot = GetComponent<ICanShoot>();
        InitializeShootDirectionObject();
    }

    private void OnDestroy()
    {
        tween.Kill();
    }

    private void InitializeShootDirectionObject()
    {
        if (ShootDirectionObject != null)
            ShootDirectionObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsInputValid() || entity.IsDead)
            return;
        HandleFiringMode();
        UpdateShootDirectionObject();


    }

    private bool IsJoystickMoved()
    {
        return Math.Abs(joystick.Horizontal) > JoystickMovementThreshold
            || Math.Abs(joystick.Vertical) > JoystickMovementThreshold;
    }

    private bool IsInputValid()
    {
        return joystick != null;
    }

    private void HandleFiringMode()
    {
        switch (playerEntity.firingMode)
        {
            case FiringMode.Automatic:
                HandleAutomaticFiringMode();
                break;
            case FiringMode.Single:
                HandleSingleFiringMode();
                break;
            case FiringMode.Burst:
                HandleBurstFiringMode();
                break;
        }
    }

    private void HandleSingleFiringMode()
    {
        if (IsJoystickMoved())
        {
            PrepareToShoot();
        }
        else if (!hasShot)
        {
            HandleShooting(SingleShot);
        }
    }

    private void HandleBurstFiringMode()
    {
        if (IsJoystickMoved())
        {
            PrepareToShoot();
        }
        else if (!hasShot)
        {
            HandleShooting(BurstShot);
        }
    }

    private void HandleAutomaticFiringMode()
    {
        if (!automaticFiringCoroutineRunning && elapsedTime == 0 && joystickDirection.magnitude > JoystickMovementThreshold && timerCoroutine == null)
        {
            StartTimer();
        }

        if (elapsedTime > MinimumElapsedTimeForShot)
        {
            StopTimer();
            elapsedTime = 0;
        }

        if (!automaticFiringCoroutineRunning && joystickDirection.magnitude == 0 && elapsedTime != 0 && elapsedTime < MinimumElapsedTimeForShot && timerCoroutine != null)
        {
            // Check if the cooldown period has elapsed
            if (Time.time - lastSingleShotTime >= (1 / (playerEntity.bulletPerSecond * MULTIPLIER_BULLET_PER_SECOND_WHEN_CLICK)))
            {
                StopTimer();
                Shoot(transform.eulerAngles.z);
                elapsedTime = 0;
                lastSingleShotTime = Time.time; // Update the last shot time
            }
        }

        if (IsJoystickMoved() && joystickDirection.magnitude >= JoystickMovementThreshold && !automaticFiringCoroutineRunning)
        {
            StaticDataManager.IsAutoModeActive = false;
            StartAutomaticFiring();
        }
        else if (!IsJoystickMoved())
        {
            StopAutomaticFiring();
        }
    }

    private void PrepareToShoot()
    {
        StaticDataManager.IsAutoModeActive = false;
        StartTimer();
        UpdateShootDirectionObject();
        hasShot = false;
    }

    private void HandleShooting(Action shootingMethod)
    {
        StopTimer();
        shootingMethod?.Invoke();
        hasShot = true;
        ShootDirectionObject.SetActive(false);
    }

    private void SingleShot()
    {
        if (elapsedTime != 0 && elapsedTime < MinimumElapsedTimeForShot)
        {
            Shoot(transform.eulerAngles.z);
        }
        else if (joystickDirection.magnitude > JoystickMovementThreshold)
        {
            Shoot(CalculateAngle(joystickDirection));
        }
        elapsedTime = 0f;
    }

    private void BurstShot()
    {
        if (elapsedTime != 0 && elapsedTime < MinimumElapsedTimeForShot)
        {
            StartCoroutine(ShootBurst(transform.eulerAngles.z, burstAmount, burstDelay));
        }
        else if (joystickDirection.magnitude > JoystickMovementThreshold)
        {
            StartCoroutine(ShootBurst(CalculateAngle(joystickDirection), burstAmount, burstDelay));
        }
        elapsedTime = 0f;
    }

    private IEnumerator ShootBurst(float angle, int burstAmount, float burstDelay)
    {
        for (int i = 0; i < burstAmount; i++)
        {
            Shoot(angle); // Shoot at the specified angle
            yield return new WaitForSeconds(burstDelay); // Wait for the specified delay before shooting again
        }
    }

    private void StartAutomaticFiring()
    {
        if (automaticFiringCoroutineRunning)
            return;
        automaticFiringCoroutineRunning = true;
        automaticFiringCoroutine = StartCoroutine(AutomaticShoot());
    }

    private void StopAutomaticFiring()
    {
        if (automaticFiringCoroutine != null)
        {
            StopCoroutine(automaticFiringCoroutine);
            automaticFiringCoroutine = null;
            automaticFiringCoroutineRunning = false;
        }
    }

    private IEnumerator AutomaticShoot()
    {
        yield return new WaitForSeconds(MinimumElapsedTimeForShot);
        while (automaticFiringCoroutineRunning)
        {
            HandleAutomaticShooting();
            yield return new WaitForSeconds(1 / playerEntity.bulletPerSecond);
        }
    }

    private void HandleAutomaticShooting()
    {
        if (joystickDirection.magnitude > 0.4f)
        {
            float angle = CalculateAngle(joystickDirection);
            Shoot(angle);
        }
    }

    private float CalculateAngle(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public void ShootMissile()
    {
        float angle = transform.eulerAngles.z;
        Vector2 shootDirection = CalculateShootDirection(angle);
        Vector2 position = (Vector2)transform.position + shootDirection * 2f;

        GameObject missile = Instantiate(
            playerEntity.MissilePrefab,
            position,
            Quaternion.Euler(0, 0, angle)
        );
        InitializeProjectile(missile, shootDirection, playerEntity.missileAttackDamage);
    }

    private Vector2 CalculateShootDirection(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private void InitializeProjectile(GameObject projectile, Vector2 direction, float damage)
    {
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.rotation = CalculateAngle(direction);

        Bullet bulletScript = projectile.GetComponent<Bullet>();
        bulletScript.OnBulletDestroy += entity.OnBulletDestroy;
        rb.linearVelocity = direction * bulletScript.bulletSpeed;
        bulletScript.Initialize(canShoot.isPlayer, 5f, damage);
    }

    public void Shoot(float angle)
    {
        ShootSinglePlayer(angle);

        ApplyRecoilEffect();
    }

    private void ShootSinglePlayer(float angle)
    {
        int bulletCount = 1;
        if (player != null && player.bonusActiveTimeRemain.ContainsKey(BonusBehavior.BonusType.X3) && player.bonusActiveTimeRemain[BonusBehavior.BonusType.X3] > 0)
        {
            bulletCount = 3;
        }
        else if (player != null && player.bonusActiveTimeRemain.ContainsKey(BonusBehavior.BonusType.X2) && player.bonusActiveTimeRemain[BonusBehavior.BonusType.X2] > 0)
        {
            bulletCount = 2;
        }

        Vector2 shootDirection = CalculateShootDirection(angle);

        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 perpendicularDirection = new Vector2(-shootDirection.y, shootDirection.x);
            Vector2 position = (Vector2)transform.position + shootDirection * 2f + perpendicularDirection * (i - (bulletCount - 1) / 3f); //TODO: Change the 3f to a variable

            if (playerEntity.LaserPrefab == null)
                return;
            GameObject bullet = Instantiate(
                playerEntity.LaserPrefab,
                position,
                Quaternion.Euler(0, 0, angle)
            );
            InitializeProjectile(bullet, shootDirection, playerEntity.laserAttackDamage);
        }
    }

    private void ApplyRecoilEffect()
    {
        if (transform)
        {
            tween = transform
                .DOScale(originalScale, 0.5f)
                .From(originalScale * 0.9f)
                .SetEase(Ease.OutBack);
        }
    }

    private void UpdateShootDirectionObject()
    {
        if (joystickDirection.magnitude > JoystickMovementThreshold /*&& elapsedTime > 0.1f*/)
        {
            if (!ShootDirectionObject.activeSelf) ShootDirectionObject.SetActive(true);
            RectTransform directionTransform = ShootDirectionObject.GetComponent<RectTransform>();
            directionTransform.localScale = new Vector3(joystickDirection.magnitude * ShootDirectionObjectWidth, 1, 1);
            directionTransform.localPosition = new Vector3(
                joystickDirection.magnitude * 2 + (ShootDirectionObjectWidth / 2 - 0.4f),
                directionTransform.localPosition.y,
                directionTransform.localPosition.z
            );

            if (joystickDirection != Vector2.zero)
            {
                float angle = CalculateAngle(joystickDirection);
                ShootDirectionObject.transform.parent.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        else
        {
            ShootDirectionObject.SetActive(false);
        }

    }

    private void StartTimer()
    {
        if (timerCoroutine == null)
        {
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    private void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private IEnumerator Timer()
    {
        elapsedTime = 0f;

        while (true)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

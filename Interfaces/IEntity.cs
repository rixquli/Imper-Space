public interface IEntity : IDamageable
{
    event System.Action OnDeath;
    bool IsDead { get; }
    bool IsPlayer { get; set; }
    void TakeShieldDamage(float damage);
    void OnBulletDestroy(Bullet.DamageType damageType);
}

using System;

public interface IEnemy
{
  int Level { get; set; }
  float AttackDamage { get; set; }
  bool IsPlayer { get; set; }
  bool IsDead { get; }

  event Action OnDeath;

  bool TakeDamage(float damage);
  void TakeShieldDamage(float damage);
  void GoToPlayerSide();
  void GoToEnemySide();
}
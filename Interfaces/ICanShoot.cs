public interface ICanShoot
{
  FiringMode firingMode { get; set; }
  float attackDamage { get; set; }
  float bulletPerSecond { get; set; }
  bool isPlayer { get; set; }
}
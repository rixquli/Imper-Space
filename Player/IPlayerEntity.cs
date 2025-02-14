using UnityEngine;

public interface IPlayerEntity
{
    FiringMode firingMode { get; set; }
    float bulletPerSecond { get; set; }
    GameObject LaserPrefab { get; set; }
    float laserAttackDamage { get; set; }
    GameObject MissilePrefab { get; set; }
    float missileAttackDamage { get; set; }
}
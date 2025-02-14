using UnityEngine;

public interface INeedNearestEnemies
{
  void UpdateWithNearestEnemies(Transform[] enemiesTransform);
  bool IsPlayer { get; set; }
}

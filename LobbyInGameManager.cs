using UnityEngine;

public class LobbyInGameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] allyPrefabs;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform center;

    private int enemyCount = 0;

    private void Start()
    {
        foreach (var ally in allyPrefabs)
        {
            GameObject go = Instantiate(ally, new Vector2(-7, 2), Quaternion.identity);
            if (go.TryGetComponent(out CompanionMouvement companionMouvement))
            {
                companionMouvement.spawnPoint = center;
            }
            if (go.TryGetComponent(out IEnemy enemyEntity))
            {
                enemyEntity.GoToPlayerSide();
            }

        }
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        foreach (var enemy in enemyPrefabs)
        {
            GameObject go = Instantiate(enemy, new Vector2(Random.Range(20, 30), Random.Range(-10, -15)), Quaternion.identity);
            if (go.TryGetComponent(out IEnemy enemyEntity))
            {
                enemyCount++;
                enemyEntity.OnDeath += EnemyDeath;
            }
        }
    }

    private void EnemyDeath()
    {
        enemyCount--;
        if (enemyCount <= 0)
        {
            Debug.Log("All enemies are dead");
            SpawnEnemies();
        }
    }
}

using UnityEngine;

public class TestCompanionsManager : MonoBehaviour
{
    public Transform allySpawnPoint;
    public Transform enemySpawnPoint;
    public int amountAlly = 50;
    public int amountEnemy = 50;

    public GameObject[] allyPrefab;
    public GameObject[] enemyPrefab;

    private void Start()
    {
        for (int i = 0; i < amountAlly; i++)
        {
            var allyGo = Instantiate(allyPrefab[Random.Range(0, allyPrefab.Length)], (Vector2)allySpawnPoint.position + new Vector2(Random.Range(-20, 20), Random.Range(-20, 20)), Quaternion.identity);
            if (allyGo.TryGetComponent(out IEnemy ally))
            {
                ally.GoToPlayerSide();
            }

        }

        for (int i = 0; i < amountEnemy; i++)
        {
            var enemyGo = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], (Vector2)enemySpawnPoint.position + new Vector2(Random.Range(-20, 20), Random.Range(-20, 20)), Quaternion.identity);
            if (enemyGo.TryGetComponent(out IEnemy enemy))
            {
                enemy.GoToEnemySide();
            }
        }
    }
}

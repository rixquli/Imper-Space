using System;
using UnityEngine;

public class EnemyCounterManager : MonoBehaviour
{
    public static EnemyCounterManager Instance;

    public event Action OnDeath;

    public int aliveEnemy = 0;
    public int killCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddListenerForEnemyCounter(ICountable countableObject)
    {
        countableObject.OnDeath += OnDeathHandler;
        aliveEnemy++;

    }

    private void OnDeathHandler()
    {
        aliveEnemy--;
        killCount++;
        OnDeath?.Invoke();
    }
}

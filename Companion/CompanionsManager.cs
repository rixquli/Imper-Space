using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class CompanionsManager : MonoBehaviour
{
    private List<INeedNearestEnemies> cachedDataPersistenceObjects;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // Cache the objects at the start
        cachedDataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    private void Start()
    {
        Ticker.OnTickAction += UpdateCompanions;
    }

    private void OnDestroy()
    {
        Ticker.OnTickAction -= UpdateCompanions;
    }

    private List<INeedNearestEnemies> FindAllDataPersistenceObjects()
    {
        // Find all MonoBehaviour objects in the scene
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        // Filter the objects that implement INeedNearestEnemies
        List<INeedNearestEnemies> dataPersistenceObjects = new List<INeedNearestEnemies>();
        foreach (var monoBehaviour in allMonoBehaviours)
        {
            if (monoBehaviour is INeedNearestEnemies dataPersistenceObject)
            {
                dataPersistenceObjects.Add(dataPersistenceObject);
            }
        }

        return dataPersistenceObjects;
    }

    private void UpdateCompanions()
    {
        Profiler.BeginSample("UpdateCompanions");
        List<INeedNearestEnemies> companions = FindAllDataPersistenceObjects();

        // trouvé tout les ennemis et obtenir leur collider et leur transform
        Collider2D[] EnemiesCollider = Physics2D.OverlapCircleAll(Vector2.zero, 500, LayerMask.GetMask("Enemy"));
        Transform[] EnemiesTransform = EnemiesCollider.Select(collider => collider.transform).ToArray();
        Collider2D[] PlayersCollider = Physics2D.OverlapCircleAll(Vector2.zero, 500, LayerMask.GetMask("Player"));
        Transform[] PlayersTransform = PlayersCollider.Select(collider => collider.transform).ToArray();

        foreach (INeedNearestEnemies companion in companions)
        {
            if (companion.IsPlayer)
            {
                companion.UpdateWithNearestEnemies(EnemiesTransform);
            }
            else
            {
                companion.UpdateWithNearestEnemies(PlayersTransform);
            }
        }
        Profiler.EndSample();
    }
}

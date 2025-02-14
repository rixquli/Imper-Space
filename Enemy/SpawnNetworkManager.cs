using System;
using UnityEngine;

public class SpawnNetworkManager : MonoBehaviour
{
    public GameObject[] networkObjectPrefabs; // Assign these via the Inspector
    public Transform[] spawnPoints; // Assign these via the Inspector

    private void Start()
    {
        Debug.Log("SpawnNetworkManager Start called.");


        Debug.Log("Game is in single-player mode. Spawning objects locally.");
        SpawnObjectsLocally();

    }

    private void SpawnObjectsLocally()
    {
        Debug.Log("Spawning objects locally.");
        for (int i = 0; i < networkObjectPrefabs.Length; i++)
        {
            GameObject prefab = networkObjectPrefabs[i];
            Transform spawnPoint = spawnPoints[i];

            if (prefab == null || spawnPoint == null)
            {
                Debug.LogWarning($"Prefab or spawn point at index {i} is null.");
                continue;
            }

            try
            {
                GameObject spawnedObject = Instantiate(prefab, new Vector3(spawnPoint.position.x, spawnPoint.position.y, prefab.transform.position.z), spawnPoint.rotation);
                Debug.Log($"Locally spawned {prefab.name} at {spawnPoint.position}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to spawn {prefab.name} at {spawnPoint.position}. Exception: {ex.Message}");
            }
        }
    }


}

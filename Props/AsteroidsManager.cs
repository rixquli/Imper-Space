using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidsManager : MonoBehaviour
{
    [Serializable]
    public struct AnglesForSpawningArea
    {
        public Transform angle1;
        public Transform angle2;
        public bool needGoToRight;


        public AnglesForSpawningArea(Transform angle1, Transform angle2, bool right)
        {
            this.angle1 = angle1;
            this.angle2 = angle2;
            this.needGoToRight = right;
        }
    }

    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private float minTimeForSpawning = 5f;
    [SerializeField] private float maxTimeForSpawning = 10f;
    [SerializeField] private float minScaleForAsteroid = 3f;
    [SerializeField] private float maxScaleForAsteroid = 5f;
    [SerializeField] private List<AnglesForSpawningArea> anglesForSpawningAreas; // should appear correctly in the inspector

    private void Start()
    {
        StartCoroutine(SpawnAsteroidLoop());
    }

    IEnumerator SpawnAsteroidLoop()
    {
        while (true)
        {

            yield return new WaitForSeconds(Random.Range(minTimeForSpawning, maxTimeForSpawning));
            SpawnAsteroid();
        }
    }

    private void SpawnAsteroid()
    {
        if (anglesForSpawningAreas.Count == 0)
            return;

        AnglesForSpawningArea area = anglesForSpawningAreas[Random.Range(0, anglesForSpawningAreas.Count)];

        Vector2 spawnPoint = new Vector2(Random.Range(area.angle1.position.x, area.angle2.position.x), Random.Range(area.angle1.position.y, area.angle2.position.y));

        float angle = Random.Range(-45f, 45f);
        if (!area.needGoToRight) angle -= 180f;

        float scale = Random.Range(minScaleForAsteroid, maxScaleForAsteroid);

        GameObject asteroid = Instantiate(asteroidPrefab, spawnPoint, Quaternion.identity);

        asteroid.transform.localScale = Vector3.one * scale;

        Rigidbody2D rb = asteroid.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = AngleToVector2(angle);
            float speed = Random.Range(50f, 100f);
            rb.linearVelocity = direction * speed;
            rb.rotation = angle;
        }
    }

    private Vector2 AngleToVector2(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public enum SpawnMode
    {
        FixEnnemies,
        GenrateWithEnnemyType,
    }

    public static WaveManager Instance;

    [SerializeField] private GameObject waveProgressBarObject;

    [SerializeField] private GameObject timerObject;

    [SerializeField] private TextMeshProUGUI textUnderProgressBarObject;

    [SerializeField] private SpawnMode spawnMode = SpawnMode.FixEnnemies;

    [SerializeField] private GameObject[] Enemies;

    [SerializeField] private UDictionary<int, GameObject> ennemieTypes;

    [SerializeField] private BoxCollider2D[] spawnAreaCollider;

    [SerializeField] private StayOrExitController stayOrExitController;

    [SerializeField] private bool canTheyUseSpeedMultiplier = true;

    [Header("Enemy Base")]
    [SerializeField] private EnemyBase enemyBase;

    private float wavePreparationDuration = 60f;
    private float wavePreparationDurationForOtherWave = 5f;
    private int totalEnemiesInWave = 0;
    private int aliveEnemies = 0;
    private int numberOfWaveBeforeQuit = 3;

    [Header("Other")]
    public int waveNumber = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (waveProgressBarObject != null)
            waveProgressBarObject.SetActive(false);
        textUnderProgressBarObject.gameObject.SetActive(false);
    }

    public void StartFirstWave()
    {
        if (StaticDataManager.gameMode == GameMode.InfinityMode)
        {
            StartCoroutine(WaveTimerCoroutine(0));
        }
        else
        {
            StartCoroutine(WaveTimerCoroutine(wavePreparationDuration));
        }

        if (waveProgressBarObject != null)
            waveProgressBarObject.SetActive(true);
        textUnderProgressBarObject.gameObject.SetActive(true);
        UIController.Instance.ShowAlertText($"Wave of enemies is Approaching");
    }

    public void StartOtherWave()
    {
        StartCoroutine(WaveTimerCoroutine(wavePreparationDurationForOtherWave));
    }

    private Vector2 GenerateRandomPoint(Vector2 origin, float minRadius, float maxRadius)
    {
        float angle = Random.Range(0, Mathf.PI * 2);
        float distance = Random.Range(minRadius, maxRadius);
        float x = origin.x + distance * Mathf.Cos(angle);
        float y = origin.y + distance * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    private GameObject[] GenerateEnemyWave(int waveNumber)
    {
        List<GameObject> waveEnemies = new List<GameObject>();

        // Déterminez les proportions des types d'ennemis en fonction de la vague
        int totalEnemies = 5 + waveNumber; // Par exemple, on peut commencer avec 5 ennemis et en ajouter au fur et à mesure

        int numWeakEnemies = Mathf.Max(1, totalEnemies - waveNumber); // Réduit le nombre d'ennemis faibles au fil des vagues
        int numIntermediateEnemies = Mathf.Min(waveNumber, totalEnemies - numWeakEnemies); // Augmente les intermédiaires
        int numBosses = Mathf.Min(
            1 + (waveNumber / numberOfWaveBeforeQuit),
            totalEnemies - numWeakEnemies - numIntermediateEnemies
        ); // Augmente les boss

        // Ajouter les ennemis faibles
        for (int i = 0; i < numWeakEnemies; i++)
        {
            waveEnemies.Add(ennemieTypes[0]);
        }

        // Ajouter les ennemis intermédiaires
        for (int i = 0; i < numIntermediateEnemies; i++)
        {
            waveEnemies.Add(ennemieTypes[1]);
        }

        // Ajouter les boss
        for (int i = 0; i < numBosses; i++)
        {
            waveEnemies.Add(ennemieTypes[2]);
        }

        // Mélanger la liste pour que les ennemis apparaissent dans un ordre aléatoire
        waveEnemies = waveEnemies.OrderBy(e => Random.value).ToList();

        Debug.Log(waveEnemies.Count);

        return waveEnemies.ToArray(); // Convertir la liste en tableau pour le retour
    }

    private void SpawnEnemies(GameObject[] enemies)
    {
        Vector2 origin = new Vector2(0, 0);
        GameObject[] enemyToSpawn =
            spawnMode == SpawnMode.FixEnnemies ? enemies : GenerateEnemyWave(waveNumber);
        totalEnemiesInWave = enemyToSpawn.Length;
        aliveEnemies = totalEnemiesInWave;
        UIController.Instance.SetWaveBar(aliveEnemies, totalEnemiesInWave);
        UpdateText($"{aliveEnemies} alive enemies");

        foreach (var enemy in enemyToSpawn)
        {
            Vector2 spawnPoint = GetRandomPoint();
            if (spawnPoint == Vector2.zero)
                continue;
            Debug.Log("Spawning enemy" + enemy.name);
            ObjectManager.Instance.SpawnObject(
                enemy,
                spawnPoint,
                Quaternion.identity,
                (GameObject enemyObject) =>
                {
                    enemyObject.GetComponent<IEnemy>().OnDeath += HandleEnemyDeath;
                    IAttackPlayerBase attackPlayerBase =
                        enemyObject.GetComponent<IAttackPlayerBase>();
                    if (attackPlayerBase != null)
                    {
                        attackPlayerBase.TargetAttackPlayerBase(canTheyUseSpeedMultiplier);
                    }

                    IEnemy enemy = enemyObject.GetComponent<IEnemy>();
                    if (enemy != null)
                    {
                        enemy.Level = CalculateEnemyLevel(waveNumber);
                    }
                }
            );
            // GameObject enemyObject = Instantiate(enemy, spawnPoint, Quaternion.identity);
        }
    }

    private void HandleEnemyDeath()
    {
        aliveEnemies--;
        UIController.Instance.SetWaveBar(aliveEnemies, totalEnemiesInWave);
        UpdateText($"{aliveEnemies} alive enemies");

        // TODO: Message la base ennemi peut etre attaqué
        if (aliveEnemies <= 0)
        {
            if (StaticDataManager.gameMode == GameMode.Classic)
            {
                UpdateText($"Attack the Enemy Base");
                enemyBase.DesactivateShield();
                UIController.Instance.ShowAlertText(
                    $"The Enemy Base Shield has been desactivated \nDestroy it"
                );
                StaticDataManager.autoMode = AutoMode.TargetEnemy;
            }
            else if (StaticDataManager.gameMode == GameMode.InfinityMode)
            {
                if (stayOrExitController != null && waveNumber % numberOfWaveBeforeQuit == 0)
                {
                    StaticDataManager.isVictory = true;
                    stayOrExitController.ShowStayOrExitMenu();
                }

                StartOtherWave();
            }
        }
    }

    private IEnumerator WaveTimerCoroutine(float duration)
    {
        float elapsedTime = 0f;

        UIController.Instance.SetWaveBar(0f, duration);

        while (elapsedTime < duration)
        {
            UIController.Instance.SetWaveBar(elapsedTime, duration); // Mettre à jour la barre de progression
            elapsedTime += Time.deltaTime; // Utilisation de Time.deltaTime pour une mise à jour plus fluide
            UpdateText($"{Mathf.RoundToInt(duration - elapsedTime)}s before the wave");
            // Debug.Log("WaveTimerCoroutine " + elapsedTime);
            yield return null; // Attendre jusqu'au prochain frame
        }
        waveNumber++;
        if (waveNumber == 1 && StaticDataManager.gameMode == GameMode.Classic)
            StaticDataManager.autoMode = AutoMode.ProtectBase;

        UIController.Instance.SetWaveBar(duration, duration);
        SpawnEnemies(Enemies);
    }

    int CalculateEnemyLevel(int wave)
    {
        // // Définir les niveaux en fonction de la vague
        if (StaticDataManager.gameMode == GameMode.Classic)
        {
            int randomLevel = (int)
                Random.Range(
                    StaticDataManager.minEnemiesLevel * 1.5f,
                    StaticDataManager.maxEnemiesLevel * 1.5f + 1
                );
            return randomLevel;
        }
        else
        {
            int minLevel = Mathf.Max(1, wave * 3 - 2); // Exemple: vague 1 => niveau 1-3, vague 2 => niveau 3-5, etc.
            int maxLevel = wave * 3;
            return Random.Range(minLevel, maxLevel + 1);
        }
    }

    private Vector2 GetRandomPoint()
    {
        if (spawnAreaCollider == null || spawnAreaCollider.Length == 0)
        {
            Debug.LogWarning("No BoxCollider2D components assigned.");
            return Vector2.zero;
        }

        int randInt = Random.Range(0, spawnAreaCollider.Length);

        // Get the bounds of the BoxCollider2D
        BoxCollider2D selectedCollider = spawnAreaCollider[randInt];
        Bounds bounds = selectedCollider.bounds;

        // Generate a random point within the bounds
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        // Convert the point from collider space to world space
        return new Vector2(randomX, randomY);
    }

    private void UpdateText(string text)
    {
        textUnderProgressBarObject.text = text;
    }
}

using TMPro;
using UnityEngine;

public class ExterminationManager : MonoBehaviour
{
    public static ExterminationManager Instance;
    [SerializeField] private TextMeshProUGUI textUnderProgressBarObject;
    private int totalEnemies;
    private int aliveEnemies;

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

        EnemyCounterManager.Instance.OnDeath += EnemyCounterManager_OnDeath;
    }

    public void ShowEnemyBar()
    {
        UIController.Instance.ShowAlertText($"You have entered an enemy area\nkill everybody");
        totalEnemies = EnemyCounterManager.Instance.aliveEnemy;
        aliveEnemies = EnemyCounterManager.Instance.aliveEnemy;
        UIController.Instance.SetWaveBar(aliveEnemies, totalEnemies);
        UpdateText($"{aliveEnemies} alive enemies");
    }

    private void EnemyCounterManager_OnDeath()
    {
        aliveEnemies = EnemyCounterManager.Instance.aliveEnemy;
        UIController.Instance.SetWaveBar(aliveEnemies, totalEnemies);
        UpdateText($"{aliveEnemies} alive enemies");
        if (aliveEnemies <= 0)
        {
            StaticDataManager.isVictory = true;
            UIController.Instance.ShowVictoryScreen();
        }
    }

    private void UpdateText(string text)
    {
        textUnderProgressBarObject.text = text;
    }
}

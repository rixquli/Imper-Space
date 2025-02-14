using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField]
    private GameObject entityGameObject;
    private TurretManager turretManager;

    [SerializeField]
    private GameObject baseShield;
    private bool hasAlreadyShown = false;

    [SerializeField]
    private bool UntaggedOnStart = true;
    [SerializeField] private bool shouldHaveShield = true;
    [SerializeField] private Transform enemyToKillToDeactivateShield;

    private void Awake()
    {
        turretManager = entityGameObject.GetComponent<TurretManager>();
        if (enemyToKillToDeactivateShield != null && enemyToKillToDeactivateShield.TryGetComponent(out IEntity entity))
            entity.OnDeath += () =>
            {
                UIController.Instance.ShowAlertText(
                    $"The Enemy Base Shield has been desactivated \nDestroy it"
                );
                DesactivateShield();
            };
    }

    private void Start()
    {
        if (UntaggedOnStart)
        {
            foreach (Transform child in transform)
            {
                child.tag = "Untagged";
            }
        }
        if (!shouldHaveShield)
        {
            DesactivateShield();
        }
    }

    public void DesactivateShield()
    {
        foreach (Transform child in transform)
        {
            child.tag = "Enemy";
        }
        if (baseShield != null)
            baseShield.SetActive(false);
    }

    private void Update()
    {
        if (!hasAlreadyShown && (turretManager.IsDead || turretManager.IsPlayer))
        {
            hasAlreadyShown = true;
            StaticDataManager.isVictory = true;
            UIController.Instance.ShowVictoryScreen();
        }
    }
}

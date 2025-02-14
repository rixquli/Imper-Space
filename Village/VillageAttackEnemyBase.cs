using UnityEngine;

public class VillageAttackEnemyBase : MonoBehaviour
{
    [SerializeField]
    private GameObject entityGameObject;
    private TurretManager turretManager;

    private bool hasAlreadyShown = false;

    private void Awake()
    {
        turretManager = entityGameObject.GetComponent<TurretManager>();
    }

    void Update()
    {
        if (!hasAlreadyShown && (turretManager.IsDead || turretManager.IsPlayer))
        {
            hasAlreadyShown = true;
            GlobalAttackVillageManager.Instance.EnemyBaseDestroyed();
        }
    }
}

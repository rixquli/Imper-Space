using UnityEngine;

public class PreviewForAttackVillageMode : MonoBehaviour
{
    public GameObject prefab;
    private bool isPlayer;

    public void SetPrefab(GameObject gameObject, bool isPlayer = false)
    {
        prefab = gameObject;
        this.isPlayer = isPlayer;

        GlobalAttackVillageManager.Instance.OnAttackBegin += OnAttackBegin;
    }

    private void OnAttackBegin()
    {
        GameObject troopObject = Instantiate(prefab, transform.position, transform.rotation);
        if (troopObject.TryGetComponent(out IEnemy troopEntity))
        {
            troopEntity.GoToPlayerSide();
        }

        if (isPlayer)
        {
            GlobalAttackVillageManager.Instance.SetPlayerTransform(troopObject.transform);

            // Ensure that GameManager instance is available before starting the game
            AttributesData playerAttributesData = DataPersistenceManager.instance.gameData.playerAttributesData;

            // set the sprite of the plyaer during the game
            Sprite playerSprite = null;
            if (DataPersistenceManager.instance.gameData.equipedEquipements != null && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(EquipeSlot.Ship, out var shipData) && shipData?.icon != null)
            {
                playerSprite = shipData.icon;
                Debug.Log("ship sprite loaded: " + shipData.displayName);
            }

            // set the laser prefab during the game
            GameObject laserPrefab = null;
            if (DataPersistenceManager.instance.gameData.equipedEquipements != null && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(EquipeSlot.Laser, out var laserData) && laserData?.prefab != null)
            {
                laserPrefab = laserData.prefab;
            }

            // set the missile prefab during the game
            GameObject missiePrefab = null;
            if (DataPersistenceManager.instance.gameData.equipedEquipements != null && DataPersistenceManager.instance.gameData.equipedEquipements.TryGetValue(EquipeSlot.Missile, out var missileData) && missileData?.prefab != null)
            {
                missiePrefab = missileData.prefab;
            }

            PlayerEntity playerEntity = troopObject.GetComponent<PlayerEntity>();
            playerEntity.sprite = playerSprite;

            playerEntity.baseAttackDamage = playerAttributesData.attack;
            playerEntity.baseMaxHealth = playerAttributesData.health;
            playerEntity.baseHealthRegenRate = playerAttributesData.regenHealth;
            playerEntity.baseMaxShield = playerAttributesData.shield;
            playerEntity.baseShieldRegenRate = playerAttributesData.regenShield;
            playerEntity.baseSpeed = playerAttributesData.speed;
            playerEntity.baseLaserAttackDamage = playerAttributesData.laserAttribute.attack;
            playerEntity.baseMissileAttackDamage = playerAttributesData.missileAttribute.attack;
            playerEntity.bulletPerSecond = playerAttributesData.bulletPerSecond;

            playerEntity.laserPrefab = laserPrefab;
            playerEntity.missilePrefab = missiePrefab;

            playerEntity.InitializeStats();
        }
        if (troopObject.TryGetComponent(out IEntity entity))
        {
            entity.OnDeath += OnDeath;
        }
        Destroy(gameObject);
    }

    private void OnDeath()
    {
        GlobalAttackVillageManager.Instance.AllyDeath();
    }
}

using System.Collections.Generic;
using UnityEngine;

public class OutPostController : BuildingsBehaviour
{
    [SerializeField]
    private GameObject dronePrefab;
    private List<Transform> companions = new List<Transform>();
    private bool hadSetupLevel = false;
    public override int Level
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            float multiplier = Mathf.Max(Mathf.Pow(1.1f, value - 1), 1);

            float multiplierBaseOnAverageEnemyLevel = Mathf.Max(
                Mathf.Pow(
                    1.1f,
                    (StaticDataManager.minEnemiesLevel + StaticDataManager.maxEnemiesLevel) / 2 - 1
                ),
                1
            );

            maxHealth = baseMaxHealth * multiplier * multiplierBaseOnAverageEnemyLevel;
            health = maxHealth;

            if (((VillagesDataManager.Instance.villageNumber == 0 && hadSetupLevel) || VillagesDataManager.Instance.villageNumber != 0) && ((IsPurchaseable && IsBought) || !IsPurchaseable))
            {
                for (int i = 0; i < Level - companions.Count; i++)
                {
                    AddCompanionToBase(transform);
                }
            }

            UpdateLv();
            if (!hadSetupLevel) hadSetupLevel = true;
        }
    }

    protected override void Update()
    {
        if (healthBarUIObject?.transform)
            healthBarUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 3f);
        if (levelUIObject?.transform)
            levelUIObject.transform.position = (Vector2)transform.position + new Vector2(0, 3.5f);
    }

    public override void Buy()
    {
        base.Buy();
        for (int i = 0; i < Level - companions.Count; i++)
        {
            AddCompanionToBase(transform);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (Transform companion in companions)
        {
            if (companion != null)
                Destroy(companion.gameObject);
        }
    }

    private Transform AddCompanionToBase(Transform spawnPosition)
    {
        GameObject companionObject = Instantiate(
            dronePrefab,
            spawnPosition.position,
            Quaternion.identity
        );
        companions.Add(companionObject.transform);
        CompanionEntity companionEntity = companionObject.GetComponent<CompanionEntity>();

        companionEntity.Initialize(spawnPosition);
        if (IsPlayer)
        {
            companionEntity.GoToPlayerSide();
        }
        else
        {
            companionEntity.GoToEnemySide();
        }
        int randomLevel = Mathf.Clamp(
            1,
            3,
            Random.Range(
                VillagesDataManager.Instance.minEnemiesLevel,
                VillagesDataManager.Instance.maxEnemiesLevel + 1
            )
        );
        companionEntity.Level = randomLevel;

        return companionEntity.transform;
    }

    public override void GoToPlayerSide()
    {
        base.GoToPlayerSide();
        if (VillagesDataManager.Instance.villageNumber != 0 && companions.Count == 0 && ((IsPurchaseable && IsBought) || !IsPurchaseable))
        {
            for (int i = 0; i < Level; i++)
            {
                AddCompanionToBase(transform);
            }
        }
        foreach (Transform companion in companions)
        {
            companion.GetComponent<CompanionEntity>().GoToPlayerSide();
        }
    }

    public override void GoToEnemySide()
    {
        base.GoToEnemySide();
        if (VillagesDataManager.Instance.villageNumber != 0 && companions.Count == 0 && ((IsPurchaseable && IsBought) || !IsPurchaseable))
        {
            for (int i = 0; i < Level; i++)
            {
                AddCompanionToBase(transform);
            }
        }
        foreach (Transform companion in companions)
        {
            companion.GetComponent<CompanionEntity>().GoToEnemySide();
        }
    }
}

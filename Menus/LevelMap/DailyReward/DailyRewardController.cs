using System;
using BattlePass;
using NUnit.Framework;
using UnityEngine;

public class DailyRewardController : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    private DailyRewards dailyRewards;

    [SerializeField]
    private GameObject dailyRewardCardPrefab;
    [SerializeField]
    private GameObject finalDailyRewardCardPrefab;

    [SerializeField]
    private Transform dailyRewardCardParent;
    [SerializeField]
    private Transform finalDailyRewardCardParent;

    [SerializeField]
    private TutorialHelper tutorialHelper;

    private const string LastMenuShownKey = "LastDailyRewardMenuShown";

    private void Awake()
    {
        tutorialHelper.blockTuto = true;
    }

    private void Start()
    {
        // Veirifie si le joueur a loupe une journee
        dailyRewards.CheckForMissedDays();

        // Vérifier si le menu a déjà été affiché aujourd'hui
        if (HasMenuBeenShownToday())
        {
            // Si c'est le cas, ne rien faire
            HideMenu();
            return;
        }

        // Afficher le menu
        ShowMenu();
    }

    private void ShowMenu()
    {
        DeleteChildsObject(dailyRewardCardParent);
        DeleteChildsObject(finalDailyRewardCardParent);
        if (dailyRewards == null || dailyRewards.rewards.Count <= 0)
            return;

        for (int i = 0; i < dailyRewards.rewards.Count; i++)
        {
            // Créer une variable temporaire pour capturer l'index actuel
            int rewardIndex = i;

            // Créer une instance de la carte de récompense quotidienne
            GameObject dailyRewardCard;
            if (i == dailyRewards.rewards.Count - 1) // si c'est le dernier donc le final
            {
                dailyRewardCard = Instantiate(finalDailyRewardCardPrefab, finalDailyRewardCardParent);
            }
            else
            {
                dailyRewardCard = Instantiate(dailyRewardCardPrefab, dailyRewardCardParent);
            }

            // Récupérer le composant DailyRewardCard
            DailyRewardCard dailyRewardCardComponent =
                dailyRewardCard.GetComponent<DailyRewardCard>();

            // Initialiser la carte avec les données de récompense quotidienne
            if (
                dailyRewards.rewards[rewardIndex].rewardType == BattlePassRewardType.Equipement
                && dailyRewards.rewards[rewardIndex].equipement != null
            )
                dailyRewardCardComponent.Initialize(
                    dailyRewards.rewards[rewardIndex].equipement,
                    dailyRewards.CanClaimReward(rewardIndex),
                    dailyRewards.rewards[rewardIndex].isClaimed,
                    rewardIndex + 1
                );
            else if (
                dailyRewards.rewards[rewardIndex].rewardType == BattlePassRewardType.Ressource
                && dailyRewards.rewards[rewardIndex].ressource != null
            )
                dailyRewardCardComponent.Initialize(
                    dailyRewards.rewards[rewardIndex].ressource,
                    dailyRewards.CanClaimReward(rewardIndex),
                    dailyRewards.rewards[rewardIndex].isClaimed,
                    rewardIndex + 1
                );

            // Supprimer les anciens listeners et ajouter un nouveau listener avec la bonne référence d'index
            dailyRewardCardComponent.claimButton.onClick.RemoveAllListeners();
            dailyRewardCardComponent.claimButton.onClick.AddListener(() =>
            {
                if (HandleClaimButtonClick(dailyRewards.rewards[rewardIndex]))
                {
                    dailyRewardCardComponent.SetClaimed(true);
                    dailyRewardCardComponent.StartClaimAnim();
                    Invoke(nameof(HideMenu), 1f);
                }
            });
        }
    }

    private bool HandleClaimButtonClick(DailyReward dailyReward)
    {
        // Vérifiez si le niveau a été atteint et non réclamé
        if (
            dailyReward.isClaimed
            || !dailyRewards.CanClaimReward(dailyRewards.rewards.IndexOf(dailyReward))
        )
        {
            Debug.LogWarning("Level not eligible for claiming or already claimed.");
            return false;
        }

        // Traitement pour les récompenses de Free Pass
        switch (dailyReward.rewardType)
        {
            case BattlePass.BattlePassRewardType.Equipement:
                HandleReward(dailyReward.equipement);
                break;
            case BattlePass.BattlePassRewardType.Ressource:
                HandleReward(dailyReward.ressource);
                break;
            default:
                Debug.LogWarning("Unknown reward type for Free Pass.");
                break;
        }

        dailyRewards.LastClaimedDay = DateTime.Now.Date;
        dailyReward.isClaimed = true;
        Debug.Log("Reward claimed for dailyreward: " + dailyReward);
        return true;
    }

    private void HandleReward(object reward)
    {
        // Exemple de traitement des récompenses
        // Ici, vous pouvez ajouter la logique pour traiter l'équipement ou la ressource
        if (reward is Equipement equipement)
        {
            RessourcesManager.Instance.AddEquipements(equipement.equipement);
        }
        else if (reward is Ressource ressource)
        {
            switch (ressource.ressourceType)
            {
                case RessourceType.Gold:
                    RessourcesManager.Instance.AddGold(ressource.amount);
                    break;
                case RessourceType.Gem:
                    RessourcesManager.Instance.AddGems(ressource.amount);
                    break;
                default:
                    Debug.LogWarning("Unknown reward type for Free Pass.");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Unknown reward type.");
        }

        RessourcesManager.Instance.StartWaitForReloading();
    }

    private void DeleteChildsObject(Transform parentObject)
    {
        if (parentObject != null && parentObject.transform.childCount > 0)
        {
            // Get the first child
            foreach (Transform child in parentObject.transform)
            {
                Destroy(child.gameObject);
            }
            // Destroy the first child GameObject
        }
    }

    public bool HasMenuBeenShownToday()
    {
        for (int i = 0; i < dailyRewards.rewards.Count; i++)
        {
            if (dailyRewards.CanClaimReward(i))
            {
                return false;
            }
        }

        // Comparer avec la date actuelle
        return true;
    }

    private void HideMenu()
    {
        tutorialHelper.blockTuto = false;
        DataPersistenceManager.instance.SaveGame();
        RessourcesManager.Instance.StartWaitForReloading();
        gameObject.SetActive(false);
    }

    public void LoadData(GameData data)
    {
        if (data.dailyRewards.rewards == null || data.dailyRewards.rewards.Count == 0)
        {
            data.dailyRewards.rewards = dailyRewards.rewards;
        }
        else
            dailyRewards = data.dailyRewards;
    }

    public void SaveData(GameData data)
    {
        data.dailyRewards = dailyRewards;
    }
}

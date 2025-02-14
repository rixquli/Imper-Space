using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackUIController : MonoBehaviour
{
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private GameObject troopsToPlaceCardParent;

    [SerializeField]
    private GameObject troopsToPlaceCardPrefab;

    private List<TroopsCard> troopsCards = new();

    private void Awake()
    {
        startButton.onClick.AddListener(StartAttack);
        GlobalAttackVillageManager.Instance.OnTroopsSetup += SetupTroopsCard;
    }

    private void StartAttack()
    {
        GlobalAttackVillageManager.Instance.StartAttack(() =>
        {
            Hide();
        });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetupTroopsCard()
    {
        Debug.LogError("SetupTroopsCard");
        foreach (TroopsToPlace troopToPlace in GlobalAttackVillageManager.Instance.troopsToPlaces)
        {
            GameObject troopObject = Instantiate(
                troopsToPlaceCardPrefab,
                troopsToPlaceCardParent.transform
            );

            if (
                troopObject.TryGetComponent<TroopsCard>(out TroopsCard troopsCard)
                && troopsCard != null
            )
            {
                troopToPlace.troopsCard = troopsCard;

                Debug.Log(troopsCard);
                Debug.Log(troopToPlace.troopsData.displayName);

                troopsCard.Init(
                    troopToPlace,
                    troopToPlace.troopsData.image,
                    troopToPlace.troopsData.displayName,
                    troopToPlace.amount
                );
                troopsCards.Add(troopsCard);

                if (troopObject.TryGetComponent<Button>(out Button button))
                {
                    button.onClick.AddListener(() => SelectTroopsCard(troopsCard));
                }
            }
        }
    }

    private void SelectTroopsCard(TroopsCard troopsCard)
    {
        foreach (var item in troopsCards)
        {
            if (item == troopsCard)
            {
                GlobalAttackVillageManager.Instance.selectedTroop = item.data;
                item.OnSelected();
            }
            else
            {
                item.OnUnselected();
            }
        }
    }
}

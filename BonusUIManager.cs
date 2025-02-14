using UnityEngine;

public class BonusUIManager : MonoBehaviour
{
    private BonusUIElement[] bonusUIElements;

    private void Awake()
    {
        bonusUIElements = GetComponentsInChildren<BonusUIElement>();
    }

    public void SetPlayerEntity(PlayerEntity playerEntity)
    {
        foreach (var bonusUIElement in bonusUIElements)
        {
            bonusUIElement.SetPlayerEntity(playerEntity);
        }
    }
}

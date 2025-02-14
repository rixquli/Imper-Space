using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassLevelUIEffect : MonoBehaviour
{
    [SerializeField] private DynamicProgressBarEffect progressBarEffect;
    [SerializeField] private DynamicProgressBarEffect alreadyHaveXpProgressBarEffect;
    [SerializeField] private TextMeshProUGUI levelText;

    public Action OnFinished;

    private void Start()
    {
        progressBarEffect.OnBarFull += () =>
        {
            if (int.TryParse(levelText.text, out int currentLevel))
            {
                levelText.text = (currentLevel + 1).ToString();
            }
            alreadyHaveXpProgressBarEffect.ResetProgress();
        };

        levelText.text = BattlePassManager.BattlePassLevel.ToString();
    }

    public void StartAnimation()
    {
        float alreadyHaveXP = BattlePassManager.CurrentBattlePassXpOnCurrentLevel / BattlePassManager.xpPerBattlePassLevel;
        alreadyHaveXpProgressBarEffect.SetProgress(alreadyHaveXP);
        float progress = 0;
        int nextLevel = BattlePassManager.BattlePassLevel + 1;
        while (true)
        {
            var xpForNextLevel = BattlePassManager.GetXpForLevel(nextLevel);
            var totalXp = BattlePassManager.battlePassXp + XPManager.Instance.Xp;
            var temp = totalXp % xpForNextLevel;
            if (temp > 1)
            {
                progress += 1;
                nextLevel += 1;
            }
            else
            {
                progress += temp;
                break;
            }
        }

        progressBarEffect.SetProgress(progress, OnFinished);
    }
}

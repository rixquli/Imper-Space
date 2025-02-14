using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DashEnergyController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Header("Separating Bars")]
    [SerializeField] private GameObject[] bars;
    [Header("Dash Energy")]
    [SerializeField] private Image filledBar;
    [SerializeField] private float dashEnergy = 100f;
    private int availableDash;
    private int currentDash;

    public void Setup(int availabeDash)
    {
        this.availableDash = availabeDash;
        currentDash = availableDash;
        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].SetActive(i == availabeDash - 1);
        }
    }

    public void SetEnergyBar(float value, float duration = 0.3f)
    {
        float targetFill = value / dashEnergy;
        int forCurentDash = Mathf.CeilToInt((targetFill * 100 + 0.0001f) / (dashEnergy / availableDash));
        if (targetFill > filledBar.fillAmount) // 40 -> 50 
        {
            if (forCurentDash > currentDash && forCurentDash > 0)
            {
                PlayDashAnimation();
            }
        }
        else if (targetFill < filledBar.fillAmount) // 50 -> 0
        {
            PlayDashAnimation();
        }
        currentDash = forCurentDash;
        filledBar.DOFillAmount(targetFill, duration).SetEase(Ease.OutQuad);
    }

    public void PlayDashAnimation()
    {
        animator.Play("filledbar");
    }
}

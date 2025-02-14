using TMPro;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DynamicAddingTextEffect : MonoBehaviour
{
    private TextMeshProUGUI text;
    public float duration = 1f; // Duration of the animation
    public float punchScale = 1.2f; // How much the text scales during punch effect
    public float targetedNumber = 0;
    public float currentNumber = 0;

    private Tween numberTween;
    private Tween scaleTween;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        // Only trigger animation when numbers are different
        if (Mathf.Abs(currentNumber - targetedNumber) > 0.01f)
        {
            AnimateNumberChange();
        }
    }

    public void SetNumber(float number)
    {
        targetedNumber = number;
        if (number == 0)
        {
            currentNumber = 0;
            text.text = "0";
            // Add punch scale effect with return to original scale
            scaleTween = transform
                .DOPunchScale(Vector3.one * (punchScale - 1f), duration * 0.5f, 1, 0.5f)
                .OnComplete(() => transform.localScale = Vector3.one);
        }
    }

    private void AnimateNumberChange()
    {
        // Kill previous tweens if they exist
        numberTween?.Kill();
        scaleTween?.Kill();

        // Reset scale before new animation
        transform.localScale = Vector3.one;

        // Animate number
        numberTween = DOTween.To(() => currentNumber, x =>
        {
            currentNumber = x;
            text.text = Mathf.RoundToInt(currentNumber).ToString();
        }, targetedNumber, duration)
        .SetEase(Ease.OutQuad);

        // Add punch scale effect with return to original scale
        scaleTween = transform
            .DOPunchScale(Vector3.one * (punchScale - 1f), duration * 0.5f, 1, 0.5f)
            .OnComplete(() => transform.localScale = Vector3.one);
    }

    private void OnDestroy()
    {
        numberTween?.Kill();
        scaleTween?.Kill();
    }
}
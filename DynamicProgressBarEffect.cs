using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DynamicProgressBarEffect : MonoBehaviour
{
    private Image progressBar;
    public float speed = 0.1f;
    public float targetedProgress = 0f;
    public float currentProgress = 0f;

    public event Action OnBarFull;

    private Action onFinished;
    private bool isFinished;

    private void Awake()
    {
        progressBar = GetComponent<Image>();
    }

    private void Update()
    {
        if (currentProgress < targetedProgress)
        {
            currentProgress += speed * Time.deltaTime;
            if (currentProgress > 1f)
            {
                currentProgress -= 1;
                targetedProgress -= 1;
                OnBarFull?.Invoke();
            }
            progressBar.fillAmount = currentProgress;
        }
        else if (!isFinished)
        {
            isFinished = true;
            onFinished?.Invoke();
        }
    }

    public void SetProgress(float progress, Action onFinished = null)
    {
        this.onFinished = onFinished;
        targetedProgress = progress;
        currentProgress = 0f;
    }

    public void ResetProgress()
    {
        currentProgress = 0f;
        targetedProgress = 0f;
        progressBar.fillAmount = 0f;
    }
}

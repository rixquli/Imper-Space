using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private Button button;
    public AudioClip buttonSoundFX;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayButtonSoundFX);
    }

    private void PlayButtonSoundFX()
    {
        SoundFXManager.Instance.PlaySoundFXClip(buttonSoundFX, transform, 1.5f);
    }
}

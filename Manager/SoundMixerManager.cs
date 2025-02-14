using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public enum SoundMixers
{
    Master,
    FX,
    Music
}

public class SoundMixerManager : MonoBehaviour, IDataPersistence
{
    public static SoundMixerManager Instance;
    [SerializeField] private AudioMixer audioMixer;

    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float fxVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    public UDictionary<SoundMixers, Slider> sliders = new();

    public Slider fxVolumeSlider;
    public Slider musicVolumeSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetAllVolume();
        UpdateSliders();
    }

    public void SetNewSlider(UDictionary<SoundMixers, Slider> sliders)
    {
        foreach (SoundMixers mixer in sliders.Keys)
        {
            Slider newSlider = sliders[mixer];
            this.sliders[mixer] = newSlider;

            switch (mixer)
            {
                case SoundMixers.Master:
                    newSlider.onValueChanged.AddListener(SetMasterVolume);
                    break;
                case SoundMixers.FX:
                    newSlider.onValueChanged.AddListener(SetSoundFXVolume);
                    break;
                case SoundMixers.Music:
                    newSlider.onValueChanged.AddListener(SetMusicVolume);
                    break;

            }
        }
        SetAllVolume();
        UpdateSliders();
    }

    private void SetAllVolume()
    {
        SetMasterVolume(masterVolume);
        SetSoundFXVolume(fxVolume);
        SetMusicVolume(musicVolume);
    }

    private void UpdateSliders()
    {
        // Update Master Volume Slider
        if (sliders.ContainsKey(SoundMixers.Master) && sliders.TryGetValue(SoundMixers.Master, out Slider masterVolumeSlider))
        {
            masterVolumeSlider.value = masterVolume;
        }

        // Update FX Volume Slider
        if (sliders.ContainsKey(SoundMixers.FX) && sliders.TryGetValue(SoundMixers.FX, out Slider fxVolumeSlider))
        {
            fxVolumeSlider.value = fxVolume;
        }

        // Update Music Volume Slider
        if (sliders.ContainsKey(SoundMixers.Music) && sliders.TryGetValue(SoundMixers.Music, out Slider musicVolumeSlider))
        {
            musicVolumeSlider.value = musicVolume;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0.001f, 1f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20f);
    }

    public void SetSoundFXVolume(float volume)
    {
        fxVolume = Mathf.Clamp(volume, 0.001f, 1f);
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(fxVolume) * 20f);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0.001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20f);
    }

    public void LoadData(GameData data)
    {
        if (data.mixerVolumes != null)
        {
            SetMasterVolume(data.mixerVolumes[SoundMixers.Master]);
            SetSoundFXVolume(data.mixerVolumes[SoundMixers.FX]);
            SetMusicVolume(data.mixerVolumes[SoundMixers.Music]);
        }
        UpdateSliders();
    }

    public void SaveData(GameData data)
    {
        if (data.mixerVolumes == null)
        {
            data.mixerVolumes = new UDictionary<SoundMixers, float>();
        }

        data.mixerVolumes[SoundMixers.Master] = masterVolume;
        data.mixerVolumes[SoundMixers.FX] = fxVolume;
        data.mixerVolumes[SoundMixers.Music] = musicVolume;
    }
}

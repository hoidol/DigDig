using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingCanvas : MonoBehaviour
{
    public AudioMixer audioMixer;

    // public Slider masterVolumeSlider;
    public Slider sFXVolumeSlider;
    public Slider bgmVolumeSlider;

    void Awake()
    {
        // masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sFXVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
    }

    void Start()
    {
        // masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioVolumeUtil.MasterVolumeKey, 1f));
        sFXVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioVolumeUtil.SFXVolumeKey, 1f));
        bgmVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioVolumeUtil.BGMVolumeKey, 1f));
    }

    void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(AudioVolumeUtil.MasterVolumeKey, value);
        AudioVolumeUtil.Apply(audioMixer, AudioVolumeUtil.MasterVolumeParam, value);
    }

    void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(AudioVolumeUtil.SFXVolumeKey, value);
        AudioVolumeUtil.Apply(audioMixer, AudioVolumeUtil.SFXVolumeParam, value);
    }

    void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(AudioVolumeUtil.BGMVolumeKey, value);
        AudioVolumeUtil.Apply(audioMixer, AudioVolumeUtil.BGMVolumeParam, value);
    }
}

using UnityEngine;
using UnityEngine.Audio;

// AudioMixer의 Exposed Parameter 이름, PlayerPrefs 키, 선형 볼륨(0~1) <-> dB 변환을 한 곳에서 관리한다.
public static class AudioVolumeUtil
{
    public const string MasterVolumeParam = "MasterVolume";
    public const string BGMVolumeParam = "BGMVolume";
    public const string SFXVolumeParam = "SFXVolume";

    public const string MasterVolumeKey = "MasterVolume";
    public const string BGMVolumeKey = "BGMVolume";
    public const string SFXVolumeKey = "SFXVolume";

    // 설정 창 음소거 스위치 (1 = 음소거). 볼륨 값은 그대로 두고 적용할 때만 0으로
    public const string BGMMuteKey = "BGMMute";
    public const string SFXMuteKey = "SFXMute";

    const float MinDb = -80f;

    public static bool IsMuted(string muteKey) => PlayerPrefs.GetInt(muteKey, 0) == 1;

    public static void SetMuted(string muteKey, bool mute)
    {
        PlayerPrefs.SetInt(muteKey, mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 실제로 믹서에 넣을 볼륨 (음소거면 0)
    public static float GetAppliedVolume(string volumeKey, string muteKey)
    {
        return IsMuted(muteKey) ? 0f : PlayerPrefs.GetFloat(volumeKey, 1f);
    }

    // AudioMixer는 선형 볼륨이 아니라 dB 단위를 받기 때문에 슬라이더 값(0~1)을 변환해서 적용한다.
    public static void Apply(AudioMixer mixer, string param, float linearVolume)
    {
        if (mixer == null)
            return;

        float dB = linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : MinDb;
        mixer.SetFloat(param, dB);
    }

    public static void ApplyAllFromPlayerPrefs(AudioMixer mixer)
    {
        Apply(mixer, MasterVolumeParam, PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        Apply(mixer, BGMVolumeParam, GetAppliedVolume(BGMVolumeKey, BGMMuteKey));
        Apply(mixer, SFXVolumeParam, GetAppliedVolume(SFXVolumeKey, SFXMuteKey));
    }
}

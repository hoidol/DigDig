using UnityEngine;

[CreateAssetMenu(menuName = "SoundData", fileName = "SoundData")]
public class SoundData : ScriptableObject
{
    public SFXData[] sFXDatas;
    public SFXData[] bgms;
}
[System.Serializable]
public class SFXData //사운드 관련 데이터
{
    [HideInInspector] public string name; // 인스펙터 칸 이름 (= sfxType, SoundManager.OnValidate가 채움)
    public SFXType sfxType;
    public AudioClip audioClip;
    [Range(0, 1)] public float volume = 1;
    public float pitch = 1;
    [Tooltip("재생할 때마다 pitch ± 이 값 안에서 랜덤 (자주 나오는 소리가 덜 단조롭게)")]
    public float pitchRandom = 0;
    [Tooltip("같은 소리를 이 시간(초) 안에 다시 재생하면 무시 (적중·코인처럼 한꺼번에 나는 소리)")]
    public float minInterval = 0;
    public bool loop = false;
    public bool playOnAwake = false;
}
[System.Serializable]
public class BGMData
{
    [HideInInspector] public string name; // 인스펙터 칸 이름 (= bgmType)
    public BGMType bgmType;
    public AudioClip audioClip;
    [Range(0, 1)] public float volume = 1;
    public bool loop = true;
}

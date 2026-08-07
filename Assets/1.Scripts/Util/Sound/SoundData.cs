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
    public SFXType sfxType;
    public AudioClip audioClip;
    public float volume;
    public bool loop;
    public bool playOnAwake;
}

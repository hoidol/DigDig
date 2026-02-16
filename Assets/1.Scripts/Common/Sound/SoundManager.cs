using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SoundMgr : MonoSingleton<SoundMgr>
{
    [SerializeField] SFXData[] sfxDatas;
    [SerializeField] BGMPlayer[] bgmPlayers;
    
    [SerializeField] AudioSource[] audioSources;


    void Start()
    {
        bool mute = PlayerPrefs.GetInt("Mute", 0) == 1; // 1이면 음소거 0이면 아
        //Debug.Log($"mute {mute}");
        SetMute(mute);

        audioSources = new AudioSource[30];
        bgmPlayers = GetComponentsInChildren<BGMPlayer>();
        for (int i = 0; i < 30; i++)
        {
            //AudioSource 컴포넌트 생성하기
            audioSources[i] = gameObject.AddComponent<AudioSource>();
        }
        PlayBGM(BGMType.Main);
    }

    //사운드 실행하기
    public void PlaySound(SFXType _sType)
    {
        SFXData sFXData = GetSoundData(_sType);
        if (sFXData == null)
            return;

        AudioSource _aSource = GetAudioSource();

        _aSource.clip = sFXData.audioClip;
        _aSource.volume = sFXData.volume;
        _aSource.loop = sFXData.loop;
        _aSource.playOnAwake = sFXData.playOnAwake;

        _aSource.Play();
    }

    public void PlayBGM(BGMType bGMType)
    {
        for(int i =0;i< bgmPlayers.Length; i++)
        {
            if(bgmPlayers[i].bgmType == bGMType)
            {
                if(!bgmPlayers[i].audioSource.isPlaying)
                    bgmPlayers[i].audioSource.Play();
            }
            else
            {
                bgmPlayers[i].audioSource.Stop();
            }
        }
    }

    public void SetMute(bool b)
    {
        if (b)
        {
            PlayerPrefs.SetInt("Mute", 1); // 1이면 음소거 0이면 아
            AudioListener.volume = 0;
        }
        else
        {
            PlayerPrefs.SetInt("Mute", 0); // 소리켜기
            AudioListener.volume = 1;
        }
    }
    //사운드 데이터 가져오기
    SFXData GetSoundData(SFXType _sType)
    {
        for (int i = 0; i < sfxDatas.Length; i++)
        {
            if (sfxDatas[i].sfxType.Equals(_sType))
            {
                return sfxDatas[i];
            }
        }
        return null;
    }

    //사용하지 않은 AudioSource 컴포넌트 가져오기
    AudioSource GetAudioSource()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i].isPlaying)
                continue;
            return audioSources[i];
        }

        return audioSources.OrderBy(e =>
        {
            return e.time / e.clip.length;
        }).FirstOrDefault();
    }
}

public enum SFXType //사운드 종류
{
    Click,
    Upgrade,
    SetUp,
    StopSpin,
    GetGold,
    Jump,
    Beep,
    Reward,
    ViewOpen, //창 열릴때
    //Clear, //
        Money_Charge,//동전 많이 받기
        New,
        PutOre,

    //CannonExplosion
}
public enum BGMType //사운드 종류
{
    Main,
    Challenge,
    //CannonExplosion
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
//[System.Serializable]
//public class BGMData //사운드 관련 데이터
//{
//    public BGMType bgmType;
//    public AudioClip audioClip;
//    public float volume;
//}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

// 사운드 파일은 프리팹(Resources/Init/SoundManager)의 sfxDatas / bgmDatas 칸에 드래그해서 넣는다.
// 칸은 enum 순서대로 자동으로 하나씩 만들어진다(OnValidate). 클립이 비어 있는 종류는 재생해도 소리 없음.
public class SoundManager : MonoSingleton<SoundManager>
{
    const int SFX_SOURCE_COUNT = 30;

    [SerializeField] SFXData[] sfxDatas;
    [SerializeField] BGMData[] bgmDatas;

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField] AudioMixerGroup bgmGroup;

    AudioSource[] audioSources;
    AudioSource bgmSource;
    BGMType? currentBGM;

    readonly Dictionary<SFXType, SFXData> sfxDic = new Dictionary<SFXType, SFXData>();
    readonly Dictionary<AudioClip, float> lastPlayTimes = new Dictionary<AudioClip, float>();

    void Start()
    {
        bool mute = PlayerPrefs.GetInt("Mute", 0) == 1; // 1이면 음소거 0이면 아
        SetMute(mute);

        AudioVolumeUtil.ApplyAllFromPlayerPrefs(audioMixer);

        if (sfxDatas != null)
            foreach (var data in sfxDatas)
                if (data != null && !sfxDic.ContainsKey(data.sfxType))
                    sfxDic.Add(data.sfxType, data);

        audioSources = new AudioSource[SFX_SOURCE_COUNT];
        for (int i = 0; i < SFX_SOURCE_COUNT; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].outputAudioMixerGroup = sfxGroup;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.outputAudioMixerGroup = bgmGroup;

        PlayBGM(BGMType.Lobby);
    }

    // 저장된 볼륨·음소거 설정을 믹서에 다시 적용 (설정 창에서 바꿨을 때)
    public void ApplyVolumes()
    {
        AudioVolumeUtil.ApplyAllFromPlayerPrefs(audioMixer);
    }

    //사운드 실행하기
    public void PlaySound(SFXType _sType)
    {
        if (sfxDic.TryGetValue(_sType, out SFXData sFXData))
            Play(sFXData);
    }

    // 개별 데이터(미니언 공격 소리 등)를 재생. 클립이 없으면 공통 소리(fallback)
    public void PlaySound(SFXData sFXData, SFXType fallback)
    {
        if (sFXData != null && sFXData.audioClip != null)
            Play(sFXData);
        else
            PlaySound(fallback);
    }

    void Play(SFXData sFXData)
    {
        if (audioSources == null || sFXData == null || sFXData.audioClip == null)
            return;

        // 자주 나오는 소리(적중·코인 등)가 한꺼번에 겹치지 않게 최소 간격 (같은 클립 기준)
        if (sFXData.minInterval > 0)
        {
            if (lastPlayTimes.TryGetValue(sFXData.audioClip, out float last) && Time.unscaledTime - last < sFXData.minInterval)
                return;
            lastPlayTimes[sFXData.audioClip] = Time.unscaledTime;
        }

        AudioSource _aSource = GetAudioSource();

        _aSource.clip = sFXData.audioClip;
        _aSource.volume = sFXData.volume;
        _aSource.pitch = sFXData.pitch + Random.Range(-sFXData.pitchRandom, sFXData.pitchRandom);
        _aSource.loop = sFXData.loop;

        _aSource.Play();
    }

    // loop 소리(지옥불·레이저 등)를 끌 때
    public void StopSound(SFXType _sType)
    {
        if (audioSources == null || !sfxDic.TryGetValue(_sType, out SFXData sFXData) || sFXData.audioClip == null)
            return;
        foreach (var source in audioSources)
            if (source.isPlaying && source.clip == sFXData.audioClip)
                source.Stop();
    }

    public void PlayBGM(BGMType bGMType)
    {
        if (bgmSource == null)
            return;
        if (currentBGM == bGMType && bgmSource.isPlaying)
            return;

        BGMData data = bgmDatas?.FirstOrDefault(e => e != null && e.bgmType == bGMType);
        currentBGM = bGMType;
        bgmSource.Stop();
        if (data == null || data.audioClip == null)
            return;

        bgmSource.clip = data.audioClip;
        bgmSource.volume = data.volume;
        bgmSource.loop = data.loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        currentBGM = null;
        if (bgmSource != null)
            bgmSource.Stop();
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

    //사용하지 않은 AudioSource 컴포넌트 가져오기 (모두 사용 중이면 가장 많이 진행된 것)
    AudioSource GetAudioSource()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i].isPlaying)
                continue;
            return audioSources[i];
        }

        AudioSource oldest = audioSources.Where(e => !e.loop).OrderByDescending(e => e.clip == null ? 1f : e.time / e.clip.length).FirstOrDefault();
        return oldest != null ? oldest : audioSources[0];
    }

#if UNITY_EDITOR
    // enum 종류마다 칸 하나씩, enum 순서대로 맞춘다 (이미 넣은 클립·값은 유지)
    void OnValidate()
    {
        sfxDatas = SyncSlots(sfxDatas, e => e.sfxType, t => new SFXData { sfxType = t }, (d, n) => d.name = n);
        bgmDatas = SyncSlots(bgmDatas, e => e.bgmType, t => new BGMData { bgmType = t }, (d, n) => d.name = n);
    }

    static TData[] SyncSlots<TEnum, TData>(TData[] datas, System.Func<TData, TEnum> getType, System.Func<TEnum, TData> create, System.Action<TData, string> setName)
        where TEnum : System.Enum where TData : class
    {
        var exist = new Dictionary<TEnum, TData>();
        if (datas != null)
            foreach (var d in datas)
                if (d != null && !exist.ContainsKey(getType(d)))
                    exist.Add(getType(d), d);

        var result = new List<TData>();
        foreach (TEnum type in System.Enum.GetValues(typeof(TEnum)))
        {
            if (!exist.TryGetValue(type, out TData data))
                data = create(type);
            setName(data, type.ToString());
            result.Add(data);
        }
        return result.ToArray();
    }
#endif
}

// 저장(직렬화)은 번호로 되므로 새 종류는 맨 뒤에만 추가한다.
public enum SFXType
{
    Click,

    // UI 공통
    TabChange, PopupOpen, Fail_NotEnough,

    // 인게임 핵심 루프
    MinionSpawnButton, MinionAppear, DragDrop, MinionSwap, MinionMerge,
    CoinGet, DiceCoinGet,

    // 확률·강화
    DrawItemRoll, DrawItemSuccess, ItemInstall, ItemSell,
    TileItemUpgradeSuccess, TileItemUpgradeFail, Reinforce, Evolve,

    // 전투 공통
    MinionAttack, // 미니언 공통 공격 소리 (MinionData.attackSfx에 클립이 없을 때)
    HitPhysic, HitMagic, HitWeak, HitResist, HitCritical,
    EnemyDead, EliteDead, BossDead, KingDamaged, KingLowHp,
    StatusSlow, StatusFreeze, StatusStun, StatusBurn, StatusStigma, StatusMud,

    // 미니언 스킬
    Thunder, LightningChain, FlameSkill, IceHit, HailSpawn, Bounce, OrbitHit, ShieldBlock,
    MineInstall, MineExplosion, MortarFire, MortarExplosion, InfernoLoop, BeamLoop,
    BerserkerHit, BerserkerStack, HunterSnipe, StigmaSkill, PowerUp,
    JokerCardDraw, JokerSpade, JokerDiamond, JokerClover, JokerHeart,
    DarknessOpen, DarknessTransform, GrowUp, SacrificeMerge, BuffReceive,

    // wave / 진행
    WaveStart, EliteAppear, BossAppear, NextWaveButton, AuthorityTrigger, TimeStop,
    Dead, Resurrection, RewardGet, RewardDouble,

    // 로비
    ChestOpen, CardFlip, CardShakeHighGrade, CardRevealHighGrade,
    MinionLevelUp, KingUpgrade, UserLevelUp, ExpBarFill, AuthoritySelect, AchievementReward, PurchaseComplete,
    TutorialNext,
}

public enum BGMType
{
    Lobby,
    InGame,
}

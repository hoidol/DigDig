using System;
using UnityEngine;

// (2026-11-20) 게임에서 쓰는 미니언과 지급 시점. Resources/MinionUnlockSetting 에셋 인스펙터에서 직접 수정
// - initMinionKeys: 처음부터 보유·장착 (장착 칸 수 = GameSetting.MINION_SLOT_COUNT만큼 필요)
// - levelUnlocks: 유저 레벨(표시 LV.N)이 되면 로비에서 구매 가능 (UserMinionManager.BuyMinion, 2026-11-24 자동 지급 → 구매)
// 두 목록에 없는 미니언은 게임에서 안 씀 (뽑기·미니언 관리 화면에서 제외, GameSetting.IsUsedMinion)
// 에셋이 없으면 코드 기본값(DEFAULT_*)
[CreateAssetMenu(fileName = "MinionUnlockSetting", menuName = "KingSlime/MinionUnlockSetting")]
public class MinionUnlockSetting : ScriptableObject
{
    public const string RESOURCE_PATH = "MinionUnlockSetting";

    [Serializable]
    public struct LevelUnlock
    {
        [Tooltip("이 유저 레벨(표시 LV.N)이 되면 지급")]
        public int level;
        [Tooltip("미니언 key (MinionData.key)")]
        public string key;
        public LevelUnlock(int level, string key) { this.level = level; this.key = key; }
    }

    [Tooltip("처음부터 보유·장착하는 미니언 key (장착 칸 순서)")]
    public string[] initMinionKeys = DEFAULT_INIT_KEYS;
    [Tooltip("레벨 지급 (위에서부터 표시 순서). 권능 해금 레벨(5·10…)과 겹치지 않게")]
    public LevelUnlock[] levelUnlocks = DEFAULT_LEVEL_UNLOCKS;

    public static readonly string[] DEFAULT_INIT_KEYS = { "Flame", "Ice", "Random", "Orbit", "Lightning" };
    public static readonly LevelUnlock[] DEFAULT_LEVEL_UNLOCKS =
    {
        new LevelUnlock(1, "Bounce"),
        new LevelUnlock(2, "Iron"),
        new LevelUnlock(3, "Pierce"),
        new LevelUnlock(4, "Mine"),
        new LevelUnlock(6, "Berserker"),
        new LevelUnlock(7, "Mortar"),
        new LevelUnlock(8, "Thunder"),
    };

    static MinionUnlockSetting instance;
    public static MinionUnlockSetting Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<MinionUnlockSetting>(RESOURCE_PATH);
            return instance;
        }
    }

    public static string[] InitKeys
    {
        get
        {
            var s = Instance;
            return s != null && s.initMinionKeys != null && s.initMinionKeys.Length > 0 ? s.initMinionKeys : DEFAULT_INIT_KEYS;
        }
    }

    public static LevelUnlock[] LevelUnlocks
    {
        get
        {
            var s = Instance;
            return s != null && s.levelUnlocks != null ? s.levelUnlocks : DEFAULT_LEVEL_UNLOCKS;
        }
    }
}

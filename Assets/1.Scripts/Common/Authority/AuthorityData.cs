using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 권능: 인게임 특수 능력 (스탯 없음, 부위 없음). 묶음마다 1개 선택, 선택한 것 전부 적용 (CLAUDE.md "권능")
[CreateAssetMenu(fileName = "AuthorityData", menuName = "Data/AuthorityData")]
public class AuthorityData : ScriptableObject
{
    // 묶음(tier) 1~6. 유저 레벨이 해금 레벨에 도달하면 그 묶음의 권능 3개 중 1개를 고른다
    public static readonly int[] TIER_UNLOCK_LEVELS = { 5, 10, 15, 20, 25, 30 };
    public const int AUTHORITIES_PER_TIER = 3;
    public static int TierCount => TIER_UNLOCK_LEVELS.Length;
    public static int GetUnlockLevel(int tier) => TIER_UNLOCK_LEVELS[tier - 1];
    // 유저 레벨로 해금된 묶음 수 (0~6)
    public static int GetUnlockedTierCount(int userLv) => System.Linq.Enumerable.Count(TIER_UNLOCK_LEVELS, l => userLv >= l);

    public string key;
    [Tooltip("묶음 1~6 (AuthorityData.csv Tier). 해금 레벨 = TIER_UNLOCK_LEVELS[tier - 1]")]
    public int tier = 1;
    public Sprite thum; // 권능은 등급 없음

    [Tooltip("어울리는 슬라임 유형 (AuthorityData.csv MatchTags, \"Zone|Heavy\"). 로비에서 장착 슬라임 중 해당 종 수 표시")]
    public MinionTag matchTags;

    [TextArea] public string desc; // AuthorityData.csv Desc (설명 원본, 아직 번역 안 함)

    public string Title => TranslateManager.GetText($"Authority.{key}");
    public string Desc => desc;

    // 장착 슬라임 중 matchTags 중 하나라도 가진 종 수
    public static int CountMatchingMinions(MinionTag matchTags, IEnumerable<MinionTag> equippedMinionTags)
    {
        if (matchTags == MinionTag.None)
            return 0;
        int count = 0;
        foreach (MinionTag tags in equippedMinionTags)
        {
            if ((tags & matchTags) != MinionTag.None)
                count++;
        }
        return count;
    }

#if UNITY_EDITOR
    public void Edit()
    {
        TableReader table = TableReader.Load("AuthorityData.csv");
        if (table == null) return;

        TableReader.Row row = table.Find("Key", key);
        if (row == null)
        {
            Debug.LogWarning($"[AuthorityData] CSV에서 key '{key}' 를 찾지 못함");
            return;
        }

        if (row.TryGetInt("Tier", out int t)) tier = t;
        if (table.HasColumn("Desc"))
            desc = row.Get("Desc");
        if (table.HasColumn("MatchTags"))
        {
            matchTags = MinionTags.Parse(row.Get("MatchTags"), out string tagError);
            if (tagError != null)
                Debug.LogWarning($"[AuthorityData] {key} MatchTags 모르는 이름: {tagError}");
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[AuthorityData] {key} Edit 완료");
    }
#endif
}

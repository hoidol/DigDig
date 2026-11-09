using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// 권능 (CLAUDE.md "권능"): 묶음 6개 × 3개, 레벨 5·10·15·20·25·30에 해금, 묶음마다 1개 선택, 선택한 것 전부 적용
public class AuthorityTest
{
    static AuthorityData Authority(string key, int tier)
    {
        var data = UnityEngine.ScriptableObject.CreateInstance<AuthorityData>();
        data.key = key;
        data.tier = tier;
        return data;
    }

    // 잠긴 묶음은 선택 불가, 해금되면 가능
    [Test]
    public void CheckSelect_LockedUntilUnlockLevel()
    {
        var tier1 = Authority("A", 1);
        var tier6 = Authority("B", 6);
        Assert.AreEqual(AuthoritySelectResult.Locked, UserAuthorityManager.CheckSelect(4, tier1));
        Assert.AreEqual(AuthoritySelectResult.Success, UserAuthorityManager.CheckSelect(5, tier1));
        Assert.AreEqual(AuthoritySelectResult.Locked, UserAuthorityManager.CheckSelect(29, tier6));
        Assert.AreEqual(AuthoritySelectResult.Success, UserAuthorityManager.CheckSelect(30, tier6));
        Assert.AreEqual(AuthoritySelectResult.NoData, UserAuthorityManager.CheckSelect(30, null));
        UnityEngine.Object.DestroyImmediate(tier1);
        UnityEngine.Object.DestroyImmediate(tier6);
    }

    // 저장 데이터 배열은 묶음 수에 맞춰짐 (이전 저장·묶음 추가 대비)
    [Test]
    public void UserAuthorityData_NormalizeToTierCount()
    {
        var data = new UserAuthorityData { selectedKeys = new[] { "A" } };
        data.Normalize();
        Assert.AreEqual(AuthorityData.TierCount, data.selectedKeys.Length);
        Assert.AreEqual("A", data.selectedKeys[0], "기존 선택 유지");
        data.selectedKeys = null;
        data.Normalize();
        Assert.AreEqual(AuthorityData.TierCount, data.selectedKeys.Length);
    }

    // 묶음 해금: 레벨 5·10·15·20·25·30
    [Test]
    public void UnlockedTierCount_ByUserLevel()
    {
        Assert.AreEqual(0, AuthorityData.GetUnlockedTierCount(4));
        Assert.AreEqual(1, AuthorityData.GetUnlockedTierCount(5));
        Assert.AreEqual(2, AuthorityData.GetUnlockedTierCount(14));
        Assert.AreEqual(6, AuthorityData.GetUnlockedTierCount(30));
        Assert.AreEqual(6, AuthorityData.GetUnlockedTierCount(99));
        Assert.AreEqual(5, AuthorityData.GetUnlockLevel(1));
        Assert.AreEqual(30, AuthorityData.GetUnlockLevel(6));
    }

    // 권능 에셋: 묶음마다 정확히 3개, key 중복 없음, 이름 번역·설명(CSV Desc) 있음
    [Test]
    public void Assets_ThreePerTier_WithTranslation()
    {
        var authorities = UnityEditor.AssetDatabase.FindAssets("t:AuthorityData", new[] { "Assets/AddressableResources/AuthorityData" })
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<AuthorityData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .ToList();
        var problems = new List<string>();
        for (int tier = 1; tier <= AuthorityData.TierCount; tier++)
        {
            int count = authorities.Count(r => r.tier == tier);
            if (count != AuthorityData.AUTHORITIES_PER_TIER)
                problems.Add($"묶음 {tier}: {count}개 (필요 {AuthorityData.AUTHORITIES_PER_TIER})");
        }
        problems.AddRange(authorities.Where(r => r.tier < 1 || r.tier > AuthorityData.TierCount).Select(r => $"{r.key}: 묶음 {r.tier} 범위 밖"));
        problems.AddRange(authorities.GroupBy(r => r.key).Where(g => g.Count() > 1).Select(g => $"key 중복: {g.Key}"));

        TableReader translate = TableReader.Load("Translate.csv");
        foreach (var r in authorities)
        {
            if (translate.Find("Key", $"Authority.{r.key}") == null)
                problems.Add($"이름 번역 없음: Authority.{r.key}");
            if (string.IsNullOrWhiteSpace(r.desc))
                problems.Add($"{r.key}: 설명 없음 (AuthorityData.csv Desc)");
        }
        Assert.IsEmpty(problems, string.Join("\n", problems));
    }
}

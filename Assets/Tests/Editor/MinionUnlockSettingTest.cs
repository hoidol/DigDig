using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// (2026-11-20) 미니언 지급 설정 (Resources/MinionUnlockSetting): 초기 지급·레벨 지급 key가 실제 미니언인지, 중복·권능 해금 레벨 겹침 없음
public class MinionUnlockSettingTest
{
    static HashSet<string> AllMinionKeys()
    {
        var keys = new HashSet<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:MinionData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<MinionData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null && !string.IsNullOrEmpty(data.key))
                keys.Add(data.key);
        }
        return keys;
    }

    [Test]
    public void Setting_KeysValid_NoDuplicate()
    {
        var setting = Resources.Load<MinionUnlockSetting>(MinionUnlockSetting.RESOURCE_PATH);
        Assert.IsNotNull(setting, "Resources/MinionUnlockSetting 없음");
        Assert.AreEqual(GameSetting.MINION_SLOT_COUNT, setting.initMinionKeys.Length, "초기 지급 = 장착 칸 수");

        var all = AllMinionKeys();
        var used = setting.initMinionKeys.Concat(setting.levelUnlocks.Select(e => e.key)).ToList();
        foreach (string key in used)
            Assert.IsTrue(all.Contains(key), $"없는 미니언 key: {key}");
        Assert.AreEqual(used.Count, used.Distinct().Count(), "중복 key");

        foreach (var e in setting.levelUnlocks)
        {
            Assert.Greater(e.level, 0, e.key);
            CollectionAssert.DoesNotContain(AuthorityData.TIER_UNLOCK_LEVELS, e.level, $"{e.key}: 권능 해금 레벨과 겹침");
        }
    }

    [Test]
    public void ScavengerAndBreeze_NotUsed()
    {
        Assert.IsFalse(GameSetting.IsUsedMinion("Scavenger"));
        Assert.IsFalse(GameSetting.IsUsedMinion("Breeze"));
        Assert.AreEqual(11, GameSetting.INIT_MINION_KEYS.Length + GameSetting.MINION_LEVEL_UNLOCKS.Length);
    }

    [Test]
    public void LevelUnlockKeys_ByLevel()
    {
        CollectionAssert.AreEqual(new[] { "Bounce", "Iron", "Pierce" }, UserMinionManager.GetLevelUnlockKeys(3));
        CollectionAssert.IsEmpty(UserMinionManager.GetLevelUnlockKeys(0));
    }
}

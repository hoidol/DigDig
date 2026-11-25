using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-25) 레벨업 미니언 강화가 실제 미니언 스탯에 들어가는지 (강적 피해는 버프 초기화 뒤에 적용돼야 함)
public class MinionEnhancePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Enhance_AppliesToFieldMinion()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        string key = GameTestUtil.MinionKeys()[0];
        Minion m = GameTestUtil.SpawnMinion(key, 0, false);
        Assert.IsNotNull(m);
        yield return WaitFrames(2);
        float atk = m.statDic[StatType.AttackPower];
        Assert.AreEqual(0f, m.EliteBossDamage, 0.001f);
        MinionEnhanceSystem.Add(key, StatType.EliteBossDamage);
        MinionEnhanceSystem.Add(key, StatType.AttackPower);
        yield return WaitFrames(1);
        Assert.AreEqual(15f, m.EliteBossDamage, 0.01f, "강적 피해 +15%");
        Assert.AreEqual(atk * 1.15f, m.statDic[StatType.AttackPower], 0.01f, "공격력 +15%");
        // 새로 소환한 같은 종도 적용
        Minion m2 = GameTestUtil.SpawnMinion(key, 0, false);
        yield return WaitFrames(2);
        Assert.AreEqual(15f, m2.EliteBossDamage, 0.01f, "이후 소환분도 강적 피해");
        MinionEnhanceSystem.Clear();
    }
}

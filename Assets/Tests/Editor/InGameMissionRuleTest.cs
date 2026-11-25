using NUnit.Framework;

// 인게임 미션 판정 (Mission 하위 클래스, 씬 없이 MissionContext로)
public class InGameMissionRuleTest
{
    static MissionContext Ctx(params (string key, int mergeLevel)[] minions)
    {
        var ctx = new MissionContext();
        foreach (var m in minions)
            ctx.fieldMinions.Add(new MissionContext.FieldMinion(m.key, m.mergeLevel));
        return ctx;
    }

    [Test]
    public void ClearWaveWithOneMinion()
    {
        var mission = new ClearWaveWithMinionsMission { waveNumber = 1, maxMinionCount = 1 };

        var ctx = Ctx(("Flame", 0));
        ctx.waveIdx = 0;
        ctx.minionCountAtWaveStart = 1;
        Assert.IsFalse(mission.IsComplete(ctx), "클리어 전");
        Assert.IsFalse(mission.IsFailed(ctx));
        ctx.isWaveCleared = true;
        Assert.IsTrue(mission.IsComplete(ctx), "1기로 wave 1 클리어");

        ctx.minionCountAtWaveStart = 2;
        Assert.IsFalse(mission.IsComplete(ctx), "2기로 시작");
        Assert.IsTrue(mission.IsFailed(ctx));

        ctx.minionCountAtWaveStart = 0;
        Assert.IsFalse(mission.IsComplete(ctx), "0기는 인정 안 함");

        var later = Ctx();
        later.waveIdx = 1;
        later.minionCountAtWaveStart = 1;
        later.isWaveCleared = true;
        Assert.IsFalse(mission.IsComplete(later), "다른 wave");
        Assert.IsTrue(mission.IsFailed(later), "wave 1이 지나감");

        var beforeStart = Ctx();
        Assert.IsFalse(mission.IsFailed(beforeStart), "게임 시작 전 브레이크타임");

        // 미니언 4기 이하로 Wave 5 클리어
        var elite = new ClearWaveWithMinionsMission { waveNumber = 5, maxMinionCount = 4 };
        var w5 = Ctx();
        w5.waveIdx = 3;
        Assert.IsFalse(elite.IsFailed(w5), "wave 4 진행 중");
        w5.waveIdx = 4;
        w5.minionCountAtWaveStart = 4;
        w5.isWaveCleared = true;
        Assert.IsTrue(elite.IsComplete(w5), "4기로 클리어");
        w5.minionCountAtWaveStart = 5;
        Assert.IsFalse(elite.IsComplete(w5));
        Assert.IsTrue(elite.IsFailed(w5), "5기로 시작");
    }

    [Test]
    public void PlaceDistinctMinions()
    {
        var lv1 = new PlaceMinionsMission { minLevel = 1, count = 5, distinctKeys = true };
        var lv2 = new PlaceMinionsMission { minLevel = 2, count = 5, distinctKeys = true };

        var four = Ctx(("Flame", 0), ("Ice", 0), ("Random", 0), ("Orbit", 0), ("Flame", 0));
        Assert.IsFalse(lv1.IsComplete(four), "같은 종은 1기로 셈");
        Assert.AreEqual((4, 5), lv1.GetProgress(four));

        var five = Ctx(("Flame", 0), ("Ice", 0), ("Random", 0), ("Orbit", 0), ("Lightning", 0));
        Assert.IsTrue(lv1.IsComplete(five));
        Assert.IsFalse(lv2.IsComplete(five), "레벨 1은 레벨 2 미션에 안 셈");

        // (2026-11-25) 정확히 그 레벨만 셈: Lv.3·4가 섞이면 Lv.2 미션 미완료
        var mixed = Ctx(("Flame", 1), ("Ice", 1), ("Random", 2), ("Orbit", 1), ("Lightning", 3));
        Assert.IsFalse(lv2.IsComplete(mixed), "Lv.2는 3기뿐");
        Assert.AreEqual((3, 5), lv2.GetProgress(mixed));
        var allLv2 = Ctx(("Flame", 1), ("Ice", 1), ("Random", 1), ("Orbit", 1), ("Lightning", 1));
        Assert.IsTrue(lv2.IsComplete(allLv2), "서로 다른 Lv.2 5기 동시에");
        Assert.IsFalse(lv1.IsComplete(allLv2), "Lv.2는 Lv.1 미션에 안 셈");

        var lv3 = new PlaceMinionsMission { minLevel = 3, count = 1, distinctKeys = false, exactLevel = false };
        Assert.IsFalse(lv3.IsComplete(Ctx(("Flame", 1), ("Ice", 1))), "Lv.2만 있음");
        Assert.IsTrue(lv3.IsComplete(Ctx(("Flame", 1), ("Ice", 2))), "Lv.3 1기");

        var notDistinct =new PlaceMinionsMission { minLevel = 1, count = 3, distinctKeys = false };
        Assert.IsTrue(notDistinct.IsComplete(Ctx(("Flame", 0), ("Flame", 0), ("Flame", 0))));
    }

    [Test]
    public void DefaultMissionsAndTitles()
    {
        // 번역 에셋은 Play 때 임포트되므로 CSV에서 직접 key 확인
        string csv = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath, "Json/Translate.csv"));
        var missions = InGameMissionSetting.GetMissions();
        Assert.GreaterOrEqual(missions.Count, 1);
        foreach (var m in missions)
        {
            Assert.IsTrue(csv.Contains("\n" + m.TitleKey + "\t"), $"Translate.csv에 없는 key: {m.TitleKey}");
            m.customTitle = "{0}/{1}";
            Assert.IsFalse(m.GetTitle().Contains("{"), "제목 수치");
        }
    }
}

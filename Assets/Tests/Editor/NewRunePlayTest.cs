using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-21) 레벨업 룬 카드 풀 + 새 룬 4종 (크리티컬·스킬 쿨타임·성장·경험치) 효과, 최대 Lv.3
public class NewRunePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator NewRunes_Work()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        var setting = SlimeGrowthSetting.Instance;
        // 카드 풀: 설정 key가 모두 ItemData로 있음, 타일 고치기는 막힌 칸이 있을 때만
        foreach (string key in setting.runeKeys)
            Assert.IsNotNull(RuneManager.Instance.GetRuneData(key), $"룬 데이터 없음: {key}");
        var pool = RuneManager.Instance.GetLevelUpRunes(setting.runeKeys).Select(d => d.key).ToList();
        Assert.AreEqual(TileManager.Instance.HasBlockedTile, pool.Contains("PioneerTile"), "타일 고치기는 막힌 칸이 있을 때만");
        foreach (string key in setting.runeKeys.Where(k => k != "PioneerTile"))
            Assert.IsTrue(RuneManager.Instance.GetRuneData(key).MaxLevel == 3, $"{key} 최대 Lv.3");

        Tile Free() => TileManager.Instance.tiles.First(t => t.IsEmpty && t.CanAddTileRune && t.tileObject == null);

        // 크리티컬: 치명타 확률 + 배율
        Tile critTile = Free();
        Minion crit = SpawnMinion("Pierce", critTile);
        Slime.Instance.AddRune("CritPowerTile", critTile).Forget();
        yield return WaitFrames(3);
        var critRune = (CritPowerTileRune)critTile.tileRune;
        var data = new AllyUnitDamageData();
        data.Init(crit);
        critRune.ModifyDamageData(data);
        // (2026-11-25) 레벨 값 = 치명타 확률 15/30/45%, 치명타 피해 ×2 고정
        Assert.AreEqual(15f, data.critChance, 0.01f);
        Assert.AreEqual(2f, data.critPower, 0.01f);
        Assert.IsTrue(critRune.ForceUpgrade() && critRune.ForceUpgrade(), "Lv.3까지");
        Assert.IsFalse(critRune.ForceUpgrade(), "Lv.3이 최대");
        Assert.AreEqual(45f, critRune.chance, 0.01f);
        Assert.AreEqual(2f, critRune.critPower, 0.01f);

        // 스킬 쿨타임: 위 미니언 SkillCooldownSpeed +20
        Tile cdTile = Free();
        Minion cd = SpawnMinion("Random", cdTile);
        Slime.Instance.AddRune("SkillCooldownTile", cdTile).Forget();
        yield return WaitFrames(3);
        Assert.AreEqual(20f, cd.SkillCooldownSpeed, 0.01f, "스킬 쿨타임 룬 Lv.1 = +20%");

        // 성장: 브레이크타임마다 +2%, 최대 10%
        Tile growTile = Free();
        Minion grow = SpawnMinion("Pierce", growTile);
        yield return WaitFrames(2);
        float baseAtk = grow.statDic[StatType.AttackPower];
        Slime.Instance.AddRune("GrowthTile", growTile).Forget();
        yield return WaitFrames(3);
        var growRune = (GrowthTileRune)growTile.tileRune;
        for (int i = 0; i < 7; i++)
            GameEventBus.Publish(new BreakTimeStartEvent(i + 1));
        yield return WaitFrames(2);
        Assert.AreEqual(10f, growRune.bonus, 0.01f, "최대 +10%에서 멈춤");
        Assert.AreEqual(baseAtk * 1.1f, grow.statDic[StatType.AttackPower], 0.01f);

        // 경험치: 이 칸 유닛이 처치 → 확률(100%로 고정해서) 슬라임 경험치 +1
        Tile expTile = Free();
        Minion expMinion = SpawnMinion("Pierce", expTile);
        Slime.Instance.AddRune("ExpTile", expTile).Forget();
        yield return WaitFrames(3);
        var expRune = (ExpTileRune)expTile.tileRune;
        expRune.chance = 100f;
        Enemy enemy = EnemyFieldController.Instance.Instantiate(GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Normal));
        enemy.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), 0);
        yield return WaitFrames(1);
        Slime.Instance.exp = 0; // 레벨업이 안 나게 0부터
        var kill = new AllyUnitDamageData();
        kill.Init(expMinion);
        kill.damage = 1e9f;
        enemy.TakeDamage(kill);
        yield return WaitFrames(2);
        Assert.AreEqual(1, Slime.Instance.exp, "경험치 룬 1 (처치 경험치는 2026-11-22부터 없음)");
    }
}

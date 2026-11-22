using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-21) Lv.15 종족 패시브: 15개를 하나씩 Lv.3까지 올려 실제 전투에서 에러 없이 도는지 + 대표 효과 확인
public class KingPassivePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator AllPassives_RunInBattle()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        var setting = KingGrowthSetting.Instance;
        var inv = king.abilityInventory;

        // 킹을 가운데쯤(1,1)에, 주변 8칸 막힘 해제
        Tile center = TileManager.Instance.GetTile(new Vector2Int(1, 1));
        foreach (var o in LevelUpChoiceRule.AROUND_OFFSETS.Append(Vector2Int.zero))
        {
            Tile t = TileManager.Instance.GetTile(new Vector2Int(1, 1) + o);
            if (t != null && t.isBlocked) t.Unblock();
        }
        MoveKing(king, center);

        if (GameManager.Instance.isBreakTime)
            GameManager.Instance.EndBreakTime(); // 적이 나오게
        Time.timeScale = 4f;

        foreach (KingRaceType race in System.Enum.GetValues(typeof(KingRaceType)))
        {
            AbilityData raceData = setting.raceAbilities.First(d => ((RaceKingAbility)d.prefab).raceType == race);
            inv.Add(raceData);
            yield return WaitFrames(2);

            // 버프 칸에 미니언 (같은 종 2마리 → 오크 결속 확인용)
            var raceTiles = KingSlime.Instance.GetComponent<KingTileBuffArea>().GetTargetTiles();
            var spawned = raceTiles.Where(t => t.IsEmpty).Take(2).Select(t => SpawnMinion("Iron", t)).ToList();
            yield return WaitFrames(3);

            foreach (AbilityData data in setting.GetRacePassives(race))
            {
                for (int i = 0; i < 3; i++)
                    inv.Add(data); // Lv.1 → 3
                Assert.AreEqual(3, inv.GetLevel(data), data.key);
                yield return WaitFrames(3);
                Check(data, spawned);
                yield return KeepAlive(king, 2f);
                inv.Remove(data);
                yield return WaitFrames(2);
            }

            foreach (Minion m in spawned)
                if (m != null) { Tile t = TileManager.Instance.GetTile(m); if (t != null) t.SetTileObject(null); m.gameObject.SetActive(false); }
            inv.Remove(raceData);
            yield return WaitFrames(2);
        }
        Time.timeScale = 1f;
    }

    static void Check(AbilityData data, System.Collections.Generic.List<Minion> minions)
    {
        var area = KingSlime.Instance.GetComponent<KingTileBuffArea>();
        switch (data.prefab)
        {
            case OrcTribalBondPassive _:
                Assert.AreEqual(3f, area.BuffScale, 0.001f, "부족의 결속 Lv.3: 같은 종 2마리 → 칸 버프 ×3");
                break;
            case HumanSolidarityPassive _:
                Assert.AreEqual(1f, area.BuffScale, 0.001f, "연대: 같은 종이면 배율 없음");
                break;
            case HumanDrillMasterPassive _:
                foreach (Minion m in minions)
                    Assert.AreEqual(2, m.kingMergeBonus, "훈련 교관 Lv.3: 머지 +2");
                break;
            case ElfForestBlessingPassive _:
                Assert.IsTrue(minions.All(m => m.statDic[StatType.AttackRange] > 0f));
                break;
        }
    }

    static IEnumerator KeepAlive(KingSlime king, float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end)
        {
            king.health.curHp = king.MaxHp;
            yield return null;
        }
    }

    static void MoveKing(KingSlime king, Tile to)
    {
        Tile from = TileManager.Instance.GetTile(king);
        if (from == to)
            return;
        if (from != null)
            from.SetTileObject(null);
        to.SetTileObject(king);
        king.transform.position = to.transform.position;
    }
}

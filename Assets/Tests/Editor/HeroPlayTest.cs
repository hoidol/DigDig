using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-25) 용사 자동 테스트 (미니언 MinionPlayTest처럼): KingGrowthSetting.heroAbilities의 모든 용사 × (스킬 강화 없음 / 있음)
// Lv.8로 장착(스킬 3단계 해방) → 킹 주변에 적 → 기본 공격 + 스킬 1·2·3을 모두 써 보고 싸우게 해 에러가 없는지, 피해가 들어가는지 본다
// 새 용사는 heroAbilities에 넣기만 하면 자동으로 대상. 피해량 비교는 Tools/Test/Minion Test 창의 "용사 테스트 > 피해 측정"
public class HeroPlayTest : InGamePlayTestBase
{
    const float FIGHT_TIME = 2.5f; // 실제 시간(초)

    [UnityTest]
    public IEnumerator AllHeroes_SkillsAndFight_NoErrors()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        AbilityData[] heroes = KingGrowthSetting.Instance.heroAbilities.Where(h => h != null).ToArray();
        Assert.IsNotEmpty(heroes);
        var failed = new List<string>();
        foreach (AbilityData data in heroes)
        {
            foreach (bool enhanced in new[] { false, true })
            {
                current = $"{data.key}{(enhanced ? "/강화" : "")}";
                int errorsBefore = errors.Count;
                HeroKingAbility hero = HeroDamageBenchmark.EquipHero(data, GameSetting.KING_MAX_LEVEL, enhanced);
                Assert.IsNotNull(hero, current);
                Assert.AreEqual(HeroKingAbility.SKILL_COUNT, hero.UnlockedSkillCount, $"{current}: Lv.8 = 스킬 3단계 해방");
                for (int i = 0; i < 2; i++)
                    Assert.AreEqual(enhanced, hero.IsSkillEnhanced(i), $"{current}: {i + 1}단계 스킬 강화");
                if (GameManager.Instance.isBreakTime)
                    GameManager.Instance.EndBreakTime();
                yield return WaitFrames(2);

                // 킹 주변 적 (경로를 따라 움직이지 않게 매 프레임 제자리로)
                Vector2 kingPos = king.transform.position;
                var spots = new List<Vector2>();
                for (int i = 0; i < 6; i++)
                    spots.Add(kingPos + (Vector2)(Quaternion.Euler(0, 0, i * 60f) * Vector2.right) * (0.8f + (i % 2) * 0.6f));
                var enemies = spots.Select(PlaceEnemy).ToList();
                DamageMeterSystem.Clear();

                // 스킬 1·2·3 (위치 지정 = 킹 오른쪽 적 자리)
                for (int idx = 0; idx < HeroKingAbility.SKILL_COUNT; idx++)
                {
                    hero.FillChargesForTest();
                    HeroKingAbility.UseResult r = idx != HeroKingAbility.COMMON_SKILL_INDEX && hero.NeedsTargetPosition(idx)
                        ? hero.TryUseSkillAt(idx, spots[0])
                        : hero.TryUseSkill(idx);
                    if (r != HeroKingAbility.UseResult.Used)
                        errors.Add($"[{current}] {hero.GetSkillName(idx)} 사용 실패: {r}");
                }

                for (double end = UnityEditor.EditorApplication.timeSinceStartup + FIGHT_TIME; UnityEditor.EditorApplication.timeSinceStartup < end;)
                {
                    for (int i = 0; i < enemies.Count; i++)
                        if (enemies[i] != null && enemies[i].CurHp > 0)
                            enemies[i].transform.position = spots[i];
                    if (king.health.curHp < king.MaxHp)
                        king.health.curHp = king.MaxHp;
                    yield return WaitFrames(1);
                }

                if (DamageMeterSystem.KingTotalDamage <= 0)
                    errors.Add($"[{current}] 킹 피해 없음");
                if (DamageMeterSystem.GetKingDamage(HeroKingAbility.TAG_BASIC) <= 0)
                    errors.Add($"[{current}] 기본 공격 피해 없음");
                // 킹 주변·지정 위치에 바로 피해를 주는 스킬 (망령은 적 경로를 돌아서 제외, 매의 눈·저주는 피해 없는 스킬)
                if (hero is KnightHero || hero is ArcherHero)
                {
                    if (DamageMeterSystem.GetKingDamage(HeroKingAbility.TAG_SKILL1) <= 0)
                        errors.Add($"[{current}] {hero.GetSkillName(0)} 피해 없음");
                }
                if (hero is KnightHero && DamageMeterSystem.GetKingDamage(HeroKingAbility.TAG_SKILL2) <= 0)
                    errors.Add($"[{current}] {hero.GetSkillName(1)} 피해 없음");

                EnemyFieldController.Instance.ClearAllEnemies();
                yield return WaitFrames(2);
                if (errors.Count > errorsBefore)
                    failed.Add(current);
            }
        }
        AssertNoErrors($"에러 난 용사: {string.Join(", ", failed)}");
    }

    static Enemy PlaceEnemy(Vector2 pos)
    {
        var before = new HashSet<Enemy>(EnemyFieldController.Instance.activeEnemies);
        MinionTestWindow.SpawnEnemies(EnemyType.Normal, DefenceType.Normal, 1);
        Enemy e = EnemyFieldController.Instance.activeEnemies.First(x => !before.Contains(x));
        e.maxHp = e.curHp = 1e6f;
        e.transform.position = pos;
        return e;
    }
}

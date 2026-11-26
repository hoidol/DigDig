using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-25) 테스트 공용 동작 (기기 테스트 패널 MobileTestPanel · 에디터 Minion Test 창 · 용사 피해 측정 · 자동 테스트가 같이 씀)
public static class GameTestUtil
{
    // ---- 미니언
    // 게임에 로드된 미니언 key (표시 순서)
    public static List<string> MinionKeys() => MinionManager.Instance == null ? new List<string>()
        : MinionManager.Instance.minionDataDic.Values.OrderBy(d => GameSetting.GetMinionOrder(d.key)).ThenBy(d => d.key).Select(d => d.key).ToList();

    // 빈 칸(막힘 X, 유닛 X)에 소환. 칸을 주면 그 칸 (비어 있어야 함)
    public static Minion SpawnMinion(string key, int mergeLevel, bool evolve, Tile tile = null)
    {
        if (string.IsNullOrEmpty(key) || KingSlime.Instance == null || TileManager.Instance == null)
            return null;
        tile ??= TileManager.Instance.tiles.FirstOrDefault(t => t != null && !t.isBlocked && t.IsEmpty);
        if (tile == null || tile.isBlocked || !tile.IsEmpty)
            return null;
        Minion minion = KingSlime.Instance.AddMinion(key, Mathf.Clamp(mergeLevel, 0, MinionData.MAX_MERGE_LEVEL));
        tile.SetTileObject(minion);
        if (evolve && minion.CanEvolve && mergeLevel >= MinionData.MAX_MERGE_LEVEL)
            minion.Evolve();
        return minion;
    }

    public static void ClearFieldMinions()
    {
        if (MinionFieldController.Instance == null)
            return;
        foreach (Minion m in MinionFieldController.Instance.activeminions.ToList())
            if (m != null && TileManager.Instance.GetTile(m) != null)
                MinionFieldController.Instance.RemoveFieldMinion(m);
    }

    // ---- 적
    // 적 소환 (일반 = 경로 번갈아, 엘리트·보스 = 주 경로, 엘리트·보스는 항상 Normal 방어 타입). 체력 = 지금 wave 기준
    public static int SpawnEnemies(EnemyType enemyType, DefenceType defenceType, int count)
    {
        GameManager gm = GameManager.Instance;
        EnemyFieldController field = EnemyFieldController.Instance;
        StageData stage = gm != null ? gm.stageData : null;
        if (stage == null || field == null)
            return 0;
        // 보스는 스테이지 데이터의 boss로 따로 생성 (BossWave와 같은 방식, 풀 밖)
        if (enemyType == EnemyType.Boss)
        {
            if (stage.boss == null)
                return 0;
            for (int i = 0; i < count; i++)
                Object.Instantiate(stage.boss).Spawn(field.GetSpawnPosition(), gm.waveIdx);
            return count;
        }
        DefenceType def = enemyType == EnemyType.Normal ? defenceType : DefenceType.Normal;
        Enemy prefab = stage.GetEnemyPrefab(enemyType, def);
        if (prefab == null)
        {
            Debug.LogWarning($"[GameTestUtil] 적 프리팹 없음: {enemyType} / {def}");
            return 0;
        }
        int spawned = 0;
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = field.Instantiate(prefab);
            if (enemy == null)
                continue;
            if (enemyType == EnemyType.Normal)
            {
                Transform[] route = field.NextNormalRoute();
                enemy.Spawn(route[0].position, gm.waveIdx);
                enemy.SetRoute(route);
            }
            else
                enemy.Spawn(field.GetSpawnPosition(), gm.waveIdx);
            spawned++;
        }
        return spawned;
    }

    // 적 체력 (wave = waveIdx, 0부터). 종류별 배율 = 적 프리팹 EnemyData.hpMultiplier
    public static float GetEnemyHp(StageData stage, int waveIdx, EnemyType type)
    {
        if (stage == null)
            return 0f;
        Enemy prefab = type == EnemyType.Boss ? stage.boss : stage.GetEnemyPrefab(type, DefenceType.Normal);
        float mul = prefab != null && prefab.enemyData != null ? prefab.enemyData.hpMultiplier : 1f;
        return Enemy.CalcWaveHp(stage, waveIdx) * mul;
    }

    // ---- 용사
    public static AbilityData[] Heroes() => KingGrowthSetting.Instance != null
        ? KingGrowthSetting.Instance.heroAbilities.Where(h => h != null).ToArray() : new AbilityData[0];

    // 킹 성장 능력 전부 제거 → 킹 레벨 → 용사 → 스킬 해방(레벨까지) → 1·2단계 스킬 강화
    public static HeroKingAbility EquipHero(AbilityData heroData, int level, bool enhanceSkills)
    {
        KingSlime king = KingSlime.Instance;
        if (king == null || heroData == null)
            return null;
        KingAbilityInventory inv = king.abilityInventory;
        foreach (KingAbility a in inv.Abilities.ToList())
            inv.Remove(a.data);
        king.lv = Mathf.Clamp(level, 1, GameSetting.KING_MAX_LEVEL);
        king.pendingLevelUps = 0;
        inv.Add(heroData);
        HeroKingAbility hero = inv.Get<HeroKingAbility>();
        if (hero == null)
            return null;
        for (int i = 0; i < HeroKingAbility.SKILL_COUNT; i++)
            if (king.lv >= LevelUpChoiceRule.GetSkillUnlockLevel(i))
                hero.UnlockNextSkill();
        if (enhanceSkills && KingGrowthSetting.Instance != null)
            foreach (AbilityData d in KingGrowthSetting.Instance.heroBoostAbilities)
                if (d != null && d.prefab is HeroSkillEnhanceKingAbility s && s.skillIndex < hero.UnlockedSkillCount)
                    inv.Add(d);
        hero.RefreshHeroLevel();
        GameEventBus.Publish(new ExpChangedEvent(king.exp, king.MaxExp, king.lv)); // 경험치바·HUD 갱신
        return hero;
    }

    // 스킬 바로 쓰기: 브레이크타임이면 wave 시작, 에너지가 모자라면 채움, 위치 지정 스킬 = 적이 가장 몰린 곳 (없으면 킹 앞)
    public static HeroKingAbility.UseResult UseHeroSkill(HeroKingAbility hero, int idx)
    {
        if (GameManager.Instance != null && GameManager.Instance.isBreakTime)
            GameManager.Instance.EndBreakTime();
        if (hero.Charges < hero.GetSkillCost(idx))
            hero.FillChargesForTest();
        if (idx != HeroKingAbility.COMMON_SKILL_INDEX && hero.NeedsTargetPosition(idx))
        {
            Vector2 pos = HeroKingAbility.FindDensest(1.5f, out Vector2 p) ? p : (Vector2)KingSlime.Instance.transform.position + Vector2.right * 2f;
            return hero.TryUseSkillAt(idx, pos);
        }
        return hero.TryUseSkill(idx);
    }
}

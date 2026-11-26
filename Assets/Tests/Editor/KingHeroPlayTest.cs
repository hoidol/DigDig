using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 용사: 정예병 칸 미니언만 칸 버프(킹 X) · 에너지(wave 중 충전, 처치 충전 없음) · 스킬 해방 3단계 · 하단 버튼으로 사용
// (2026-11-25) 공격 형태·스킬 개편: 기사 휘두르기·칼날 회오리·심판의 대검(위치) / 레인저 다중 2·화살비(위치)·매의 눈 / 네크로멘서 슬래쉬·망령·저주
public class KingHeroPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Heroes_TileBuff_Energy_Skills()
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
        AbilityData knightData = setting.heroAbilities.First(d => d.prefab is KnightHero);
        AbilityData archerData = setting.heroAbilities.First(d => d.prefab is RangerHero);
        AbilityData necroData = setting.heroAbilities.First(d => d.prefab is NecromancerHero);

        // 킹을 (1,1), 기사 칸 = 왼(0,1)·오른(2,1), 칸 밖 = 위(1,0)
        Tile center = TileManager.Instance.GetTile(new Vector2Int(1, 1));
        Tile left = TileManager.Instance.GetTile(new Vector2Int(0, 1));
        Tile up = TileManager.Instance.GetTile(new Vector2Int(1, 0));
        foreach (Tile t in new[] { center, left, up })
        {
            if (t.isBlocked) t.Unblock();
            if (t.tileObject != null && t.tileObject != (object)king) t.SetTileObject(null);
        }
        Tile from = TileManager.Instance.GetTile(king);
        if (from != center)
        {
            if (from != null) from.SetTileObject(null);
            center.SetTileObject(king);
            king.transform.position = center.transform.position;
        }
        yield return WaitFrames(2);

        // 1) 기사: 칸 미니언 공격력 +10%, 킹은 안 받음, 킹 공격 = 물리
        inv.Add(knightData);
        var knight = inv.Get<KnightHero>();
        Minion buffed = SpawnMinion("Iron", left, 0);
        Minion plain = SpawnMinion("Iron", up, 0);
        yield return WaitFrames(3);
        float baseAtk = plain.statDic[StatType.AttackPower];
        // (2026-11-25) 정예병 칸 버프 = HeroData.csv InitBuffValue (기사 공격력 +20%)
        float knightBuff = 1f + HeroDataTable.Get("Knight").GetEliteBuffValue(1) / 100f;
        Assert.Greater(knightBuff, 1f, "HeroData 기사 InitBuffValue");
        Assert.AreEqual(baseAtk * knightBuff, buffed.statDic[StatType.AttackPower], 0.01f, "기사 칸 미니언 공격력 (HeroData)");
        // 킹 공격력 = 기사 HeroData 값 그대로 (칸 버프는 킹이 안 받음)
        Assert.AreEqual(king.statMgr.GetHeroStat(HeroDataTable.Get("Knight"), StatType.AttackPower, knight.HeroLevel), king.statMgr.AttackPower, 0.01f, "킹은 칸 버프를 안 받음");
        Assert.AreEqual(AttackType.Physic, king.AttackType);

        // 2) 에너지: 스킬 해방 전엔 최대 0개. 해방하면 최대 1개, wave 중에만 충전
        Assert.AreEqual(0, knight.MaxCharges);
        Assert.AreEqual(HeroKingAbility.UseResult.Locked, knight.TryUseSkill(0));
        king.lv = 2; // 에너지 최대 = 1단계 해방 때 2개 (이후 자동 증가 없음)
        knight.UnlockNextSkill();
        Assert.AreEqual(2, knight.MaxCharges);
        if (GameManager.Instance.isBreakTime)
            GameManager.Instance.EndBreakTime();
        yield return WaitFrames(2);
        Assert.IsTrue(HeroKingAbility.IsCharging, "wave 중 충전");
        knight.AddChargeTime(knight.ChargeTime - 1f);
        Assert.AreEqual(0, knight.Charges);

        // (2026-11-25) 정예병 칸 미니언이 처치해도 충전 시간은 그대로 (처치 충전 삭제)
        MinionTestWindow.SpawnEnemies(EnemyType.Normal, DefenceType.Normal, 1);
        yield return WaitFrames(2);
        Enemy enemy = EnemyFieldController.Instance.activeEnemies.Last();
        float before = knight.ChargeProgress;
        var kill = new AllyUnitDamageData();
        kill.Init(buffed);
        kill.damage = 999999f;
        enemy.TakeDamage(kill);
        yield return WaitFrames(1);
        Assert.Less(knight.ChargeProgress - before, 0.1f, "정예병 처치로 충전되면 안 됨");

        knight.FillChargesForTest();
        Assert.AreEqual(2, knight.Charges);
        Assert.IsTrue(knight.IsFull);

        // 기본 공격 = 근거리 휘두르기 (탄 없음): 킹 옆 적이 맞음
        Vector2 kingPos = king.transform.position;
        Enemy e1 = PlaceEnemy(kingPos + Vector2.right * 0.8f);
        float hp = e1.CurHp;
        Assert.IsTrue(knight.TryKingAttack(Vector2.right));
        Assert.Less(e1.CurHp, hp, "기사 휘두르기 피해 없음");
        float swingDamage = hp - e1.CurHp;

        // (2026-11-25) 킹 칸 연발 룬 = 휘두르기 반복 (탄이 아니어도 룬 적용)
        KingSlime.Instance.AddRune("BurstShotTile", center).Forget();
        yield return WaitFrames(2);
        Assert.IsTrue(king.shotModifier.tileRune is BurstShotTileRune, "킹 칸 연발 룬");
        hp = e1.CurHp;
        Assert.IsTrue(knight.TryKingAttack(Vector2.right));
        for (double end = UnityEditor.EditorApplication.timeSinceStartup + 1; UnityEditor.EditorApplication.timeSinceStartup < end;)
        {
            e1.transform.position = kingPos + Vector2.right * 0.8f;
            yield return WaitFrames(1);
        }
        Assert.GreaterOrEqual(hp - e1.CurHp, swingDamage * 1.99f, "연발 룬 = 휘두르기 2번");

        // (2026-11-26) 1단계 심판의 대검 = 위치 지정 (버튼만 누르면 에너지 그대로) → 누른 곳 범위 피해 + 스턴, 에너지 1개
        Assert.AreEqual(1, knight.GetSkillCost(0), "1단계 = 에너지 1개");
        Assert.AreEqual(HeroKingAbility.UseResult.NeedPosition, knight.TryUseSkill(0));
        Assert.AreEqual(2, knight.Charges, "위치를 고르기 전엔 에너지 그대로");
        Vector2 far = kingPos + Vector2.up * 3f;
        Enemy e2 = PlaceEnemy(far);
        hp = e2.CurHp;
        Assert.AreEqual(HeroKingAbility.UseResult.Used, knight.TryUseSkillAt(0, far));
        Assert.AreEqual(1, knight.Charges);
        for (double end = UnityEditor.EditorApplication.timeSinceStartup + 2; UnityEditor.EditorApplication.timeSinceStartup < end && Mathf.Approximately(e2.CurHp, hp);)
        {
            e2.transform.position = far;
            yield return WaitFrames(1);
        }
        Assert.Less(e2.CurHp, hp, "심판의 대검 피해 없음");
        Assert.IsTrue(e2.statusEffectHandler.IsStunned, "심판의 대검 스턴 없음");
        for (double end = UnityEditor.EditorApplication.timeSinceStartup + 1; UnityEditor.EditorApplication.timeSinceStartup < end && e2.statusEffectHandler.FixedSlowRate >= 1f;)
        {
            e2.transform.position = far;
            yield return WaitFrames(1);
        }
        Assert.AreEqual(knight.greatswordSlowRate, e2.statusEffectHandler.FixedSlowRate, 0.001f, $"남은 대검 근처 둔화 (대검 {Object.FindObjectsOfType<HeroGreatSwordDrop>().Length}개, 적 체력 {e2.CurHp})");

        // 2단계 칼날 회오리 = 킹 주변 적 피해 + 둔화, 에너지 1개 (모든 용사 공통, 3단계만 2개)
        // (2026-11-26) 최대 미니언 수 +1 = 2단계 해방이 아니라 용사 Lv.7 도달
        int maxMinion = MinionFieldController.Instance.minionMaxCount;
        king.lv = 6;
        knight.UnlockNextSkill();
        knight.RefreshHeroLevel();
        Assert.AreEqual(maxMinion, MinionFieldController.Instance.minionMaxCount, "2단계 해방(Lv.6)으로는 최대 미니언 수 그대로");
        king.lv = 7;
        knight.RefreshHeroLevel();
        Assert.AreEqual(maxMinion + 1, MinionFieldController.Instance.minionMaxCount, "용사 Lv.7 = 최대 미니언 수 +1");
        Assert.AreEqual(2, knight.MaxCharges, "킹 레벨로 에너지 최대가 늘지 않음");
        Assert.AreEqual(1, knight.GetSkillCost(1), "2단계 = 에너지 1개");
        Assert.AreEqual(2, knight.GetSkillCost(HeroKingAbility.COMMON_SKILL_INDEX), "3단계 = 에너지 2개");
        Assert.AreEqual(1, knight.Charges);
        Assert.AreEqual(HeroKingAbility.UseResult.Used, knight.TryUseSkill(1), "에너지 1개로 2단계 사용");
        Assert.AreEqual(0, knight.Charges);
        Assert.IsTrue(knight.IsSpinActive);
        hp = e1.CurHp;
        for (double end = UnityEditor.EditorApplication.timeSinceStartup + 1.5; UnityEditor.EditorApplication.timeSinceStartup < end && Mathf.Approximately(e1.CurHp, hp);)
        {
            e1.transform.position = kingPos + Vector2.right * 0.8f;
            yield return WaitFrames(1);
        }
        Assert.Less(e1.CurHp, hp, $"칼날 회오리 피해 없음 (회오리 {knight.IsSpinActive}, 거리 {Vector2.Distance(e1.transform.position, king.transform.position):0.00}, timeScale {Time.timeScale}, 적 활성 {e1.isActiveAndEnabled})");
        Assert.Less(e1.statusEffectHandler.SlowRate, 1f, "칼날 회오리 둔화 없음");
        Assert.AreEqual(knight.spinSlowRate, e1.statusEffectHandler.FixedSlowRate, 0.001f, "고정 둔화 50%");
        Assert.IsTrue(e1.statusEffectHandler.HasEffect(SlowEffect.EFFECT_KEY), "발밑 둔화 표시");
        Assert.IsFalse(e1.statusEffectHandler.HasEffect("Ice"), "얼음 표시 없음");

        // 3단계 용사가 될거야! = 칸 밖 미니언도 공격력 ×1.5
        king.lv = 10;
        knight.UnlockNextSkill();
        Assert.AreEqual(2, knight.MaxCharges);
        knight.FillChargesForTest();
        Assert.Greater(knight.CommonAttackPowerMul, 1f, "용사가 될거야! 배율 (HeroData Skill3Value)");
        float plainAtk = plain.statDic[StatType.AttackPower];
        Assert.AreEqual(HeroKingAbility.UseResult.Used, knight.TryUseSkill(2));
        yield return WaitFrames(2);
        Assert.AreEqual(plainAtk * knight.CommonAttackPowerMul, plain.statDic[StatType.AttackPower], 0.01f, "용사가 될거야! = 모든 미니언 공격력");
        inv.Remove(knightData);
        yield return WaitFrames(2);
        Assert.AreEqual(baseAtk, buffed.statDic[StatType.AttackPower], 0.01f, "용사를 빼면 칸 버프 해제");
        Assert.AreEqual(plainAtk, plain.statDic[StatType.AttackPower], 0.01f, "용사를 빼면 공용 스킬 버프 해제");
        Assert.AreEqual(maxMinion, MinionFieldController.Instance.minionMaxCount);

        // 3) 레인저: 다중 타겟 2, 화살비(위치 지정)·매의 눈
        int shootBefore = king.shotModifier.bonusShootCount;
        inv.Add(archerData);
        var archer = inv.Get<RangerHero>();
        Assert.AreEqual(shootBefore + 1, king.shotModifier.bonusShootCount, "레인저 = 다중 타겟 2");
        archer.UnlockNextSkill();
        archer.UnlockNextSkill();
        archer.FillChargesForTest();
        Assert.AreEqual(HeroKingAbility.UseResult.NeedPosition, archer.TryUseSkill(0));
        Enemy e3 = PlaceEnemy(far);
        hp = e3.CurHp;
        Assert.AreEqual(HeroKingAbility.UseResult.Used, archer.TryUseSkillAt(0, far));
        Assert.Less(e3.CurHp, hp, "화살비 첫 피해 없음");

        float rangeBefore = plain.statDic[StatType.AttackRange];
        float spdBefore = plain.statDic[StatType.AttackSpeed];
        archer.FillChargesForTest();
        Assert.AreEqual(HeroKingAbility.UseResult.Used, archer.TryUseSkill(1));
        yield return WaitFrames(2);
        Assert.IsTrue(archer.IsEyeActive);
        Assert.AreEqual(rangeBefore * archer.EyeRangeMul, plain.statDic[StatType.AttackRange], 0.01f, "매의 눈 = 공격 범위 ×(1 + Skill2Value)");
        Assert.AreEqual(1.5f, archer.EyeRangeMul, 0.001f, "HeroData.csv Skill2Value 0.5 = +50%");
        Assert.AreEqual(spdBefore, plain.statDic[StatType.AttackSpeed], 0.01f, "(2026-11-26) 매의 눈은 공속을 안 올림");
        Assert.AreEqual(FindTargetType.HighHp, plain.EffectiveFindTargetType, "매의 눈 = 높은 체력 노림");
        Assert.AreEqual(FindTargetType.HighHp, king.EffectiveFindTargetType);
        inv.Remove(archerData);
        yield return WaitFrames(2);
        Assert.AreEqual(rangeBefore, plain.statDic[StatType.AttackRange], 0.01f, "레인저를 빼면 매의 눈 해제");
        Assert.IsNull(HeroKingAbility.ForcedAllyFindTarget);
        Assert.AreEqual(shootBefore, king.shotModifier.bonusShootCount);

        // 4) 네크로멘서: 카오스 폭발탄(스플래쉬), 망령 소환·저주
        inv.Add(necroData);
        var necro = inv.Get<NecromancerHero>();
        Assert.AreEqual(AttackType.Chaos, king.AttackType);
        EnemyFieldController.Instance.ClearAllEnemies(); // 앞 단계 적이 남아 있으면 폭발탄 타겟이 그쪽이 됨
        yield return WaitFrames(1);
        Vector2 p4 = kingPos + Vector2.right * 1.5f, p5 = p4 + Vector2.up * necro.splashRadius * 0.6f;
        Enemy e4 = PlaceEnemy(p4);
        Enemy e5 = PlaceEnemy(p5);
        hp = e4.CurHp;
        float hp5 = e5.CurHp;
        int splashes = HeroSplashProjectile.ActiveCount;
        Assert.IsTrue(necro.TryKingAttack(((Vector2)e4.transform.position - kingPos).normalized));
        Assert.Greater(HeroSplashProjectile.ActiveCount, splashes, "폭발탄 발사");
        for (double end = UnityEditor.EditorApplication.timeSinceStartup + 2; UnityEditor.EditorApplication.timeSinceStartup < end && (Mathf.Approximately(e4.CurHp, hp) || Mathf.Approximately(e5.CurHp, hp5));)
        {
            e4.transform.position = p4;
            e5.transform.position = p5;
            yield return WaitFrames(1);
        }
        Assert.Less(e4.CurHp, hp, "폭발탄 피해 없음");
        Assert.Less(e5.CurHp, hp5, "폭발탄 범위 피해 (옆 적) 없음");

        necro.UnlockNextSkill();
        necro.UnlockNextSkill();
        int ghosts = HeroGhost.ActiveCount;
        necro.FillChargesForTest();
        Assert.AreEqual(HeroKingAbility.UseResult.Used, necro.TryUseSkill(0));
        yield return WaitFrames(2);
        Assert.AreEqual(ghosts + 1, HeroGhost.ActiveCount, "망령 소환 안 됨");
        // (2026-11-26) 망령에 닿으면 피해 + 3초 둔화
        hp5 = e5.CurHp;
        necro.GhostHit(e5);
        Assert.Less(e5.CurHp, hp5, "망령 피해 없음");
        Assert.AreEqual(necro.ghostSlowRate, e5.statusEffectHandler.FixedSlowRate, 0.001f, "망령 둔화 없음");
        necro.FillChargesForTest();
        Assert.AreEqual(HeroKingAbility.UseResult.Used, necro.TryUseSkill(1));
        Assert.AreEqual(1.5f, e4.statusEffectHandler.CurseDamageMul, 0.001f, "저주 = 받는 피해 +50%");
        Assert.IsTrue(e4.statusEffectHandler.IsStunned, "저주 스턴 없음");
        StringAssert.Contains("망령 소환", necroData.GetDescription(1));
        inv.Remove(necroData);
    }

    // 적 1마리를 pos에 두고 체력을 크게 (스킬 한 번에 죽지 않게)
    static Enemy PlaceEnemy(Vector2 pos)
    {
        var before = new System.Collections.Generic.HashSet<Enemy>(EnemyFieldController.Instance.activeEnemies);
        MinionTestWindow.SpawnEnemies(EnemyType.Normal, DefenceType.Normal, 1);
        Enemy e = EnemyFieldController.Instance.activeEnemies.First(x => !before.Contains(x));
        e.maxHp = e.curHp = 1e6f; // 1e9면 float 정밀도 때문에 작은 피해가 안 줄어듦
        e.transform.position = pos;
        return e;
    }
}

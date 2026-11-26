using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-26) 킹 레벨업 구성: [Lv.1 용사 선택] → 스탯 변화 화면(스킬 해방 자동) → [용사 강화] → 미니언 강화
// Lv.1 용사(레인저) → 2 스킬 1 → 3·4·5 용사 강화 → 6 스킬 2 → 7(미니언 수 +1)·8·9 용사 강화 → 10 스킬 3 → 11·12 용사 강화 → MAX
// (2026-11-25) 레벨업마다 용사 스탯(HeroData Add*)·정예병 칸 버프(Init + Add × (Lv-1)) 자동 상승, 단계 카드 화면에 변화 표시
//   용사 강화 = 정예 칸 추가(카드마다 다른 칸) · 충전 가속(-10%/Lv)
public class KingLevelUpFlowPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator LevelUp_1To12()
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
        var canvas = InGame.LevelUpCanvas.Instance;
        var inv = king.abilityInventory;
        int baseMinionMax = MinionFieldController.Instance.minionMaxCount;
        Assert.AreEqual(0, king.lv);
        // Lv.2 카드 확률 = 킹 레벨 %, Lv.0이면 표시 숨김
        var chanceText = MinionOfferSystem.Instance.levelUpChanceText;
        Assert.IsNotNull(chanceText);
        Assert.IsFalse(chanceText.gameObject.activeSelf, "킹 Lv.0 = 확률 표시 숨김");

        // 킹을 가운데쪽 (1,1)로 옮기고 레인저 칸(왼위·오른위) 풀기 (킹 시작 칸은 랜덤)
        Tile center = TileManager.Instance.GetTile(new Vector2Int(1, 1));
        foreach (var idx in new[] { new Vector2Int(1, 1), new Vector2Int(0, 0), new Vector2Int(2, 0) })
        {
            Tile t = TileManager.Instance.GetTile(idx);
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

        // Lv.1 용사 = 레인저 (버프 칸 2칸, 칸 미니언 2발 확률, 킹 물리 다중 타겟)
        yield return LevelUp(king);
        Assert.IsTrue(canvas.rewardPanel != null && canvas.rewardPanel.gameObject.activeInHierarchy, "레벨업 보상(주사위 코인 +1) 표시");
        Assert.AreEqual(InGame.LevelUpCanvas.Step.Hero, canvas.CurrentStep, "레벨업 첫 화면 = 용사 카드");
        var shown = Shown(canvas);
        Assert.AreEqual(3, shown.Length, "용사 3장");
        AbilityData archer = setting.heroAbilities.First(d => d.prefab is RangerHero);
        int bonusBefore = king.shotModifier.bonusShootCount;
        var heroTiles = new[] { new Vector2Int(-1, -1), new Vector2Int(1, -1) };
        shown[0].SetHeroData(archer, heroTiles);
        shown[0].OnClickedSelect();
        yield return WaitFrames(2);
        Assert.AreEqual(1, inv.GetLevel(archer));
        // (2026-11-26) 용사를 고르면 용사 스탯 변화 화면 (킹 기본 → 용사 Lv.1 + 정예병 칸)
        yield return ConfirmStatScreen(canvas, "정예병 칸");
        // (2026-11-25) 미니언 강화 카드 새로고침: 무료 2번 → 광고 3번 (테스트에선 광고 바로 성공), 다 쓰면 버튼 숨김
        Assert.IsTrue(canvas.IsStatStep);
        Assert.IsTrue(canvas.rerollButton != null && canvas.rerollButton.gameObject.activeInHierarchy, "새로고침 버튼");
        var counter = RerollCounter.MinionEnhance;
        Assert.AreEqual(RerollCounter.FREE_PER_GAME, counter.FreeLeft);
        Assert.AreEqual(RerollCounter.AD_PER_GAME, counter.AdLeft);
        for (int r = 0; r < 3; r++)
        {
            canvas.OnClickedReroll();
            yield return WaitFrames(3);
            Assert.IsTrue(canvas.IsStatStep && canvas.gameObject.activeSelf, "새로고침해도 미니언 강화 단계");
            Assert.AreEqual(3, Shown(canvas).Count(p => p.Data != null && p.Data.IsMinionEnhance), "새로고침 후 카드 3장");
        }
        Assert.AreEqual(0, counter.FreeLeft, "무료 2번 다 씀");
        Assert.AreEqual(RerollCounter.AD_PER_GAME - 1, counter.AdLeft, "3번째는 광고");
        StringAssert.Contains("광고", canvas.rerollText.text);
        for (int r = 0; r < RerollCounter.AD_PER_GAME - 1; r++)
        {
            canvas.OnClickedReroll();
            yield return WaitFrames(3);
        }
        Assert.IsFalse(counter.CanReroll);
        Assert.IsFalse(canvas.rerollButton.gameObject.activeSelf, "다 쓰면 새로고침 버튼 숨김");
        Assert.AreEqual(1, king.pendingLevelUps, "새로고침은 레벨업을 소모하지 않음");
        yield return LevelUpRunePlayTest.PickMinionEnhanceCard();
        CollectionAssert.AreEquivalent(heroTiles, inv.GetBuffTileOffsets(), "카드에 나온 정예병 칸 2칸");
        Assert.AreEqual(AttackType.Physic, king.AttackType, "레인저 = 물리");
        Assert.AreEqual(bonusBefore + 1, king.shotModifier.bonusShootCount, "레인저 = 다중 타겟 2");
        Assert.IsTrue(chanceText.gameObject.activeSelf, "킹 Lv.1부터 확률 표시");
        StringAssert.Contains("1%", chanceText.text);
        var hero = inv.Get<RangerHero>();
        Assert.AreEqual(0, hero.MaxCharges, "Lv.1 = 스킬 없음");

        Tile buffTile = king.GetComponent<EliteArea>().GetEliteTiles().First(t => t.tileObject == null);
        Minion minion = SpawnMinion("Iron", buffTile, 0);
        yield return WaitFrames(3);
        Assert.AreEqual(20f, minion.DoubleShotChance, 0.01f, "레인저 Lv.1 2발 확률");

        HeroDataTable.HeroData archerData = HeroDataTable.Get("Ranger");
        Assert.IsNotNull(archerData);
        float atkLv1 = king.statMgr.AttackPower;

        // Lv.2 1단계 스킬 해방 = 스탯 변화 화면에서 자동 (카드 없음) + 용사 레벨 2
        yield return LevelUpFlow(king, canvas, hero.GetSkillName(0));
        Assert.AreEqual(1, hero.UnlockedSkillCount);
        Assert.AreEqual(atkLv1 + archerData.GetAdd(StatType.AttackPower), king.statMgr.AttackPower, 0.01f, "레벨업 = 킹 공격력 +AddAttackPower");
        Assert.AreEqual(archerData.GetEliteBuffValue(2), minion.DoubleShotChance, 0.01f, "레인저 Lv.2 2발 확률 (Init + Add)");
        Assert.AreEqual(2, hero.MaxCharges, "Lv.2 에너지 최대 2");

        // Lv.3 용사 강화 3장 = 정예 칸 추가 아닌 카드 2장 + 정예 칸 추가 1장 → 정예 칸 추가 (2단계 스킬 강화는 아직 안 나옴)
        yield return LevelUp(king);
        yield return ConfirmStatScreen(canvas, "→");
        Assert.AreEqual(InGame.LevelUpCanvas.Step.HeroBoost, canvas.CurrentStep);
        var boost = Shown(canvas);
        Assert.AreEqual(3, boost.Length, "용사 강화 3장");
        Assert.IsFalse(boost.Any(p => p.Data == archer), "용사 카드는 안 나옴");
        var expandCards = boost.Where(p => p.Data.category == AbilityCategory.TileExpand).ToArray();
        Assert.AreEqual(1, expandCards.Length, "정예 칸 추가 1장");
        Assert.IsFalse(boost.Any(p => p.Data.prefab is HeroSkillEnhanceKingAbility s && s.skillIndex == 1), "2단계 스킬 강화는 2단계 해방 후");
        expandCards[0].OnClickedSelect();
        yield return WaitFrames(2);
        yield return LevelUpRunePlayTest.PickMinionEnhanceCard();
        Assert.AreEqual(3, inv.GetBuffTileOffsets().Count, "정예 칸 3칸");
        Assert.AreEqual(archerData.GetEliteBuffValue(3), minion.DoubleShotChance, 0.01f, "레인저 Lv.3 2발 확률");

        // Lv.4 용사 강화 → 충전 가속 (×0.8)
        AbilityData charge = setting.heroBoostAbilities.First(d => d.prefab is HeroEnergyKingAbility e && e.boostType == HeroEnergyBoostType.ChargeTime);
        yield return LevelUpFlow(king, canvas, "→", charge);
        Assert.AreEqual(GameSetting.HERO_ENERGY_CHARGE_TIME * 0.8f, hero.ChargeTime, 0.01f, "충전 가속 1 = ×0.8");

        // Lv.5 용사 강화 → 에너지 보유 증가 2 → 3, 최대 미니언 수 그대로
        AbilityData energyMax = setting.heroBoostAbilities.First(d => d.prefab is HeroEnergyKingAbility e && e.boostType == HeroEnergyBoostType.MaxCharges);
        yield return LevelUpFlow(king, canvas, "→", energyMax);
        Assert.AreEqual(3, hero.MaxCharges, "에너지 보유 증가 = 최대 3");
        Assert.AreEqual(baseMinionMax, MinionFieldController.Instance.minionMaxCount, "Lv.5엔 최대 미니언 수 그대로");

        // Lv.6 2단계 스킬 해방 (자동)
        yield return LevelUpFlow(king, canvas, hero.GetSkillName(1));
        Assert.AreEqual(2, hero.UnlockedSkillCount);

        // Lv.7 스탯 화면에 최대 미니언 수 +1 → 용사 강화(2단계 스킬 강화) → 레벨업을 마치면 +1
        AbilityData skill2 = setting.heroBoostAbilities.First(d => d.prefab is HeroSkillEnhanceKingAbility s && s.skillIndex == 1);
        StringAssert.Contains(hero.GetSkillName(1), skill2.GetDescription(1), "스킬 강화 카드 설명 = 용사 스킬");
        string eyeDesc = hero.GetSkillDesc(1);
        yield return LevelUpFlow(king, canvas, "필드 최대 미니언 수", skill2);
        Assert.IsTrue(hero.IsSkillEnhanced(1));
        Assert.AreNotEqual(eyeDesc, hero.GetSkillDesc(1), "강화하면 스킬 설명 수치가 바뀜");
        Assert.AreEqual(baseMinionMax + 1, MinionFieldController.Instance.minionMaxCount, "Lv.7 최대 미니언 수 +1");

        // Lv.8 충전 가속 2 (×0.64), Lv.9 아무 강화
        yield return LevelUpFlow(king, canvas, "→", charge);
        Assert.AreEqual(GameSetting.HERO_ENERGY_CHARGE_TIME * 0.64f, hero.ChargeTime, 0.01f, "충전 가속 2 = ×0.8^2");
        yield return LevelUpFlow(king, canvas, "→", null);

        // Lv.10 3단계 공용 스킬 (자동)
        yield return LevelUpFlow(king, canvas, HeroKingAbility.COMMON_SKILL_NAME);
        Assert.AreEqual(3, hero.UnlockedSkillCount);
        Assert.AreEqual(archerData.GetEliteBuffValue(10), minion.DoubleShotChance, 0.01f, "레인저 Lv.10 2발 확률");

        // Lv.11·12 용사 강화
        yield return LevelUpFlow(king, canvas, "→", null);
        yield return LevelUpFlow(king, canvas, "→", null);
        Assert.AreEqual(baseMinionMax + 1, MinionFieldController.Instance.minionMaxCount, "미니언 수 +1은 Lv.7 한 번만");

        // 최대 레벨(12)이면 경험치 MAX, 더 못 삼
        Assert.AreEqual(GameSetting.KING_MAX_LEVEL, king.lv);
        Assert.IsTrue(king.IsMaxLv);
        king.AddDiceCoin(10);
        int coin = king.diceCoin;
        Assert.IsFalse(king.TryBuyExp(), "최대 레벨이면 경험치 구매 불가");
        Assert.AreEqual(coin, king.diceCoin);
        king.AddExp(5);
        Assert.AreEqual(GameSetting.KING_MAX_LEVEL, king.lv);
        Assert.AreEqual(0, king.pendingLevelUps);
    }

    static LevelUpStatPanel[] Shown(InGame.LevelUpCanvas canvas) => canvas.levelUpStatPanels.Where(p => p.gameObject.activeSelf).ToArray();

    static IEnumerator LevelUp(KingSlime king)
    {
        king.AddExp(king.MaxExp);
        yield return WaitFrames(2);
        Assert.IsTrue(InGame.LevelUpCanvas.Instance.gameObject.activeSelf, $"Lv.{king.lv} 레벨업 창이 안 열림");
    }

    // 스탯 변화 화면: contains가 글씨에 있어야 함 → 확인
    static IEnumerator ConfirmStatScreen(InGame.LevelUpCanvas canvas, string contains)
    {
        Assert.IsTrue(canvas.IsStatChangeScreen, $"Lv.{KingSlime.Instance.ChoiceLevel} 스탯 변화 화면이 안 나옴 (단계 {canvas.CurrentStep})");
        StringAssert.Contains(contains, canvas.statScreenText.text);
        Assert.IsTrue(canvas.statScreenConfirmButton != null, "확인 버튼");
        int pending = KingSlime.Instance.pendingLevelUps;
        canvas.statScreenConfirmButton.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.AreEqual(pending, KingSlime.Instance.pendingLevelUps, "스탯 화면 확인으로는 레벨업이 소모되지 않음");
        Assert.IsTrue(canvas.gameObject.activeSelf, "확인 다음 단계로 다시 열려야 함");
    }

    // 레벨업 → 스탯 변화 화면(statText 포함) → (pickBoost가 있거나 용사 강화 단계면) 용사 강화 카드 → 미니언 강화
    // pickBoost = null이면 용사 강화 단계가 나오는지 보지 않고, 나오면 첫 카드
    static IEnumerator LevelUpFlow(KingSlime king, InGame.LevelUpCanvas canvas, string statText, AbilityData pickBoost = null)
    {
        yield return LevelUp(king);
        yield return ConfirmStatScreen(canvas, statText);
        if (canvas.CurrentStep == InGame.LevelUpCanvas.Step.HeroBoost)
        {
            var shown = Shown(canvas);
            Assert.AreEqual(3, shown.Length, $"Lv.{king.ChoiceLevel} 용사 강화 카드 수");
            if (pickBoost != null)
                shown[0].SetData(pickBoost, king.abilityInventory.GetLevel(pickBoost) + 1);
            shown[0].OnClickedSelect();
            yield return WaitFrames(2);
            Assert.AreEqual(1, king.pendingLevelUps, "용사 강화로는 레벨업이 소모되지 않음");
        }
        else
            Assert.IsNull(pickBoost, $"Lv.{king.ChoiceLevel} 용사 강화 단계가 안 나옴");
        yield return LevelUpRunePlayTest.PickMinionEnhanceCard();
        Assert.AreEqual(0, king.pendingLevelUps);
    }
}

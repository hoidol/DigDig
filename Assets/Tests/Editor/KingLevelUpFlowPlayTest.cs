using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 킹 레벨업 구성: 레벨업마다 ① 킹 능력치 카드 → ② 단계 카드
// Lv.1 용사(엘프 궁수, 정예병 칸 2칸) → 2 스킬 1 → 3·4 용사 강화 → 5 스킬 2 + 최대 미니언 수 +1 → 6·7 용사 강화 → 8 스킬 3 → MAX
// (2026-11-25) 레벨업마다 용사 스탯(HeroData Add*)·정예병 칸 버프(Init + Add × (Lv-1)) 자동 상승, 단계 카드 화면에 변화 표시
//   용사 강화 = 정예 칸 추가(카드마다 다른 칸) · 충전 가속(-10%/Lv)
public class KingLevelUpFlowPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator LevelUp_1To8()
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

        // 킹을 가운데쪽 (1,1)로 옮기고 궁수 칸(왼위·오른위) 풀기 (킹 시작 칸은 랜덤)
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

        // Lv.1 용사 = 엘프 궁수 (버프 칸 2칸, 칸 미니언 2발 확률, 킹 물리 다중 타겟)
        yield return LevelUp(king);
        Assert.IsTrue(canvas.rewardPanel != null && canvas.rewardPanel.gameObject.activeInHierarchy, "레벨업 보상(주사위 코인 +1) 표시");
        Assert.IsFalse(canvas.IsStatStep, "레벨업 첫 화면 = 용사 카드");
        var shown = Shown(canvas);
        Assert.AreEqual(3, shown.Length, "용사 3장");
        AbilityData archer = setting.heroAbilities.First(d => d.prefab is ArcherHero);
        int bonusBefore = king.shotModifier.bonusShootCount;
        var heroTiles = new[] { new Vector2Int(-1, -1), new Vector2Int(1, -1) };
        shown[0].SetHeroData(archer, heroTiles);
        shown[0].OnClickedSelect();
        yield return WaitFrames(2);
        Assert.AreEqual(1, inv.GetLevel(archer));
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
        // 그다음 미니언 강화 카드 → 고른 종·스탯이 필드 미니언에 적용
        yield return LevelUpRunePlayTest.PickMinionEnhanceCard();
        CollectionAssert.AreEquivalent(heroTiles, inv.GetBuffTileOffsets(), "카드에 나온 정예병 칸 2칸");
        Assert.AreEqual(AttackType.Physic, king.AttackType, "궁수 = 물리");
        Assert.AreEqual(bonusBefore + 1, king.shotModifier.bonusShootCount, "궁수 = 다중 타겟 2");
        Assert.IsTrue(chanceText.gameObject.activeSelf, "킹 Lv.1부터 확률 표시");
        StringAssert.Contains("1%", chanceText.text);
        var hero = inv.Get<ArcherHero>();
        Assert.AreEqual(0, hero.MaxCharges, "Lv.1 = 스킬 없음");

        Tile buffTile = king.GetComponent<EliteArea>().GetEliteTiles().First(t => t.tileObject == null);
        Minion minion = SpawnMinion("Iron", buffTile, 0);
        yield return WaitFrames(3);
        Assert.AreEqual(20f, minion.DoubleShotChance, 0.01f, "궁수 Lv.1 2발 확률");

        HeroDataTable.HeroData archerData = HeroDataTable.Get("Archer");
        Assert.IsNotNull(archerData);
        float atkLv1 = king.statMgr.AttackPower;

        // Lv.2 1단계 스킬 해방 (한 장) + 용사 레벨 2: 킹 공격력 +Add, 2발 확률 +Add
        yield return LevelUpStage(king, canvas, 1);
        Assert.AreEqual(atkLv1 + archerData.GetAdd(StatType.AttackPower), king.statMgr.AttackPower, 0.01f, "레벨업 = 킹 공격력 +AddAttackPower");
        Assert.AreEqual(archerData.GetEliteBuffValue(2), minion.DoubleShotChance, 0.01f, "궁수 Lv.2 2발 확률 (Init + Add)");
        Assert.AreEqual(2, hero.MaxCharges, "Lv.2 에너지 최대 2");
        Assert.AreEqual(baseMinionMax, MinionFieldController.Instance.minionMaxCount, "Lv.2엔 최대 미니언 수 그대로");

        // Lv.3 용사 강화 3장 = 정예 칸 추가 아닌 카드 2장 + 정예 칸 추가 1장 → 정예 칸 추가 (2단계 스킬 강화는 아직 안 나옴)
        yield return LevelUp(king);
        var boost = Shown(canvas);
        Assert.AreEqual(3, boost.Length, "용사 강화 3장");
        Assert.IsFalse(boost.Any(p => p.Data == archer), "용사 카드(정예병 버프 강화)는 안 나옴");
        var expandCards = boost.Where(p => p.Data.category == AbilityCategory.TileExpand).ToArray();
        Assert.AreEqual(1, expandCards.Length, "정예 칸 추가 1장");
        Assert.IsFalse(boost.Any(p => p.Data.prefab is HeroSkillEnhanceKingAbility s && s.skillIndex == 1), "2단계 스킬 강화는 2단계 해방 후");
        expandCards[0].OnClickedSelect();
        yield return WaitFrames(2);
        yield return LevelUpRunePlayTest.PickMinionEnhanceCard();
        Assert.AreEqual(3, inv.GetBuffTileOffsets().Count, "정예 칸 3칸");
        Assert.AreEqual(1, inv.GetLevel(archer), "용사 카드 레벨은 그대로");
        Assert.AreEqual(archerData.GetEliteBuffValue(3), minion.DoubleShotChance, 0.01f, "궁수 Lv.3 2발 확률");

        // Lv.4 용사 강화 → 충전 가속
        AbilityData charge = setting.heroBoostAbilities.First(d => d.prefab is HeroEnergyKingAbility e && e.boostType == HeroEnergyBoostType.ChargeTime);
        yield return LevelUpStage(king, canvas, 3, charge);
        Assert.AreEqual(GameSetting.HERO_ENERGY_CHARGE_TIME * 0.9f, hero.ChargeTime, 0.01f, "충전 가속 -10%");
        Assert.AreEqual(2, hero.MaxCharges, "Lv.4 에너지 최대 그대로 2");

        // Lv.5 2단계 스킬 + 최대 미니언 수 +1
        yield return LevelUpStage(king, canvas, 1);
        Assert.AreEqual(2, hero.MaxCharges, "Lv.5 에너지 최대 그대로 2");
        Assert.AreEqual(baseMinionMax + 1, MinionFieldController.Instance.minionMaxCount, "Lv.5 최대 미니언 수 +1");

        // Lv.6 용사 강화 → 2단계 스킬 강화 (매의 눈 지속 시간 ×1.5)
        AbilityData skill2 = setting.heroBoostAbilities.First(d => d.prefab is HeroSkillEnhanceKingAbility s && s.skillIndex == 1);
        StringAssert.Contains(hero.GetSkillName(1), skill2.GetDescription(1), "스킬 강화 카드 설명 = 용사 스킬");
        string eyeDesc = hero.GetSkillDesc(1);
        yield return LevelUpStage(king, canvas, 3, skill2);
        Assert.IsTrue(hero.IsSkillEnhanced(1));
        Assert.IsFalse(hero.IsSkillEnhanced(0));
        Assert.AreNotEqual(eyeDesc, hero.GetSkillDesc(1), "강화하면 스킬 설명 수치가 바뀜");
        // Lv.7 용사 강화 → 에너지 보유 증가 2 → 3
        AbilityData energyMax = setting.heroBoostAbilities.First(d => d.prefab is HeroEnergyKingAbility e && e.boostType == HeroEnergyBoostType.MaxCharges);
        yield return LevelUpStage(king, canvas, 3, energyMax);
        Assert.AreEqual(3, hero.MaxCharges, "에너지 보유 증가 = 최대 3");

        // Lv.8 3단계 공용 스킬
        yield return LevelUpStage(king, canvas, 1);
        Assert.AreEqual(3, hero.MaxCharges, "Lv.8 에너지 최대 그대로 (카드로만 증가)");
        Assert.AreEqual(HeroKingAbility.COMMON_SKILL_NAME, hero.GetSkillName(2));
        Assert.AreEqual(archerData.GetEliteBuffValue(8), minion.DoubleShotChance, 0.01f, "궁수 Lv.8 2발 확률");

        // 최대 Lv.8: 경험치 MAX, 더 못 삼
        Assert.AreEqual(8, king.lv);
        Assert.IsTrue(king.IsMaxLv);
        king.AddDiceCoin(10);
        int coin = king.diceCoin;
        Assert.IsFalse(king.TryBuyExp(), "최대 레벨이면 경험치 구매 불가");
        Assert.AreEqual(coin, king.diceCoin);
        king.AddExp(5);
        Assert.AreEqual(8, king.lv);
        Assert.AreEqual(0, king.pendingLevelUps);
    }

    static LevelUpStatPanel[] Shown(InGame.LevelUpCanvas canvas) => canvas.levelUpStatPanels.Where(p => p.gameObject.activeSelf).ToArray();

    static IEnumerator LevelUp(KingSlime king)
    {
        king.AddExp(king.MaxExp);
        yield return WaitFrames(2);
        Assert.IsTrue(InGame.LevelUpCanvas.Instance.gameObject.activeSelf, $"Lv.{king.lv} 레벨업 창이 안 열림");
    }

    // 능력치 카드만 고르고 창이 닫힘
    static IEnumerator LevelUpStatOnly(KingSlime king, InGame.LevelUpCanvas canvas)
    {
        yield return LevelUp(king);
        Assert.IsTrue(canvas.IsStatStep);
        Shown(canvas)[0].OnClickedSelect();
        yield return WaitFrames(2);
        Assert.IsFalse(canvas.gameObject.activeSelf, $"Lv.{king.lv}: 능력치만 고르고 닫힘");
        Assert.AreEqual(0, king.pendingLevelUps);
    }

    // 레벨업 → 단계 카드 cardCount장 중 첫 장 → 미니언 강화 카드 (pick을 주면 첫 칸을 그 카드로 바꿔 고름)
    static IEnumerator LevelUpStage(KingSlime king, InGame.LevelUpCanvas canvas, int cardCount, AbilityData pick = null)
    {
        yield return LevelUp(king);
        Assert.IsFalse(canvas.IsStatStep, $"Lv.{king.ChoiceLevel} 첫 화면은 단계 카드");
        var shown = Shown(canvas);
        Assert.AreEqual(cardCount, shown.Length, $"Lv.{king.ChoiceLevel} 단계 카드 수");
        if (king.abilityInventory.Get<HeroKingAbility>() != null) // 용사 능력치 변화 "공격력 : 15 → 20"
        {
            Assert.IsTrue(canvas.statChangeText != null && canvas.statChangeText.gameObject.activeInHierarchy, "능력치 변화 표시");
            StringAssert.Contains("→", canvas.statChangeText.text);
        }
        if (pick != null)
            shown[0].SetData(pick, king.abilityInventory.GetLevel(pick) + 1);
        shown[0].OnClickedSelect();
        yield return WaitFrames(2);
        Assert.AreEqual(1, king.pendingLevelUps, "단계 카드로는 레벨업이 소모되지 않음");
        Assert.IsTrue(canvas.gameObject.activeSelf, "단계 카드 다음 미니언 강화 카드로 다시 열려야 함");
        yield return LevelUpRunePlayTest.PickMinionEnhanceCard();
        Assert.AreEqual(0, king.pendingLevelUps);
    }
}

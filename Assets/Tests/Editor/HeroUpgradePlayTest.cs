using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 용사 강화 능력(HeroUpgradeSystem)이 실제 인게임에서 동작하는지: 값은 SetForTest로 직접 지정 (확률 능력은 100%로)
public class HeroUpgradePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator HeroUpgradeAbilities_Work()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        try
        {
            // ── 시작 효과: 주사위 코인 예약 · 막힌 칸 해제 · 룬 보상 ──
            current = "시작 효과";
            int bonusDice = king.bonusDiceCoinNextBreak;
            int blocked = TileManager.Instance.tiles.Count(t => t.isBlocked);
            int runes = king.pendingRuneRewards;
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.StartDiceCoin, 3);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.StartUnblockTile, 1);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.StartRuneChance, 100);
            HeroUpgradeSystem.ApplyStartEffects();
            Assert.AreEqual(bonusDice + 3, king.bonusDiceCoinNextBreak, "시작 주사위 코인 = 다음 브레이크타임 보너스");
            if (blocked > 0)
                Assert.AreEqual(blocked - 1, TileManager.Instance.tiles.Count(t => t.isBlocked), "막힌 칸 1칸 해제");
            Assert.AreEqual(runes + 1, king.pendingRuneRewards, "시작 룬 보상 100%");
            king.pendingRuneRewards = runes;

            // ── 카드 Lv.2 확률 ──
            current = "카드 확률";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.OfferLv2Chance, 0);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.FirstOfferLv2Chance, 100);
            Assert.AreEqual(1, MinionOfferSystem.RollOfferMergeLevel(true), "첫 지급 Lv.2 100%");
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.OfferLv2Chance, 100);
            Assert.AreEqual(1, MinionOfferSystem.RollOfferMergeLevel(false), "Lv.2 카드 100%");
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.OfferLv2Chance, 0);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.FirstOfferLv2Chance, 0);

            // ── 카드 가격 -1 (가격 2 이상만) ──
            current = "카드 가격 할인";
            string costKey = MinionManager.Instance.minionDataDic.Values.First(d => d.offerDiceCoinPrice > 1).key;
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.MinionCostDownChance, 100);
            var cheap = new MinionOfferSystem.OfferCard(costKey, 0, MinionOfferSystem.RollCardDiscount(costKey));
            Assert.AreEqual(MinionOfferSystem.GetCardPrice(costKey) - 1, cheap.Price, "100% → 가격 -1");
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.MinionCostDownChance, 0);
            Assert.AreEqual(0, MinionOfferSystem.RollCardDiscount(costKey), "0% → 할인 없음");

            // ── 카드 다시 뽑기 (무료 1회) ──
            current = "다시 뽑기";
            MinionOfferSystem offer = MinionOfferSystem.Instance;
            Assert.IsNotNull(offer, "MinionOfferSystem 없음");
            Assert.IsTrue(GameManager.Instance.isBreakTime, "게임 시작 = 브레이크타임");
            offer.freeRerolls = 1;
            var before = offer.hand.ToList();
            offer.OnClickedReroll();
            Assert.AreEqual(0, offer.freeRerolls, "다시 뽑기 소모");
            Assert.AreEqual(MinionOfferSystem.OFFER_COUNT, offer.hand.Count(c => c != null), "새 카드 3장");
            Assert.IsFalse(before.Where(c => c != null).Any(c => offer.hand.Contains(c)), "이전 카드가 남아 있음");
            // (2026-11-25) 보너스가 없으면 한 판 무료 다시 뽑기(2번) → 광고 (RerollCounter.BreakOffer)
            int freeLeft = RerollCounter.BreakOffer.FreeLeft;
            before = offer.hand.ToList();
            offer.OnClickedReroll();
            Assert.AreEqual(freeLeft - 1, RerollCounter.BreakOffer.FreeLeft, "한 판 무료 다시 뽑기 소모");
            Assert.AreEqual(MinionOfferSystem.OFFER_COUNT, offer.hand.Count(c => c != null), "새 카드 3장");
            Assert.IsFalse(before.Where(c => c != null).Any(c => offer.hand.Contains(c)), "이전 카드가 남아 있음");

            // ── 엘리트·보스 처치 주사위 ──
            current = "처치 주사위";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.EliteBossDiceChance, 100);
            // 보상 주사위 코인은 브레이크타임 지급이 끝났으면 즉시, 아니면 다음 브레이크타임 (KingSlime.AddRewardDiceCoin) → 합계로 확인
            int dice = king.bonusDiceCoinNextBreak + king.diceCoin;
            HeroUpgradeSystem.OnEliteBossKilled(king.transform.position);
            Assert.AreEqual(dice + 1, king.bonusDiceCoinNextBreak + king.diceCoin, "처치 주사위 100%");

            // ── 경험치 구매 보너스 ──
            current = "경험치 구매";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.ExpBuyBonusChance, 100);
            king.AddDiceCoin(GameSetting.BUY_EXP_DICE_COIN_PRICE);
            int lv = king.lv, exp = king.exp;
            Assert.IsTrue(king.TryBuyExp(), "경험치 구매 실패");
            int gained = (king.lv - lv) > 0 ? -1 : king.exp - exp;
            if (gained >= 0)
                Assert.AreEqual(GameSetting.BUY_EXP_AMOUNT + 1, gained, "경험치 +1 더");
            else
                Assert.Greater(king.lv, lv, "레벨업으로 넘어감");

            // ── 같은 종 이웃 공격력 ──
            current = "쌍둥이";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.TwinAttack, 50);
            Tile a = TileManager.Instance.tiles.FirstOrDefault(t => t.IsEmpty && TileManager.Instance.GetTilesAround(t, new[] { Vector2Int.right })
                .Any(n => n.IsEmpty && n.tileGroup == t.tileGroup));
            Assert.IsNotNull(a, "이웃이 빈 칸 없음");
            Tile b = TileManager.Instance.GetTilesAround(a, new[] { Vector2Int.right }).First(n => n.IsEmpty && n.tileGroup == a.tileGroup);
            Minion m1 = SpawnMinion("Iron", a, 1);
            yield return WaitFrames(2);
            float alone = m1.statDic[StatType.AttackPower];
            Minion m2 = SpawnMinion("Iron", b, 0);
            yield return WaitFrames(2);
            Assert.AreEqual(alone * 1.5f, m1.statDic[StatType.AttackPower], alone * 0.001f, "같은 종이 붙으면 공격력 +50%");
            MinionFieldController.Instance.RemoveFieldMinion(m2);
            yield return WaitFrames(2);
            Assert.AreEqual(alone, m1.statDic[StatType.AttackPower], alone * 0.001f, "이웃이 사라지면 원래대로");
            MinionFieldController.Instance.RemoveFieldMinion(m1);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.TwinAttack, 0);
            yield return WaitFrames(2);

            // ── 진화 공격력 · 형태 다양 공속 ──
            current = "진화·공속";
            Tile c = GetFreeTile();
            Minion evo = SpawnMinion("Iron", c, MinionData.MAX_MERGE_LEVEL);
            yield return WaitFrames(3);
            if (!evo.isEvolved)
                evo.Evolve();
            float evoAttack = evo.statDic[StatType.AttackPower];
            float speed = evo.statDic[StatType.AttackSpeed];
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.EvolvedAttack, 20);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.FormVarietyAttackSpeed, 10);
            HeroUpgradeSystem.SetConditionsForTest(true, false);
            evo.UpdateMinion();
            Assert.AreEqual(evoAttack * 1.2f, evo.statDic[StatType.AttackPower], evoAttack * 0.001f, "진화 공격력 +20%");
            Assert.AreEqual(speed * 1.1f, evo.statDic[StatType.AttackSpeed], speed * 0.001f, "형태 다양 공속 +10%");
            MinionFieldController.Instance.RemoveFieldMinion(evo);
            yield return WaitFrames(2);

            // ── 약점 배율 ──
            current = "약점";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.PureTypeWeakness, 10);
            HeroUpgradeSystem.SetConditionsForTest(false, true);
            Assert.AreEqual(1.5f * 1.1f, HeroUpgradeSystem.ApplyWeakness(1.5f), 0.0001f, "약점 배율 +10%");
            Assert.AreEqual(0.5f, HeroUpgradeSystem.ApplyWeakness(0.5f), 0.0001f, "내성엔 적용 안 함");

            // ── 병과: 원거리 공속 · 근거리 공격력 · 마법 공격력 ──
            current = "병과";
            HeroUpgradeSystem.SetConditionsForTest(false, false);
            foreach (var (key, check) in new (string, string)[] { ("Pierce", "ranged"), ("Berserker", "melee"), ("Orbit", "melee"), ("Flame", "magic"), ("Mortar", "none"), ("Mine", "none") })
            {
                Tile t = GetFreeTile();
                Minion m = SpawnMinion(key, t, 0);
                yield return WaitFrames(2);
                Assert.Greater(m.statDic[StatType.AttackPower], 0f, $"{key} 공격력 0 (MinionData.csv AttackPower)");
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.RangedAttackSpeed, 10);
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.MeleeAttack, 20);
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.MagicAttack, 30);
                // 데이터 공격력이 0인 미니언도 있어서 배율로 확인
                float atkMul = HeroUpgradeSystem.GetMinionAttackMul(m), spdMul = HeroUpgradeSystem.GetMinionAttackSpeedMul(m);
                if (check == "ranged") { Assert.AreEqual(1.1f, spdMul, 0.001f, $"{key} 원거리 공속"); }
                if (check == "melee") { Assert.AreEqual(1.2f, atkMul, 0.001f, $"{key} 근거리 공격력"); Assert.AreEqual(1f, spdMul, 0.001f, $"{key} 근거리는 원거리 공속 없음"); }
                if (check == "magic") { Assert.AreEqual(1.3f, atkMul, 0.001f, $"{key} 마법 공격력"); }
                MinionFieldController.Instance.RemoveFieldMinion(m);
                yield return WaitFrames(2);
            }
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.RangedAttackSpeed, 0);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.MeleeAttack, 0);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.MagicAttack, 0);

            // ── 상성: 내성 완화 · 공격 타입 → 방어 타입 ──
            current = "상성";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.PureTypeWeakness, 0);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.ResistEase, 10);
            Assert.AreEqual(0.6f, HeroUpgradeSystem.ApplyDefenceMultiplier(0.5f, AttackType.Physic, DefenceType.Armored), 0.0001f, "내성 0.5 → 0.6");
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.ResistEase, 0);
            HeroUpgradeSystem.SetVersusForTest(AttackType.Vibration, DefenceType.Light, 5);
            Assert.AreEqual(0.75f * 1.05f, HeroUpgradeSystem.ApplyDefenceMultiplier(0.75f, AttackType.Vibration, DefenceType.Light), 0.0001f, "진동 → 경갑 +5%");
            Assert.AreEqual(0.75f, HeroUpgradeSystem.ApplyDefenceMultiplier(0.75f, AttackType.Magic, DefenceType.Light), 0.0001f, "다른 공격 타입은 그대로");

            // ── 정예병 칸 · 킹 피격 (용사가 있을 때만 정예병 칸이 생김) ──
            current = "정예병";
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.EliteMinionAttack, 10);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.KingHitEliteRage, 20);
            var area = EliteArea.GetOrCreate(king);
            object src = new object();
            area.SetShape(src, new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down });
            area.SetEliteBuffs(src, new[] { new Buff(StatType.CritChance, 0f, StatOpType.Add) });
            Tile eliteTile = area.GetEliteTiles().FirstOrDefault(t => t.IsEmpty);
            if (eliteTile != null)
            {
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.EliteMinionAttack, 0);
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.KingHitEliteRage, 0);
                Minion em = SpawnMinion("Iron", eliteTile, 0);
                yield return WaitFrames(2);
                float baseAtk = em.statDic[StatType.AttackPower];
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.EliteMinionAttack, 10);
                HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.KingHitEliteRage, 20);
                area.Refresh();
                Assert.AreEqual(baseAtk * 1.1f, em.statDic[StatType.AttackPower], baseAtk * 0.001f, "정예병 공격력 +10%");
                king.health.curHp = king.health.MaxHp;
                king.TakeDamage(new DamageData { damage = 1f, attackType = AttackType.Chaos });
                Assert.AreEqual(baseAtk * 1.1f * 1.2f, em.statDic[StatType.AttackPower], baseAtk * 0.001f, "킹 피격 → 정예병 공격력 +20%");
                MinionFieldController.Instance.RemoveFieldMinion(em);
                yield return WaitFrames(2);
            }
            area.RemoveShape(src);
            area.RemoveEliteBuffs(src);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.EliteMinionAttack, 0);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.KingHitEliteRage, 0);

            // ── 킹 체력: 낮은 체력 적 정지 · 불굴 ──
            current = "킹 체력";
            HeroUpgradeSystem.ResetOncePerRunForTest();
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.LowHpFreeze, 2);
            HeroUpgradeSystem.SetForTest(HeroUpgradeAbility.LastStand, 30);
            Enemy.ClearFreezeAll();
            float max = king.health.MaxHp;
            king.health.curHp = max;
            king.TakeDamage(new DamageData { damage = max * 0.8f, attackType = AttackType.Chaos });
            Assert.IsTrue(Enemy.IsFrozenAll, "체력 30% 아래 → 적 정지");
            king.TakeDamage(KILL);
            Assert.AreEqual(max * 0.3f, king.health.CurHp, 0.01f, "불굴: 체력 30%로 버팀");
            king.TakeDamage(KILL);
            Assert.AreEqual(0f, king.health.CurHp, "불굴은 판당 1번");
        }
        finally
        {
            HeroUpgradeSystem.End();
            Enemy.ClearFreezeAll();
        }
        yield break;
    }
}

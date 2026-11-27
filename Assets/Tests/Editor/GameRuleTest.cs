using System.Linq;
using NUnit.Framework;

// 기획 규칙 테스트 (CLAUDE.md 기준)
public class WaveTypeTest
{
    // waveIdx는 0부터, wave 번호 = waveIdx + 1
    [TestCase(0, WaveType.Battle)]
    [TestCase(3, WaveType.Battle)]
    [TestCase(4, WaveType.Elite)]   // 5번째
    [TestCase(9, WaveType.Elite)]   // 10번째
    [TestCase(14, WaveType.Elite)]  // 15번째
    [TestCase(19, WaveType.Elite)]  // 20번째 (2026-11-26 보스 25마다)
    [TestCase(24, WaveType.Boss)]   // 25번째: 5의 배수지만 보스 우선
    [TestCase(25, WaveType.Battle)]
    [TestCase(29, WaveType.Elite)]
    [TestCase(49, WaveType.Boss)]   // 50번째 = 마지막 보스
    public void GetWaveType(int waveIdx, WaveType expected)
    {
        Assert.AreEqual(expected, GameManager.GetWaveType(waveIdx));
    }

    // 스킵: 그 wave의 적이 모두 소환된 뒤부터, 보스 wave는 불가
    [TestCase(WaveType.Battle, false, false)]
    [TestCase(WaveType.Battle, true, true)]
    [TestCase(WaveType.Elite, false, false)]
    [TestCase(WaveType.Elite, true, true)]
    [TestCase(WaveType.Boss, true, false)]
    public void CanSkipWave(WaveType waveType, bool isSpawnEnd, bool expected)
    {
        Assert.AreEqual(expected, GameManager.CanSkipWave(waveType, isSpawnEnd));
    }

    // 기본 순서(StageData.DEFAULT_DEFENCE_WAVE_TYPES) = Light/Normal/Ethereal/Light/Normal/Armored 반복
    [TestCase(0, DefenceType.Light)]
    [TestCase(1, DefenceType.Normal)]
    [TestCase(2, DefenceType.Ethereal)]  // 3번째
    [TestCase(3, DefenceType.Light)]
    [TestCase(5, DefenceType.Armored)]   // 6번째
    [TestCase(8, DefenceType.Ethereal)]  // 9번째
    [TestCase(11, DefenceType.Armored)]  // 12번째
    [TestCase(19, DefenceType.Normal)]   // 20번째 (보스 wave)
    public void GetWaveDefenceType(int waveIdx, DefenceType expected)
    {
        Assert.AreEqual(expected, StageData.GetDefenceType(null, waveIdx));
    }

    // 데이터 순서를 wave 1부터 반복, Random은 같은 wave면 항상 같은 값
    [Test]
    public void GetDefenceTypeCycle()
    {
        var cycle = new[] { DefenceWaveType.Armored, DefenceWaveType.Normal };
        Assert.AreEqual(DefenceType.Armored, StageData.GetDefenceType(cycle, 0));
        Assert.AreEqual(DefenceType.Normal, StageData.GetDefenceType(cycle, 1));
        Assert.AreEqual(DefenceType.Armored, StageData.GetDefenceType(cycle, 4));
        Assert.AreEqual(DefenceType.Light, StageData.GetDefenceType(new[] { DefenceWaveType.Light }, 0));
        var random = new[] { DefenceWaveType.Random };
        for (int i = 0; i < 10; i++)
            Assert.AreEqual(StageData.GetDefenceType(random, i), StageData.GetDefenceType(random, i));
    }

    // 기본 배율 (Resources/DefenceInfo 에셋이 없을 때. 에셋 값은 DefenceTypePlayTest가 실제 전투로 확인)
    // (2026-11-25 사용자 변경) 약점 1.3 / 내성 0.7
    [TestCase(AttackType.Physic, DefenceType.Armored, 0.7f)]
    [TestCase(AttackType.Magic, DefenceType.Armored, 1.3f)]
    [TestCase(AttackType.Physic, DefenceType.Ethereal, 1.3f)]
    [TestCase(AttackType.Magic, DefenceType.Ethereal, 0.7f)]
    [TestCase(AttackType.Physic, DefenceType.Light, 1.3f)]
    [TestCase(AttackType.Vibration, DefenceType.Light, 0.7f)]
    [TestCase(AttackType.Magic, DefenceType.Normal, 0.7f)]
    [TestCase(AttackType.Vibration, DefenceType.Normal, 1.3f)]
    [TestCase(AttackType.Chaos, DefenceType.Armored, 1f)]
    public void DefenceDefaultMultiplier(AttackType attack, DefenceType defence, float expected)
    {
        Assert.AreEqual(expected, DefenceInfo.GetDefault(attack, defence), 0.0001f);
    }

    // 아군 최소 피해: 1 미만이면 1, 원래 피해 0은 0
    [TestCase(3f, 0.6f, 1f)]
    [TestCase(3f, 0f, 1f)]
    [TestCase(5f, 2.5f, 2.5f)]
    [TestCase(0f, 0f, 0f)]
    public void AllyMinDamage(float baseDamage, float finalDamage, float expected)
    {
        Assert.AreEqual(expected, AllyUnitDamageData.GetMinGuaranteedDamage(baseDamage, finalDamage), 0.0001f);
    }
}

public class StageUnlockTest
{
    // 낮은 난이도를 클리어(마지막 wave 60 보스 처치)해야 다음 난이도 해금. 도달만으로는 안 열림
    [Test]
    public void UnlockNeedsClear()
    {
        Assert.IsFalse(StageData.IsUnlocked(null));
        Assert.IsFalse(StageData.IsUnlocked(new UserStage { maxWave = GameSetting.LAST_WAVE - 1 }));
        Assert.IsTrue(StageData.IsUnlocked(new UserStage { maxWave = GameSetting.LAST_WAVE - 1, isClear = true }));
    }

    // 일반 적 처치 코인: 20 wave마다 +1 (표시 wave 1~20 = 1, 21 = 2, 80 = 4)
    [TestCase(0, 1)]
    [TestCase(19, 1)]
    [TestCase(20, 2)]
    [TestCase(39, 2)]
    [TestCase(40, 3)]
    [TestCase(79, 4)]
    public void KillGoldPer20Waves(int waveIdx, int expected)
    {
        Assert.AreEqual(expected, NormalEnemy.GetKillGold(waveIdx));
    }

    // 인게임 슬라임 레벨
    // (2026-11-22) 필요 경험치 = 2 + 레벨
    [TestCase(0, 2)]
    // (2026-11-26) GameSetting.SLIME_LEVEL_EXPS = 2,2,3,3,4,4,5,5,6,6,7,7 (최대 Lv.12, 그 이상은 마지막 값)
    [TestCase(1, 2)]
    [TestCase(3, 3)]
    [TestCase(6, 5)]
    [TestCase(8, 6)]
    [TestCase(10, 7)]
    [TestCase(20, 7)]
    public void SlimeMaxExp(int lv, int expected)
    {
        Assert.AreEqual(expected, Slime.GetMaxExp(lv));
    }

    // (2026-11-23) wave 카드 Lv.2 확률 = 슬라임 레벨 × 1%
    [TestCase(0, 0f)]
    [TestCase(1, 1f)]
    [TestCase(2, 2f)]
    [TestCase(9, 9f)]
    public void OfferLevelUpChance_BySlimeLevel(int slimeLv, float expected)
    {
        Assert.AreEqual(expected, MinionOfferSystem.GetOfferLevelUpChance(slimeLv), 0.0001f);
    }


    // 빙결: 머지 레벨별 배율 0.7~0.3, 다시 걸리면 강한 배율 유지 + 시간은 긴 쪽 (0.3 3초 → 2초 후 0.7 3초 = 0.3, 남은 3초)
    [Test]
    public void IceSlow_KeepStrongerAndLonger()
    {
        // (2026-11-25 사용자 변경) 머지 레벨별 이동속도 배율 0.6~0.2 = 40~80% 감속
        CollectionAssert.AreEqual(new[] { 0.6f, 0.5f, 0.4f, 0.3f, 0.2f }, UnityEngine.ScriptableObject.CreateInstance<IceMinionData>().slowRate);
        Assert.AreEqual(0.3f, IceEffect.Merge(0.3f, 0.7f));
        Assert.AreEqual(0.3f, IceEffect.Merge(0.7f, 0.3f));
        Assert.AreEqual(3f, IceEffect.MergeRemain(1f, 3f));
        Assert.AreEqual(2.5f, IceEffect.MergeRemain(2.5f, 1f));
    }

    // (2026-11-26) 마지막 wave = 50 (waveIdx 49), 보스 wave (보스 = 25·50)
    [Test]
    public void LastWaveIs50Boss()
    {
        Assert.AreEqual(50, GameSetting.LAST_WAVE);
        Assert.IsFalse(GameManager.IsLastWave(48));
        Assert.IsTrue(GameManager.IsLastWave(49));
        Assert.AreEqual(WaveType.Boss, GameManager.GetWaveType(49));
        Assert.AreEqual(50, GameSetting.FINAL_BOSS_WAVE_NORMAL_COUNT);
        Assert.AreEqual(2f, GameSetting.FINAL_BOSS_WAVE_SPAWN_INTERVAL);
    }
}

public class MergeRuleTest
{
    const int MAX = MinionData.MAX_MERGE_LEVEL;

    [Test]
    public void SameKeySameLv_CanMerge()
    {
        Assert.IsTrue(Minion.CanMerge("Ice", 0, 1, "Ice", 0, 1));
        Assert.IsTrue(Minion.CanMerge("Ice", MAX - 1, 1, "Ice", MAX - 1, 1));
    }

    [Test]
    public void DifferentKey_CannotMerge()
    {
        Assert.IsFalse(Minion.CanMerge("Ice", 0, 1, "Flame", 0, 1));
    }

    [Test]
    public void DifferentLv_CannotMerge()
    {
        Assert.IsFalse(Minion.CanMerge("Ice", 0, 1, "Ice", 1, 1));
    }

    [Test]
    public void MaxLv_CannotMerge()
    {
        Assert.IsFalse(Minion.CanMerge("Ice", MAX, 1, "Ice", MAX, 1));
    }

    [Test]
    public void Sacrifice_AnyKeySameLv_CanMerge()
    {
        Assert.IsTrue(SacrificeMinion.CanSacrificeMerge(0, 1, 0, 1));
        Assert.IsTrue(SacrificeMinion.CanSacrificeMerge(MAX - 1, 1, MAX - 1, 1));
    }

    [Test]
    public void Sacrifice_DifferentLv_CannotMerge()
    {
        Assert.IsFalse(SacrificeMinion.CanSacrificeMerge(0, 1, 1, 1));
    }

    [Test]
    public void Sacrifice_MaxLv_CannotMerge()
    {
        Assert.IsFalse(SacrificeMinion.CanSacrificeMerge(MAX, 1, MAX, 1));
    }
}

// 슬라임 확률 연속 실패 보정: 연속 4회 실패 후 다음 시도부터 성공 +50%p, 성공하면 초기화
public class MinionChancePityTest
{
    [TestCase(0.3f, 0, 0.3f)]
    [TestCase(0.3f, 3, 0.3f)]   // 3연속 실패까지는 그대로
    [TestCase(0.3f, 4, 0.8f)]   // 4연속 실패 후 +50%p
    [TestCase(0.3f, 6, 0.8f)]   // 성공할 때까지 유지
    [TestCase(0.7f, 4, 1f)]     // 최대 100%
    public void GetChance(float baseChance, int failStreak, float expected)
    {
        Assert.AreEqual(expected, MinionChancePity.GetChance(baseChance, failStreak), 0.0001f);
    }

    [Test]
    public void FailStreak_ResetOnSuccess()
    {
        int streak = 0;
        for (int i = 0; i < 3; i++)
            streak = MinionChancePity.NextFailStreak(streak, false);
        Assert.AreEqual(3, streak);
        Assert.AreEqual(0, MinionChancePity.NextFailStreak(streak, true)); // 3연속 실패 후 성공 → 초기화
        Assert.AreEqual(5, MinionChancePity.NextFailStreak(4, false));
    }

    [Test]
    public void Roll_ZeroOrFullChance_NoStreakChange()
    {
        MinionChancePity pity = new MinionChancePity();
        Assert.IsFalse(pity.Roll("Joker", 0f));
        Assert.IsTrue(pity.Roll("Joker", 1f));
        Assert.AreEqual(0, pity.GetFailStreak("Joker"));
    }
}

public class UserLevelTest
{
    // 최대 레벨(30)이면 경험치를 받지 않음
    [TestCase(0, 10, 10)]
    [TestCase(29, 10, 10)]
    [TestCase(30, 10, 0)]
    [TestCase(31, 10, 0)]
    [TestCase(5, -3, 0)]
    public void GetGainExp(int lv, int exp, int expected)
    {
        Assert.AreEqual(30, UserMyInfoManager.MAX_LEVEL);
        Assert.AreEqual(expected, UserMyInfoManager.GetGainExp(lv, exp));
    }
}

// 슬라임 조합 프리셋: 프리셋 key → 실제 장착 key 정리
public class MinionPresetTest
{
    static readonly string[] Current = { "A", "B", "C", "D", "E" };
    static readonly string[] Owned = { "A", "B", "C", "D", "E", "F", "G" };

    [Test]
    public void ValidPresetUsedAsIs()
    {
        string[] preset = { "F", "G", "A", "B", "C" };
        CollectionAssert.AreEqual(preset, UserMinionManager.ResolvePresetKeys(preset, Current, Owned));
    }

    [Test]
    public void UnownedKeyReplacedByCurrentSlotKey()
    {
        string[] preset = { "X", "G", "A", "B", "C" };
        // 0번 칸 X 미보유 → 현재 0번 칸 A는 이미 사용 → 현재 장착 중 남은 D
        CollectionAssert.AreEqual(new[] { "D", "G", "A", "B", "C" },
            UserMinionManager.ResolvePresetKeys(preset, Current, Owned));
    }

    [Test]
    public void DuplicateAndEmptyKeysReplaced()
    {
        string[] preset = { "F", "F", null, "B", "C" };
        string[] result = UserMinionManager.ResolvePresetKeys(preset, Current, Owned);
        Assert.AreEqual(5, result.Length);
        CollectionAssert.AllItemsAreUnique(result);
        CollectionAssert.AllItemsAreNotNull(result);
        Assert.AreEqual("F", result[0]);
        Assert.AreEqual("A", result[1]); // 현재 1번 칸 B는 3번 칸에서 사용 → 현재 장착 중 남은 A
    }
}

// 엘리트 처치 골드 = (wave 번호 / 5) × 30
public class EliteRewardTest
{
    [Test]
    public void EliteGoldScalesWithWave()
    {
        Assert.AreEqual(30, EliteEnemy.GetEliteRewardGold(4));  // wave 5
        Assert.AreEqual(60, EliteEnemy.GetEliteRewardGold(9));  // wave 10
        Assert.AreEqual(90, EliteEnemy.GetEliteRewardGold(14)); // wave 15
        Assert.AreEqual(30, EliteEnemy.GetEliteRewardGold(0));  // 최소 1단계
    }
}

// 에너지: 게임 시작에 5 소모, 10분마다 1 충전
public class EnergeRuleTest
{
    [Test]
    public void PlayNeedsCostEnerge()
    {
        Assert.AreEqual(5, GameSetting.PLAY_COST_ENERGE);
        Assert.IsTrue(EnergeManager.CanPlay(5));
        Assert.IsFalse(EnergeManager.CanPlay(4));
    }

    [Test]
    public void ChargesOnePerFiveMinutes()
    {
        Assert.AreEqual(5, EnergeManager.CHARGE_INTERVAL_MINUTES);
        Assert.AreEqual(0, EnergeManager.GetChargeCount(4.9));
        Assert.AreEqual(1, EnergeManager.GetChargeCount(5));
        Assert.AreEqual(7, EnergeManager.GetChargeCount(35));
        Assert.AreEqual(0, EnergeManager.GetChargeCount(-20)); // 기기 시간을 되돌린 경우
    }

    // 자리 비운 동안 충전: 최대(30)까지만, 이미 넘었으면 0
    [Test]
    public void OfflineChargeClampsToMax()
    {
        Assert.AreEqual(3, EnergeManager.GetAddableCount(10, 3));
        Assert.AreEqual(2, EnergeManager.GetAddableCount(GameSetting.MAX_ENERGE - 2, 100));
        Assert.AreEqual(0, EnergeManager.GetAddableCount(GameSetting.MAX_ENERGE + 5, 3));
    }

    // 다크니스 변환: 성공 시 10% 확률로 머지 레벨 +1, 최대 레벨이면 그대로
    [Test]
    public void Darkness_TransformMergeLevel()
    {
        Assert.AreEqual(1, DarknessMinion.GetTransformMergeLevel(1, false));
        Assert.AreEqual(2, DarknessMinion.GetTransformMergeLevel(1, true));
        Assert.AreEqual(MinionData.MAX_MERGE_LEVEL, DarknessMinion.GetTransformMergeLevel(MinionData.MAX_MERGE_LEVEL, true));
        Assert.AreEqual(0.7f, DarknessMinion.TRANSFORM_SUCCESS_CHANCE, 0.0001f);
        Assert.AreEqual(0.1f, DarknessMinion.TRANSFORM_GROW_CHANCE, 0.0001f);
    }
}

// (2026-11-23) 미니언 판매 = 표시 머지 레벨만큼 주사위 코인 (다음 브레이크타임)
public class SellMinionRefundTest
{
    [Test]
    public void Refund_IsDisplayMergeLevel()
    {
        Assert.AreEqual(1, SellMinionButton.GetRefundDiceCoin(0));
        Assert.AreEqual(2, SellMinionButton.GetRefundDiceCoin(1));
        Assert.AreEqual(3, SellMinionButton.GetRefundDiceCoin(2));
        Assert.AreEqual(5, SellMinionButton.GetRefundDiceCoin(MinionData.MAX_MERGE_LEVEL));
    }
}

// (2026-11-27) 판매 상품 4개 (광고 제거 · 게임 속도 · 기본 게임 팩 · 용사 패스), 스테이지 이용권 삭제
public class ProductRuleTest
{
    [Test]
    public void Products_AreFour()
    {
        CollectionAssert.AreEquivalent(
            new[] { InAppProductInfo.REMOVE_AD_KEY, InAppProductInfo.GAME_SPEED_UP_KEY, InAppProductInfo.BASE_GAME_PACK_KEY, InAppProductInfo.HERO_PASS_KEY },
            InAppProductInfo.PRODUCTS.Select(p => p.key).ToArray());
    }

    // 기본 게임 팩 = 광고 제거 + 게임 속도 효과
    [Test]
    public void BaseGamePack_GivesRemoveAdAndSpeed()
    {
        CollectionAssert.Contains(InAppProductInfo.REMOVE_AD_KEYS, InAppProductInfo.BASE_GAME_PACK_KEY);
        CollectionAssert.Contains(InAppProductInfo.GAME_SPEED_UP_KEYS, InAppProductInfo.BASE_GAME_PACK_KEY);
    }

    // 기본 게임 팩 = 구매 시각부터 BASE_GAME_PACK_HERO_DAYS일 용사 패스
    [Test]
    public void BaseGamePack_HeroDays()
    {
        var bought = new System.DateTime(2026, 11, 1, 12, 0, 0);
        string time = TimeManager.DateTimeToString(bought);
        Assert.IsTrue(HeroPass.IsBundleActiveAt(time, bought.AddDays(InAppProductInfo.BASE_GAME_PACK_HERO_DAYS - 1)));
        Assert.IsFalse(HeroPass.IsBundleActiveAt(time, bought.AddDays(InAppProductInfo.BASE_GAME_PACK_HERO_DAYS + 1)));
        Assert.IsFalse(HeroPass.IsBundleActiveAt("", bought));
    }

    // (2026-11-27) 용사 패스 = 월 구독 (ProductData.csv), 구독 확인 후 OFFLINE_GRACE_HOURS 동안 유효
    [Test]
    public void HeroPass_IsSubscription()
    {
        var info = InAppProductInfo.GetByKey(InAppProductInfo.HERO_PASS_KEY);
        Assert.IsNotNull(info, "ProductData.csv에 HeroPass 행 없음");
        Assert.AreEqual(UnityEngine.Purchasing.ProductType.Subscription, info.productType);
        Assert.IsFalse(string.IsNullOrEmpty(info.productId));
    }

    [Test]
    public void HeroPass_ActiveUntil()
    {
        var now = new System.DateTime(2026, 11, 27, 0, 0, 0, System.DateTimeKind.Utc);
        Assert.IsTrue(HeroPass.IsActiveAt(now.AddHours(1).Ticks.ToString(), now), "유효 기간 안");
        Assert.IsFalse(HeroPass.IsActiveAt(now.AddHours(-1).Ticks.ToString(), now), "유효 기간 지남");
        Assert.IsFalse(HeroPass.IsActiveAt("", now), "확인 기록 없음");
    }

    [Test]
    public void HeroPass_OwnedHeroes()
    {
        var table = HeroDataTable.Instance;
        Assume.That(table != null, "HeroData 없음");
        var mgr = new UserHeroManager();
        typeof(UserHeroManager).GetProperty("userHeroData").SetValue(mgr, new UserHeroData());
        var passHero = table.heroDatas.FirstOrDefault(h => h != null && !h.hidden && !h.isFree);
        Assume.That(passHero != null, "패스 용사(isFree가 아닌 용사) 없음");
        try
        {
            HeroPass.ForceActiveForTest = false;
            Assert.IsFalse(mgr.IsOwned(passHero.key), "패스 없으면 패스 용사 못 씀");
            HeroPass.ForceActiveForTest = true;
            Assert.IsTrue(mgr.IsOwned(passHero.key), "패스 있으면 패스 용사 이용");
        }
        finally { HeroPass.ForceActiveForTest = null; }
    }
}

// (2026-11-24) 브레이크타임 경로 표시: 출발 위치가 가깝고 다음 지점부터 같은 길이면 한 줄
public class EnemyPathPreviewRuleTest
{
    static System.Collections.Generic.List<UnityEngine.Vector2> Route(float startX, float startY)
        => new() { new(startX, startY), new(-6.25f, 0f), new(0f, 0f) };

    [Test]
    public void NearStarts_MergeIntoOne()
    {
        var routes = new System.Collections.Generic.List<System.Collections.Generic.List<UnityEngine.Vector2>> { Route(-6.1f, 6.25f), Route(-6.45f, 6.25f) };
        var merged = EnemyPathPreview.MergeNearStarts(routes, 1f);
        Assert.AreEqual(1, merged.Count);
        Assert.AreEqual(-6.275f, merged[0][0].x, 0.001f);
    }

    [Test]
    public void FarStarts_StaySeparate()
    {
        var routes = new System.Collections.Generic.List<System.Collections.Generic.List<UnityEngine.Vector2>> { Route(-6.1f, 6.25f), Route(3f, 6.25f) };
        Assert.AreEqual(2, EnemyPathPreview.MergeNearStarts(routes, 1f).Count);
    }
}

// (2026-11-24) 화면 드래그 카메라 이동: 맵이 화면 안에 들어오면 안 움직임, 크면 화면이 맵 밖으로 안 나가게
public class CameraPanRuleTest
{
    [Test]
    public void MapFitsScreen_DoesNotMove()
    {
        // 화면 반폭 7.32, 맵 -7 ~ 7 → 어디로 끌어도 처음 위치(0)
        Assert.AreEqual(0f, CameraManager.ClampAxis(5f, 0f, 7.32f, -7f, 7f), 0.0001f);
        Assert.AreEqual(0f, CameraManager.ClampAxis(-5f, 0f, 7.32f, -7f, 7f), 0.0001f);
    }

    [Test]
    public void MapWiderThanScreen_ClampsToEdges()
    {
        // 맵 -12 ~ 12, 화면 반폭 7.32 → 카메라 x는 -4.68 ~ 4.68
        Assert.AreEqual(3f, CameraManager.ClampAxis(3f, 0f, 7.32f, -12f, 12f), 0.0001f);
        Assert.AreEqual(4.68f, CameraManager.ClampAxis(10f, 0f, 7.32f, -12f, 12f), 0.0001f);
        Assert.AreEqual(-4.68f, CameraManager.ClampAxis(-10f, 0f, 7.32f, -12f, 12f), 0.0001f);
    }
}

// (2026-11-25) 용사 에너지 최대 = 2 + 에너지 보유 증가 카드 (슬라임 레벨 자동 증가 삭제 → HeroEnergyMaxRuleTest 삭제)

// (2026-11-25) 레벨업 미니언 강화 카드 (종 × 스탯)
public class MinionEnhanceRuleTest
{
    [TearDown] public void TearDown() => MinionEnhanceSystem.Clear();

    [Test]
    public void PickChoices_DistinctAndSkipMax()
    {
        MinionEnhanceSystem.Clear();
        var keys = new[] { "Bounce", "Iron" };
        for (int seed = 0; seed < 30; seed++)
        {
            var picks = MinionEnhanceSystem.PickChoices(keys, null, 3, new System.Random(seed));
            Assert.AreEqual(3, picks.Count);
            Assert.AreEqual(3, picks.Distinct().Count(), "같은 조합이 겹침");
            Assert.IsTrue(picks.All(p => keys.Contains(p.key)));
        }
        // Bounce 스탯 모두 최대 → Iron만 나옴, 후보 = 스탯 수
        foreach (var s in MinionEnhanceSystem.STATS)
            MinionEnhanceSystem.SetLevel("Bounce", s.statType, MinionEnhanceSystem.MAX_LEVEL);
        var only = MinionEnhanceSystem.PickChoices(keys, null, 10, new System.Random(1));
        Assert.AreEqual(MinionEnhanceSystem.STATS.Length, only.Count);
        Assert.IsTrue(only.All(p => p.key == "Iron"));
    }

    [Test]
    public void Apply_ByKeyAndStat()
    {
        MinionEnhanceSystem.Clear();
        MinionEnhanceSystem.SetLevel("Bounce", StatType.AttackPower, 2);
        MinionEnhanceSystem.SetLevel("Bounce", StatType.AttackRange, 1);
        MinionEnhanceSystem.SetLevel("Bounce", StatType.CritChance, 3);
        MinionEnhanceSystem.SetLevel("Bounce", StatType.EliteBossDamage, 2);
        var dic = new System.Collections.Generic.Dictionary<StatType, float>
        {
            [StatType.AttackPower] = 100f, [StatType.AttackSpeed] = 50f, [StatType.AttackRange] = 2f, [StatType.CritChance] = 0f, [StatType.EliteBossDamage] = 0f,
        };
        MinionEnhanceSystem.Apply("Bounce", dic);
        Assert.AreEqual(130f, dic[StatType.AttackPower], 0.01f, "공격력 +15% × 2");
        Assert.AreEqual(50f, dic[StatType.AttackSpeed], 0.01f);
        Assert.AreEqual(2.3f, dic[StatType.AttackRange], 0.01f);
        Assert.AreEqual(15f, dic[StatType.CritChance], 0.01f);
        Assert.AreEqual(30f, dic[StatType.EliteBossDamage], 0.01f, "강적 피해 +15% × 2");
        var other = new System.Collections.Generic.Dictionary<StatType, float> { [StatType.AttackPower] = 100f };
        MinionEnhanceSystem.Apply("Iron", other);
        Assert.AreEqual(100f, other[StatType.AttackPower], 0.01f, "다른 종은 그대로");
        StringAssert.Contains("+30%", MinionEnhanceSystem.GetDescription(StatType.AttackPower, 2));
    }

    // (2026-11-25) 행운 카드: 10% 확률로 한 번에 2레벨, 최대 레벨을 넘으면 보통 카드
    [Test]
    public void LuckyCard_DoubleLevel()
    {
        MinionEnhanceSystem.Clear();
        Assert.AreEqual(3, MinionEnhanceSystem.RollAmount("Bounce", StatType.AttackPower, 0.5f), "1% 안 = 대박 3레벨");
        Assert.AreEqual(2, MinionEnhanceSystem.RollAmount("Bounce", StatType.AttackPower, 5f), "그다음 10% = 행운");
        MinionEnhanceSystem.SetLevel("Bounce", StatType.AttackPower, MinionEnhanceSystem.MAX_LEVEL - 2);
        Assert.AreEqual(2, MinionEnhanceSystem.RollAmount("Bounce", StatType.AttackPower, 0.5f), "3레벨이 안 남으면 대박 → 2레벨");
        MinionEnhanceSystem.SetLevel("Bounce", StatType.AttackPower, 0);
        Assert.AreEqual(1, MinionEnhanceSystem.RollAmount("Bounce", StatType.AttackPower, 50f), "10% 밖 = 보통");
        MinionEnhanceSystem.SetLevel("Bounce", StatType.AttackPower, MinionEnhanceSystem.MAX_LEVEL - 1);
        Assert.AreEqual(1, MinionEnhanceSystem.RollAmount("Bounce", StatType.AttackPower, 5f), "2레벨이 안 남으면 보통");
        MinionEnhanceSystem.SetLevel("Bounce", StatType.AttackPower, 1);
        MinionEnhanceSystem.Add("Bounce", StatType.AttackPower, 2);
        Assert.AreEqual(3, MinionEnhanceSystem.GetLevel("Bounce", StatType.AttackPower));
        // 설명: Lv.1 → Lv.3 = +15% → +45%
        string desc = MinionEnhanceSystem.GetDescription(StatType.AttackPower, 3, 2);
        StringAssert.Contains("+15%", desc);
        StringAssert.Contains("+45%", desc);
        StringAssert.Contains("+30%", MinionEnhanceSystem.GetDescription(StatType.AttackPower, 2, 2), "처음 고르는 행운 카드 = +30%");
    }
}

// (2026-11-25) 룬 수치 (RuneData 에셋 = CSV 반영 결과) + 둔화 3초·스턴 1초
public class RuneValueRuleTest
{
    static float[] Values(string key)
    {
        DataTableImporter.ImportChanged();
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<RuneData>($"Assets/AddressableResources/RuneData/Level1/{key}.asset");
        Assert.IsNotNull(data, key);
        return data.levelValues.ToArray();
    }

    [TestCase("AttackPowerTile", 1.5f, 2f, 2.5f)]
    [TestCase("AttackSpeedTile", 1.4f, 1.8f, 2.2f)]
    [TestCase("CritPowerTile", 15f, 30f, 45f)]
    [TestCase("RangeUpTile", 1f, 2f, 3f)]      // 2026-11-26 RuneData.csv
    [TestCase("GiantSlayerTile", 2f, 3f, 4f)]
    [TestCase("SlowTile", 25f, 40f, 70f)]
    [TestCase("StunTile", 10f, 20f, 30f)]     // 2026-11-26 RuneData.csv
    public void RuneLevelValues(string key, float lv1, float lv2, float lv3)
    {
        CollectionAssert.AreEqual(new[] { lv1, lv2, lv3 }, Values(key), key);
    }

    [Test]
    public void SlowStunDuration()
    {
        var slow = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/3.Prefabs/Rune/Level1/SlowTile.prefab").GetComponentInChildren<SlowTileRune>(true);
        var stun = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/3.Prefabs/Rune/Level1/StunTile.prefab").GetComponentInChildren<StunTileRune>(true);
        Assert.AreEqual(3f, slow.duration, 0.001f);
        Assert.AreEqual(1f, stun.duration, 0.001f);
    }
}

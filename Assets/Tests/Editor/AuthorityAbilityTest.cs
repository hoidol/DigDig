using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// 권능 능력 규칙 (씬 없이 계산되는 부분). 수치는 각 권능 클래스 const (CLAUDE.md "권능")
public class AuthorityAbilityTest
{
    // 모든 권능 데이터에 능력 클래스({Key}Authority)가 있어야 인게임에서 생성됨
    [Test]
    public void EveryAuthorityData_HasAuthorityClass()
    {
        var keys = UnityEditor.AssetDatabase.FindAssets("t:AuthorityData", new[] { "Assets/AddressableResources/AuthorityData" })
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<AuthorityData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)).key);
        var missing = keys.Where(k => AuthoritySystem.GetAuthorityType(k) == null || !typeof(Authority).IsAssignableFrom(AuthoritySystem.GetAuthorityType(k))).ToList();
        Assert.IsEmpty(missing, "능력 클래스 없음: " + string.Join(", ", missing.Select(k => k + "Authority")));
    }

    [Test]
    public void MerchantLedger_HalfPriceOnlyFirstWave()
    {
        Assert.AreEqual(5, MerchantLedgerAuthority.ApplyDiscount(10, 0));
        Assert.AreEqual(6, MerchantLedgerAuthority.ApplyDiscount(11, 0), "올림");
        Assert.AreEqual(10, MerchantLedgerAuthority.ApplyDiscount(10, 1));
    }

    [Test]
    public void GamblerCoin_Roll()
    {
        Assert.AreEqual((1, 10), GamblerCoinAuthority.Roll(0.1f, 10, 100), "25% 미만: 레벨 1, 가격 그대로");
        Assert.AreEqual((0, 20), GamblerCoinAuthority.Roll(0.3f, 10, 100), "25~50%: 2배");
        Assert.AreEqual((0, 15), GamblerCoinAuthority.Roll(0.3f, 10, 15), "2배를 못 내면 가진 만큼");
        Assert.AreEqual((0, 10), GamblerCoinAuthority.Roll(0.7f, 10, 100), "나머지: 보통");
    }

    [Test]
    public void ArtisanHammer_MinChanceAndRefund()
    {
        Assert.AreEqual(30, ArtisanHammerAuthority.ApplyMinChance(10));
        Assert.AreEqual(80, ArtisanHammerAuthority.ApplyMinChance(80));
        Assert.AreEqual(3, ArtisanHammerAuthority.GetRefund(7));
    }

    [Test]
    public void PureCrown_AllSameAttackType_ScalesOnlyWeakness()
    {
        Assert.IsTrue(PureCrownAuthority.IsPure(new[] { AttackType.Magic, AttackType.Magic, AttackType.Magic }));
        Assert.IsFalse(PureCrownAuthority.IsPure(new[] { AttackType.Magic, AttackType.Physic }));
        Assert.IsFalse(PureCrownAuthority.IsPure(new[] { AttackType.AllRounder, AttackType.AllRounder }), "만능은 제외");
        Assert.AreEqual(2.5f * 1.5f, PureCrownAuthority.Apply(2.5f), 0.0001f);
        Assert.AreEqual(0.2f, PureCrownAuthority.Apply(0.2f), 0.0001f, "내성은 그대로");
        Assert.AreEqual(1f, PureCrownAuthority.Apply(1f), 0.0001f);
    }

    [Test]
    public void KingSeal_AllDifferentForms()
    {
        var five = new[] { SlimeTag.Shooter | SlimeTag.Status, SlimeTag.Zone, SlimeTag.Orbit, SlimeTag.Support, SlimeTag.Special };
        Assert.IsTrue(KingSealAuthority.AllDifferentForms(five));
        Assert.IsFalse(KingSealAuthority.AllDifferentForms(new[] { SlimeTag.Shooter, SlimeTag.Shooter | SlimeTag.Heavy, SlimeTag.Orbit, SlimeTag.Support, SlimeTag.Special }), "형태 중복");
        Assert.IsFalse(KingSealAuthority.AllDifferentForms(five.Take(4)), "5종 미만");
    }

    [Test]
    public void MixedCrest_Mixed()
    {
        Assert.IsTrue(MixedCrestAuthority.IsMixed(new[] { AttackType.Physic, AttackType.Magic, AttackType.Physic }));
        Assert.IsFalse(MixedCrestAuthority.IsMixed(new[] { AttackType.Physic, AttackType.AllRounder }));
    }

    [Test]
    public void Hourglass_TriggerOnceBelow30Percent()
    {
        Assert.IsTrue(HourglassAuthority.ShouldTrigger(false, 29, 100));
        Assert.IsFalse(HourglassAuthority.ShouldTrigger(false, 31, 100));
        Assert.IsFalse(HourglassAuthority.ShouldTrigger(true, 10, 100), "한 판에 1번");
        Assert.IsFalse(HourglassAuthority.ShouldTrigger(false, 0, 100), "사망은 제외");
    }

    [Test]
    public void BloodlineFlask_KeepHalf()
    {
        Assert.AreEqual("Ice", BloodlineFlaskAuthority.PickResultKey("Ice", "Flame", 0.2f));
        Assert.AreEqual("Flame", BloodlineFlaskAuthority.PickResultKey("Ice", "Flame", 0.7f));
    }

    [Test]
    public void SplitShard_TwoDirsAtAngle()
    {
        Vector2[] dirs = SplitShardAuthority.GetSplitDirs(Vector2.right);
        Assert.AreEqual(2, dirs.Length);
        Assert.AreEqual(SplitShardAuthority.SPLIT_ANGLE, Vector2.Angle(Vector2.right, dirs[0]), 0.01f);
        Assert.AreEqual(SplitShardAuthority.SPLIT_ANGLE, Vector2.Angle(Vector2.right, dirs[1]), 0.01f);
        Assert.AreNotEqual(dirs[0], dirs[1]);
    }

    [Test]
    public void RelayCrystal_KingArea3x3()
    {
        Assert.IsTrue(RelayCrystalAuthority.IsInKingArea(Vector2.zero, new Vector2(1.4f, -1.4f), 1f));
        Assert.IsFalse(RelayCrystalAuthority.IsInKingArea(Vector2.zero, new Vector2(1.6f, 0f), 1f));
    }

    [Test]
    public void PlagueChalice_CountsDebuffTypesAndCopies()
    {
        var effects = new List<StatusEffect> { new IceEffect(1f), new IceEffect(2f), new FlameEffect(3f, 7f), new ShieldEffect(null) };
        List<StatusEffect> debuffs = PlagueChaliceAuthority.GetDebuffs(effects);
        Assert.AreEqual(2, debuffs.Count, "빙결 1종 + 화염 1종 (방어막 제외)");
        var flame = PlagueChaliceAuthority.Copy(new FlameEffect(3f, 7f)) as FlameEffect;
        Assert.IsNotNull(flame);
        Assert.AreEqual(7f, flame.DamagePerSecond);
        Assert.IsInstanceOf<IceEffect>(PlagueChaliceAuthority.Copy(new IceEffect(1f)), "빙결은 슬로우가 아니라 빙결로 복사");
    }
}

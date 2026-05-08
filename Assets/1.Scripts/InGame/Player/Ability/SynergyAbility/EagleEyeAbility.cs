using UnityEngine;

// 독수리눈 - 거리가 멀수록 데미지 증가
public class EagleEyeAbility : SynergyAbility, IBulletItem
{
    static readonly float maxBonusRatio = 1f;
    float MAX_RANGE = 10f;

    public override void OnEquip(Player player)
    {
        UpdateEnhancement();
    }
    public override void OnUnequip(Player player) { }

    public override void UpdateEnhancement()
    {
        Camera mainCamera = Camera.main;
        MAX_RANGE = mainCamera.orthographicSize * mainCamera.aspect;
    }
    public void OnBulletFired(PlayerBullet bullet)
    {
        bullet.AddBulletForce(new EagleEyeForce(maxBonusRatio, MAX_RANGE));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"거리가 멀수록 최대 공격력 {100}% 추가 데미지";
    }
}

public class EagleEyeForce : IBulletForce
{
    readonly float maxBonusRatio;
    readonly float maxRange;

    public EagleEyeForce(float maxBonusRatio, float maxRange)
    {
        this.maxBonusRatio = maxBonusRatio;
        this.maxRange = maxRange;
    }

    public float GetMultiDamage(BulletBase bullet, IHittable hit, RaycastHit2D hit2D)
    {
        float dist = Vector2.Distance(Player.Instance.transform.position, hit2D.point);
        float ratio = Mathf.Clamp01(dist / maxRange);
        return Player.Instance.statMgr.AttackPower * maxBonusRatio * ratio;
    }
}

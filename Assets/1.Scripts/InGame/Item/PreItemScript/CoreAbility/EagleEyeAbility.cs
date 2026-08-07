using UnityEngine;

// 독수리눈 - 거리가 멀수록 데미지 증가
public class EagleEyeAbility : SynergyAbility, IBullet
{
    static readonly float maxBonusRatio = 1f;
    float MAX_RANGE = 10f;

    public override void OnEquip(Character player)
    {
        UpdateEnhancement();
    }
    public override void OnUnequip(Character player) { }

    public override void UpdateEnhancement()
    {
        Camera mainCamera = Camera.main;
        MAX_RANGE = mainCamera.orthographicSize * mainCamera.aspect;
    }
    public void OnBulletFired(CharacterBulletObject bullet)
    {
        bullet.AddBulletForce(new EagleEyeForce(maxBonusRatio, MAX_RANGE));
    }

    public override string GetDescription(bool detail = false)
    {
        return "거리가 멀수록 최대 공격력 100% 추가 데미지";
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

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        float dist = Vector2.Distance(Character.Instance.transform.position, hit2D.point);
        float ratio = Mathf.Clamp01(dist / maxRange);
        return Character.Instance.statMgr.AttackPower * maxBonusRatio * ratio;
    }
}

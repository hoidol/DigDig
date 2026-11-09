using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
// [각성] (진화) 회전에 닿은 적 EVOLVE_STUN_CHANCE 확률로 EVOLVE_STUN_DURATION초 스턴
public class OrbitSlime : SlimeGrowth1, IRotationDirectionUnit
{
    public const float EVOLVE_STUN_CHANCE = 0.03f; // 0~1
    public const float EVOLVE_STUN_DURATION = 0.1f;

    // 회전 방향 설정 (설정 버튼). 타겟을 노리지 않으므로 타겟 방식 버튼은 숨김
    public RotationDirection RotationDirection => orbitMachine.rotationDirection;
    public void SetRotationDirection(RotationDirection direction) => orbitMachine.rotationDirection = direction;
    public override bool UseFindTarget => false;

    public OrbitMachine orbitMachine;
    public OrbitSlimeData orbitSlimeData;
    public int orbitCount;
    public float rotationSpeed;
    public float radius;

    public override void Spawn(Vector2 pos, int lv)
    {
        orbitSlimeData = SlimeManager.Instance.GetSlimeData(key) as OrbitSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        orbitCount = orbitSlimeData.orbitCount.GetByMergeLevel(mergeLevel);
        radius = orbitSlimeData.radius;

        base.InitSlime();

        //회전 머신 설정
        orbitMachine.damage = AttackPower;
        orbitMachine.attackType = AttackType;
        orbitMachine.owner = this;
        orbitMachine.rotationDirection = OrbitMachine.DEFAULT_ROTATION_DIRECTION;

        Debug.Log($"OrbitSlime InitSlime() {key} MergeLevel {mergeLevel} AttackPower {AttackPower}");
        orbitMachine.radius = radius;
        for (int i = 0; i < orbitCount; i++)
            orbitMachine.AddOrbit();
    }

    protected override void OnEvolve()
    {
        foreach (OrbitOrb orb in orbitMachine.orbitOrbs)
            orb.AddBulletBehaviour(new StunOnHitBehavior(EVOLVE_STUN_DURATION, EVOLVE_STUN_CHANCE));
    }

    public override void StartDrag()
    {
        base.StartDrag();
        orbitMachine.gameObject.SetActive(false);
    }
    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        orbitMachine.gameObject.SetActive(true);
    }

    public override void UpdateSlime()
    {
        base.UpdateSlime();
        orbitMachine.damage = AttackPower;
        // 회전 속도 = 공속 (버프 반영 후 값). 공격 타이머와 같은 기준: AttackSpeed가 ATTACK_SPEED_PER_SECOND일 때 orbitRotationSpeed
        rotationSpeed = orbitSlimeData.orbitRotationSpeed * AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;
        orbitMachine.rotationSpeed = rotationSpeed;
        Debug.Log($"OrbitSlime UpdateSlime() {key} MergeLevel {mergeLevel} AttackPower {AttackPower}");
        orbitMachine.UpdateOrbitMachine();
    }
    public override void Fire(Vector2 dir)
    {

    }

    public override AllyBulletObject GetBullet()
    {
        return null;
    }
}
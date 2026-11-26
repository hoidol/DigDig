using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
// (2026-11-26) 진화 = MinionEvolutionSetting + EvolutionMinion/Behaviours (예전 [각성] 효과 삭제)
public class OrbitMinion : Minion, IRotationDirectionUnit
{

    // 회전 방향 설정 (설정 버튼). 타겟을 노리지 않으므로 타겟 방식 버튼은 숨김
    public RotationDirection RotationDirection => orbitMachine.rotationDirection;
    public void SetRotationDirection(RotationDirection direction) => orbitMachine.rotationDirection = direction;
    public override bool UseFindTarget => false;

    public OrbitMachine orbitMachine;
    public OrbitMinionData orbitMinionData;
    public int orbitCount;
    public float rotationSpeed;
    public float radius;

    public override void Spawn(Vector2 pos, int lv)
    {
        orbitMinionData = MinionManager.Instance.GetMinionData(key) as OrbitMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        orbitCount = orbitMinionData.orbitCount.GetByMergeLevel(mergeLevel);
        radius = orbitMinionData.radius;

        base.InitMinion();

        //회전 머신 설정
        orbitMachine.damage = AttackPower;
        orbitMachine.attackType = AttackType;
        orbitMachine.owner = this;
        orbitMachine.rotationDirection = OrbitMachine.DEFAULT_ROTATION_DIRECTION;

        Debug.Log($"OrbitMinion InitMinion() {key} MergeLevel {mergeLevel} AttackPower {AttackPower}");
        orbitMachine.radius = radius;
        for (int i = 0; i < orbitCount; i++)
            orbitMachine.AddOrbit();
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

    public override void UpdateMinion()
    {
        base.UpdateMinion();
        orbitMachine.damage = AttackPower;
        // 회전 속도 = 공속 (버프 반영 후 값). 공격 타이머와 같은 기준: AttackSpeed가 ATTACK_SPEED_PER_SECOND일 때 orbitRotationSpeed
        rotationSpeed = orbitMinionData.orbitRotationSpeed * AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;
        orbitMachine.rotationSpeed = rotationSpeed;
        Debug.Log($"OrbitMinion UpdateMinion() {key} MergeLevel {mergeLevel} AttackPower {AttackPower}");
        orbitMachine.UpdateOrbitMachine();
    }
    // 발사 없음. 타이머만 리셋 (안 하면 attackTimer가 1 이상으로 남아 매 프레임 타겟을 찾음)
    public override void Fire(Vector2 dir)
    {
        attackTimer = 0;
    }

    public override AllyBulletObject GetBullet()
    {
        return null;
    }
}
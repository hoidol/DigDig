using UnityEngine;

// [B] 철벽: 방패(ShieldOrbitOrb)가 슬라임 주위를 돈다. 방패 수 = mergeLevel + 1 (+강화)
// 방패는 닿은 적의 이동을 멈춤 (방패당 최대 blockCountPerShield기). 막는 동안 그 방패만 정지 (SeparateOrbitMachine)
// 프리팹: orbitMachine의 orbitOrbPrefab에 ShieldOrbitOrb 프리팹 연결
public class ShieldSlime : SlimeGrowth1, IRotationDirectionUnit
{
    // 회전 방향 설정 (설정 버튼). 방패만 돌리므로 타겟 방식 버튼은 숨김
    public RotationDirection RotationDirection => orbitMachine.rotationDirection;
    public void SetRotationDirection(RotationDirection direction) => orbitMachine.rotationDirection = direction;
    public override bool UseFindTarget => false;

    public SeparateOrbitMachine orbitMachine;
    public float orbitRotationSpeed = 120; // 프리팹 기본값 (강화는 rotationSpeed에 반영)

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public float rotationSpeed;
    public int shieldCount;
    public int blockCountPerShield; // 방패당 동시에 막는 적 수
    public float thornMaxHpRatio;   // [각성] 가시방패: 막힌 적에게 초당 최대 체력 × 비율 피해 (0이면 없음)

    public const int BASE_BLOCK_COUNT_PER_SHIELD = 2;

    public override void InitSlime()
    {
        rotationSpeed = orbitRotationSpeed;
        shieldCount = mergeLevel + 1;
        blockCountPerShield = BASE_BLOCK_COUNT_PER_SHIELD;
        thornMaxHpRatio = 0;
        base.InitSlime(); // 강화 능력이 값을 바꿀 수 있으므로 기본값 설정 후 호출
    }

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);

        orbitMachine.damage = AttackPower;
        orbitMachine.attackType = AttackType;
        orbitMachine.owner = this;
        orbitMachine.rotationDirection = OrbitMachine.DEFAULT_ROTATION_DIRECTION;
        orbitMachine.radius = 2.3f;
        orbitMachine.rotationSpeed = rotationSpeed;
        for (int i = 0; i < shieldCount; i++)
        {
            OrbitOrb orb = orbitMachine.AddOrbit();
            if (orb is ShieldOrbitOrb shield)
            {
                shield.maxBlockCount = blockCountPerShield;
                shield.thornMaxHpRatio = thornMaxHpRatio;
            }
        }
    }

    public override AllyBulletObject GetBullet()
    {
        return null;
    }
}

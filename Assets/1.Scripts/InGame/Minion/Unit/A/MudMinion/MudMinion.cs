using System.Collections.Generic;
using UnityEngine;

// [A] 진흙: 공격 주기(공속)마다 진흙 구슬(MudOrb)을 하나씩 만들어 주변을 회전 (최대 maxOrbCount개, 머지 레벨로 증가)
// 구슬이 적에 닿으면 스플래시 피해 + 진흙 1중첩 후 사라짐 → 중첩당 이동속도 15% 감소 (최대 80%, 엘리트·보스 60%, MudEffect)
// 최대 개수면 생성하지 않고 공격 타이머를 유지 → 구슬이 터지면 바로 다시 생성
public class MudMinion : MinionGrowth1, IRotationDirectionUnit
{
    // 회전 방향 설정 (설정 버튼). 사거리 안 적 유무만 보고 타겟 방향은 안 쓰므로 타겟 방식 버튼은 숨김
    public RotationDirection RotationDirection => orbitMachine.rotationDirection;
    public void SetRotationDirection(RotationDirection direction) => orbitMachine.rotationDirection = direction;
    public override bool UseFindTarget => false;

    public OrbitMachine orbitMachine;
    MudMinionData mudMinionData;

    public int maxOrbCount;
    public const int EVOLVE_ADD_ORB_COUNT = 1; // [각성] (진화) 구슬 최대 수 +1
    public float radius;
    public float splashRadius;
    public int maxHitCount;
    public float mudDuration;

    bool isDragging;

    public override void Spawn(Vector2 pos, int lv)
    {
        mudMinionData = MinionManager.Instance.GetMinionData(key) as MudMinionData;
        base.Spawn(pos, lv);
    }

    protected override void OnEvolve()
    {
        maxOrbCount += EVOLVE_ADD_ORB_COUNT;
    }

    public override void InitMinion()
    {
        maxOrbCount = mudMinionData.maxOrbCounts.GetByMergeLevel(mergeLevel);
        radius = mudMinionData.radius;
        splashRadius = mudMinionData.splashRadius;
        maxHitCount = mudMinionData.maxHitCount;
        mudDuration = mudMinionData.mudDuration;
        isDragging = false;
        base.InitMinion();

        // 풀링 재사용 시 이전 구슬 정리 후 처음부터 다시 생성
        foreach (OrbitOrb orb in new List<OrbitOrb>(orbitMachine.orbitOrbs))
            orbitMachine.RemoveOrbitBullet(orb);
        orbitMachine.owner = this;
        orbitMachine.rotationDirection = OrbitMachine.DEFAULT_ROTATION_DIRECTION;
        orbitMachine.attackType = AttackType;
        orbitMachine.radius = radius;
        orbitMachine.rotationSpeed = mudMinionData.orbitRotationSpeed; // 공속은 구슬 생성 주기라 회전 속도는 고정
        orbitMachine.gameObject.SetActive(true);
    }

    public override void UpdateMinion()
    {
        base.UpdateMinion();
        if (orbitMachine == null)
            return;
        orbitMachine.damage = AttackPower;
        orbitMachine.UpdateOrbitMachine();
    }

    public override void StartDrag()
    {
        base.StartDrag();
        isDragging = true;
        orbitMachine.gameObject.SetActive(false);
    }

    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        isDragging = false;
        orbitMachine.gameObject.SetActive(true);
    }

    // 타겟 방향은 쓰지 않음. 공격 주기마다 구슬 1개 생성
    public override void Fire(Vector2 dir)
    {
        if (isDragging || orbitMachine.orbitOrbs.Count >= maxOrbCount)
            return;

        orbitMachine.AddOrbit();
        attackTimer = 0;
    }

    // 탄을 쏘지 않음 (Fire 재정의)
    public override AllyBulletObject GetBullet() => null;
}

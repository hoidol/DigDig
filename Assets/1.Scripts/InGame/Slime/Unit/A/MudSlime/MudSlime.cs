using System.Collections.Generic;
using UnityEngine;

// [A] 진흙: 공격 주기(공속)마다 진흙 구슬(MudOrb)을 하나씩 만들어 주변을 회전 (최대 maxOrbCount개, 머지 레벨로 증가)
// 구슬이 적에 닿으면 스플래시 피해 + 진흙 1중첩 후 사라짐 → 중첩당 이동속도 15% 감소 (최대 80%, 엘리트·보스 60%, MudEffect)
// 최대 개수면 생성하지 않고 공격 타이머를 유지 → 구슬이 터지면 바로 다시 생성
public class MudSlime : SlimeGrowth1
{
    public OrbitMachine orbitMachine;
    MudSlimeData mudSlimeData;

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public int maxOrbCount;
    public float radius;
    public float splashRadius;
    public int maxHitCount;
    public float mudDuration;

    bool isDragging;

    public override void Spawn(Vector2 pos, int lv)
    {
        mudSlimeData = SlimeManager.Instance.GetSlimeData(key) as MudSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        maxOrbCount = mudSlimeData.maxOrbCounts.GetByMergeLevel(mergeLevel);
        radius = mudSlimeData.radius;
        splashRadius = mudSlimeData.splashRadius;
        maxHitCount = mudSlimeData.maxHitCount;
        mudDuration = mudSlimeData.mudDuration;
        isDragging = false;
        base.InitSlime(); // 강화 능력이 값을 바꿀 수 있으므로 기본값 설정 후 호출

        // 풀링 재사용 시 이전 구슬 정리 후 처음부터 다시 생성
        foreach (OrbitOrb orb in new List<OrbitOrb>(orbitMachine.orbitOrbs))
            orbitMachine.RemoveOrbitBullet(orb);
        orbitMachine.owner = this;
        orbitMachine.attackType = AttackType;
        orbitMachine.radius = radius;
        orbitMachine.rotationSpeed = mudSlimeData.orbitRotationSpeed; // 공속은 구슬 생성 주기라 회전 속도는 고정
        orbitMachine.gameObject.SetActive(true);
    }

    public override void UpdateSlime()
    {
        base.UpdateSlime();
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

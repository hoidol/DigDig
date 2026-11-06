using UnityEngine;
using System.Collections.Generic;

// 철벽 슬라임 방패: 닿은 적의 이동을 멈춘다 (최대 maxBlockCount기, 초과한 적은 그냥 지나감)
// 적 탄에 닿으면 탄 제거. 피해는 OrbitOrb와 같이 닿을 때 줌
// 막는 동안 방패는 그 자리에 멈추고(SeparateOrbitMachine.SetSpeedMul 0), 막던 적이 모두 죽거나 빠지면 다시 회전
public class ShieldOrbitOrb : OrbitOrb
{
    public int maxBlockCount = 3; // ShieldSlime이 AddOrbit 후 설정 (blockCountPerShield)

    // [각성] 가시방패: 막혀 있는 적에게 THORN_INTERVAL마다 최대 체력 × thornMaxHpRatio 피해 (0이면 없음)
    // 퍼센트 피해라 방어 타입 배율은 무시, 치명타 없음. 낙인 배율 등 적 쪽 배율은 그대로 적용
    public float thornMaxHpRatio;
    const float THORN_INTERVAL = 1f;
    float thornTimer;
    readonly AllyUnitDamageData thornDamageData = new AllyUnitDamageData();

    struct Blocked
    {
        public Enemy enemy;
        public int spawnId; // 풀링 재사용 후의 같은 객체는 해제하지 않음
    }
    readonly List<Blocked> blocked = new List<Blocked>();

    public int BlockedCount => blocked.Count;

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyBulletObject enemyBullet))
        {
            Destroy(enemyBullet.gameObject);
            return;
        }
        base.OnTriggerEnter2D(other);

        if (((1 << other.gameObject.layer) & layerMask.value) == 0) return;
        if (!other.TryGetComponent(out Enemy enemy)) return;
        TryBlock(enemy);
    }

    public virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Enemy enemy))
            Unblock(enemy);
    }

    void TryBlock(Enemy enemy)
    {
        if (!enemy.CanBeBlocked) // 엘리트/보스
            return;
        if (blocked.Count >= maxBlockCount)
            return;
        if (blocked.Exists(b => b.enemy == enemy))
            return;

        blocked.Add(new Blocked { enemy = enemy, spawnId = enemy.SpawnId });
        enemy.AddBlocker();
        UpdateRotation();
    }

    void Unblock(Enemy enemy)
    {
        int index = blocked.FindIndex(b => b.enemy == enemy);
        if (index < 0)
            return;

        Blocked b = blocked[index];
        if (b.enemy != null && b.enemy.SpawnId == b.spawnId)
            b.enemy.RemoveBlocker();
        blocked.RemoveAt(index);
        UpdateRotation();
    }

    public override void Update()
    {
        base.Update();

        // 죽어서 풀로 돌아간(꺼진/재사용된) 적 정리 (꺼질 때 Exit가 안 올 수 있음)
        int removed = blocked.RemoveAll(b => b.enemy == null || !b.enemy.gameObject.activeInHierarchy || b.enemy.SpawnId != b.spawnId);
        if (removed > 0)
            UpdateRotation();

        UpdateThorn();
    }

    void UpdateThorn()
    {
        if (thornMaxHpRatio <= 0 || blocked.Count == 0)
        {
            thornTimer = 0; // 새로 막으면 1초 뒤 첫 피해
            return;
        }

        thornTimer += Time.deltaTime;
        if (thornTimer < THORN_INTERVAL)
            return;
        thornTimer -= THORN_INTERVAL;

        IAllyUnit owner = orbitMachine != null ? orbitMachine.owner : null;
        // 피해 중 적이 죽어 목록이 바뀔 수 있으므로 복사본으로 순회
        foreach (Blocked b in blocked.ToArray())
        {
            if (b.enemy == null || !b.enemy.gameObject.activeInHierarchy || b.enemy.SpawnId != b.spawnId)
                continue;

            thornDamageData.Init(owner);
            thornDamageData.critChance = 0;
            thornDamageData.ignoreDefence = true;
            thornDamageData.attackType = orbitMachine != null ? orbitMachine.attackType : AttackType.AllRounder;
            thornDamageData.damage = b.enemy.MaxHp * thornMaxHpRatio;
            owner?.AccumulateDamage(thornDamageData.damage);
            b.enemy.TakeDamage(thornDamageData);
        }
    }

    void OnDisable()
    {
        foreach (Blocked b in blocked)
        {
            if (b.enemy != null && b.enemy.SpawnId == b.spawnId)
                b.enemy.RemoveBlocker();
        }
        blocked.Clear();
    }

    // 막는 중이면 이 방패만 정지 (SeparateOrbitMachine일 때만. 일반 OrbitMachine은 전체가 같이 돌아서 개별 정지 불가)
    void UpdateRotation()
    {
        if (GetComponentInParent<SeparateOrbitMachine>() is SeparateOrbitMachine machine)
            machine.SetSpeedMul(this, blocked.Count > 0 ? 0f : 1f);
    }
}

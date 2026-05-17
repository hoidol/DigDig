using UnityEngine;

public class EnemyDeadEvent
{
    public Enemy enemy;
    public Vector2 position;
    public Transform cause;
    public EnemyDeadEvent(Enemy enemy, Transform c)
    {
        this.enemy = enemy;
        position = enemy.transform.position;
        cause = c;
    }
}

public class PlayerDamagedEvent
{
    public float Damage;
    public PlayerDamagedEvent(float damage) { Damage = damage; }
}



// public class ApproachingOrdealStartEvent
// {
//     public OrdealProgressData ordealProgressData;
//     public ApproachingOrdealStartEvent(OrdealProgressData ordealProgressData)
//     {
//         this.ordealProgressData = ordealProgressData;
//     }
// }

public class OrdealStartEvent
{
    public OrdealData ordealData;

    public OrdealProgressData ordealProgressData;
    public OrdealStartEvent(OrdealData ordealData, OrdealProgressData ordealProgressData)
    {
        this.ordealData = ordealData;
        this.ordealProgressData = ordealProgressData;
    }
}

public class OrdealEndEvent
{
    public OrdealData ordealData;
    public int ordealClearCount;
    public OrdealEndEvent(OrdealData ordealData, int ordealClearCount)
    {
        this.ordealData = ordealData;
        this.ordealClearCount = ordealClearCount;
    }
}
public class ReloadStartEvent
{
    public float reloadTime;
    public float reloadSpeed;
    public ReloadStartEvent(float reloadTime, float reloadSpeed)
    {
        this.reloadTime = reloadTime;
        this.reloadSpeed = reloadSpeed;
    }
}

public class ReloadEndEvent { }

public class BossSpawnEvent
{
    public Boss boss;
    public BossSpawnEvent(Boss boss) { this.boss = boss; }
}

public class BossPhaseChangedEvent
{
    public Boss boss;
    public int phase;
    public BossPhaseChangedEvent(Boss boss, int phase) { this.boss = boss; this.phase = phase; }
}

public class BossDeadEvent
{
    public Boss boss;
    public Vector2 position;
    public BossDeadEvent(Boss boss) { this.boss = boss; position = boss.transform.position; }
}

public class BulletChargedEvent
{
    public int currentBulletCount;
    public int maxBulletCount;
    public BulletChargedEvent(int cur, int max)
    {
        currentBulletCount = cur;
        maxBulletCount = max;
    }
}

public class PlayerHpChangedEvent
{
    public float curHp;
    public float maxHp;
    public PlayerHpChangedEvent(float curHp, float maxHp)
    {
        this.curHp = curHp;
        this.maxHp = maxHp;
    }
}

public class TryAddItemEvent
{
    public ItemData itemData;
    public TryAddItemEvent(ItemData itemData)
    {
        this.itemData = itemData;
    }
}
public class AddedItemEvent
{
    public ItemData itemData;
    public AddedItemEvent(ItemData itemData)
    {
        this.itemData = itemData;
    }
}

// 벙커 소환 가능 상태 알림
public class IronNestReadyEvent
{
    public int remainingCount; // 현재 누적 파괴 수 / 임계값 표시용
    public IronNestReadyEvent(int remaining) { remainingCount = remaining; }
}

// 소환 버튼 클릭 시 발행
public class IronNestSpawnRequestEvent { }

// public class AngerChangedEvent
// {
//     public bool isAngry;
//     public AngerChangedEvent(bool isAngry) { this.isAngry = isAngry; }
// }

// public class OrdealTimerEvent
// {
//     public float remainTime;
//     public float totalTime;
//     public OrdealTimerEvent(float remainTime, float totalTime)
//     {
//         this.remainTime = remainTime;
//         this.totalTime = totalTime;
//     }
// }

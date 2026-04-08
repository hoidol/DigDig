using UnityEngine;

public class EnemyDeadEvent
{
    public Enemy enemy;
    public Vector2 position;
    public EnemyDeadEvent(Enemy enemy, Transform cause)
    {
        this.enemy = enemy;
        position = enemy.transform.position;
    }
}

public class PlayerDamagedEvent
{
    public float Damage;
    public PlayerDamagedEvent(float damage) { Damage = damage; }
}

public class WaveStartEvent
{
    public int undergroundIdx;
    public WaveData waveData; //웨이브 데이터
    public WaveStartEvent(WaveData waveData)
    {
        this.waveData = waveData;
    }
}
public class WaveEndEvent
{
    public WaveData waveData;
    public WaveEndEvent(WaveData waveData)
    {
        this.waveData = waveData;
    }
}


public class UndergroundStartEvent
{
    public UndergroundData undergroundData;
    public UndergroundStartEvent(UndergroundData undergroundData)
    {
        this.undergroundData = undergroundData;
    }
}

public class UndergroundEndEvent
{
    public UndergroundData undergroundData;
    public UndergroundEndEvent(UndergroundData undergroundData)
    {
        this.undergroundData = undergroundData;
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

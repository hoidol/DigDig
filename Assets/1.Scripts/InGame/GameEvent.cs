using UnityEngine;

public class EnemyDeadEvent
{
    public Enemy enemy;
    public Vector2 position;
    public EnemyDeadEvent(Enemy enemy)
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
public class BulletFiredEvent
{
    public PlayerBullet bullet;
    public BulletFiredEvent(PlayerBullet bullet) { this.bullet = bullet; }
}
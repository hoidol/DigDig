using UnityEngine;

//4킬 처치 시 360° 5발 발사 (체력 -2)
public class SprayItem : Item
{
    int[] killsNeeded = { 4, 3, 2 };
    int[] bulletCounts = { 5, 7, 9 };
    int killCount = 0;
    public float coolTime = 2;
    public float coolTimer = 0;

    void OnEnable()
    {
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStoneEvent);
        GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    void OnDestroyedStoneEvent(DestroyedStoneEvent e)
    {
        OnKill();
    }

    void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        OnKill();
    }

    void OnKill()
    {
        if (coolTimer > 0)
            return;

        killCount++;
        if (killCount < killsNeeded[count - 1])
            return;

        killCount = 0;
        Shoot();
        coolTimer = coolTime;
    }

    void Update()
    {
        if (coolTimer > 0)
            coolTimer -= Time.deltaTime;
    }

    void Shoot()
    {
        int bulletCount = bulletCounts[count - 1];
        float angleStep = 360f / bulletCount;
        Vector2 baseDir = Random.insideUnitCircle.normalized;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angleStep * i) * baseDir;
            Player.Instance.Shoot(new NormalBullet(), dir);
        }

        Player.Instance.AddHp(-itemData.consumeHp);
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"{killsNeeded[lv - 1]}킬 처치 시 360도 {bulletCounts[lv - 1]}발 발사 (체력 -{itemData.consumeHp})";
    }

}

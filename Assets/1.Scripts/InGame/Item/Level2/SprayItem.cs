using UnityEngine;

//4킬 처치 시 360° 5발 발사 (체력 -2)
public class SprayItem : Item
{
    int killsNeeded = 4;
    int bulletCount = 5;
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
        if (killCount < killsNeeded)
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
        int bCount = bulletCount * count;
        float angleStep = 360f / bCount;
        Vector2 baseDir = Random.insideUnitCircle.normalized;
        for (int i = 0; i < bCount; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angleStep * i) * baseDir;
            Character.Instance.Shoot(new NormalBulletSpec(), dir);
        }

        Character.Instance.AddHp(-itemData.consumeHp);
    }

    public override string GetDescription()
    {
        return $"{killsNeeded}킬 처치 시 360도 {bulletCount}발 발사 (체력 -{itemData.consumeHp})";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),killsNeeded,bulletCount,itemData.consumeHp);
    }

}

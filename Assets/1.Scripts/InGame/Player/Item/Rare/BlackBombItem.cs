using UnityEngine;

// [검은 폭탄]
// 쿨타임마다 랜덤 방향으로 BlackBomb을 투척하는 TriggerItem.
// 폭탄은 Rigidbody2D로 날아가다가 속도가 0.1 이하로 떨어지면 1초 후 폭발.
// 폭발 데미지는 마력의 50%/65%/80%.
public class BlackBombItem : TriggerItem
{
    public BlackBomb bombPrefab;

    static readonly float[] damageRates = { 0.5f, 0.65f, 0.8f };

    float DamageRate => damageRates[Mathf.Clamp(count - 1, 0, damageRates.Length - 1)];

    public override void OnTrigger()
    {
        base.OnTrigger();
        float damage = Player.Instance.statMgr.MagicPower * DamageRate;
        Vector2 randomDir = Random.insideUnitCircle.normalized;

        var bomb = Instantiate(bombPrefab);
        bomb.transform.position = Player.Instance.transform.position;
        bomb.Shoot(randomDir, damage);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float rate = damageRates[Mathf.Clamp(c - 1, 0, damageRates.Length - 1)];
        return $"랜덤 방향으로 폭탄 투척, 멈추면 폭발 (마력의 {rate * 100:0}% 데미지)";
    }
}

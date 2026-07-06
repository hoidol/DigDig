using UnityEngine;

// 용암 지대 - 범위 내 적에게 주기적으로 데미지
public class LavaZone : MonoBehaviour
{
    float radius;
    LayerMask layer;
    float damageRate;
    float duration;
    const float TICK_INTERVAL = 0.5f;
    float tickTimer;
    float remainingTime;

    public void Init(float radius, LayerMask layer, float damageRate, float duration)
    {
        this.radius = radius;
        this.layer = layer;
        this.damageRate = damageRate;
        this.duration = duration;
        remainingTime = duration;
        tickTimer = TICK_INTERVAL;
    }

    void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0) { Destroy(gameObject); return; }

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0)
        {
            tickTimer = TICK_INTERVAL;
            float damage = Player.Instance.statMgr.AttackPower * damageRate;
            InGameUtil.DamageEnemies(transform.position, radius, damage, layer);
        }
    }
}

using UnityEngine;

// 프리즘: OrbitOrb 상속 - 총알이 닿으면 좌우로 분열 + 데미지 감소
public class Prism : OrbitOrb
{
    public float spreadAngle = 30f;

    float damageRate;

    public void Init(float damageRate)
    {
        this.damageRate = damageRate;
    }

    // OrbitOrb의 OnTriggerEnter2D(IHittable) 대신 PlayerBullet 감지
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerBullet bullet = other.GetComponent<PlayerBullet>();
        if (bullet == null) return;

        Vector2 dir = bullet.transform.right;
        float splitDamage = bullet.damage * damageRate;

        for (int i = -1; i <= 1; i += 2)
        {
            Vector2 splitDir = Quaternion.Euler(0, 0, spreadAngle * i) * dir;
            var splitBullet = AllyBullet.Instantiate();
            splitBullet.transform.position = bullet.transform.position;
            splitBullet.Shoot(splitDir, splitDamage);
        }

        bullet.Release();
    }
}

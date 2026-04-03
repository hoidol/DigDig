using UnityEngine;

public class Shadow : MonoBehaviour
{
    public Transform attackPoint;
    public Vector2 offset = new(-2f, 0);

    void Update()
    {
        // 플레이어 위치 + offset 추적
        transform.position = (Vector2)Player.Instance.transform.position + offset;

        // 플레이어 방향 미러링
        transform.localScale = Player.Instance.bodyRootTr.localScale;
    }

    public void Shoot(Vector2 dir, float damage)
    {
        var bullet = PlayerBullet.Instantiate();
        bullet.ClearBehaviors();
        bullet.transform.position = attackPoint != null
            ? attackPoint.position
            : transform.position;
        bullet.Shoot(dir, damage);
    }
}

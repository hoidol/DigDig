using UnityEngine;
using System.Collections.Generic;

// 회전 오브젝트 베이스: 회전은 OrbitItemBase의 컨테이너가 담당, 여기선 피해 처리만
public class ShieldOrbitOrb : OrbitOrb
{

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyBulletObject enemyBullet))
        {
            Destroy(enemyBullet.gameObject);
            return;
        }
        base.OnTriggerEnter2D(other);
    }
}

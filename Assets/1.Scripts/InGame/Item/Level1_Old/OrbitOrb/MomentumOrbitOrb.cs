using UnityEngine;
using System.Collections.Generic;

// 회전 오브젝트 베이스: 회전은 OrbitItemBase의 컨테이너가 담당, 여기선 피해 처리만
public class MomentumOrbitOrb : OrbitOrb
{
    public float pushPower = 10;
    public override void OnHit(Collider2D other, IHittable hittable)
    {
        base.OnHit(other, hittable);
        //적이면 밖으로 밀리기
        if(other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.Push((enemy.transform.position - Character.Instance.transform.position).normalized, pushPower);
        }
    }
}

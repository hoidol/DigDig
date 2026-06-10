// 약점 공략 - 디버프 걸린 적에게 30% 추가 피해
using UnityEngine;

public class DaggerItem : Item, IBulletForce
{
    const float BONUSValue = 1.2f;

    public override string GetDescription(bool detail = false)
    {
        return $"뒤로 공격 시 {20}% 추가 데미지";
    }

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {      
        if( hit.Transform.TryGetComponent<Enemy>(out Enemy e))
        {
            Vector2 dir = (Player.Instance.transform.position - hit.Transform.position).normalized;
            if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                if(dir.x > 0)
                {
                    dir.x = -1;
                    dir.y = 0;
                }else 
                {
                    
                    dir.x = 1;
                    dir.y = 0;
                }
            }
            else
            {
                if(dir.y > 0)
                {
                    dir.x = 0;
                    dir.y = -1;
                }else 
                {
                    
                    dir.x = 0;
                    dir.y = 1;
                }
            }
            
            float angle = Vector2.Angle(dir,hit2D.normal);
            if(angle < 10)
            {
                return BONUSValue;
            }

        }   
        return 1;
        //throw new System.NotImplementedException();
    }
}

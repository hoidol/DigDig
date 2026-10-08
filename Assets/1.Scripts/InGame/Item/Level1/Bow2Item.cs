using Cysharp.Threading.Tasks;
using UnityEngine;

// 고장난 총: 한1초마다 랜덤 방향으로 총 발사 (데미지 100%)
public class Bow2Item : Item, IFired
{
    public override string GetDescription()
    {
        return $"탄을 1발 추가 발사합니다.";
    }
    async UniTaskVoid Shoot(Vector2 dir )
    {
        for(int i = 0; i < count; i++)
        {
            await UniTask.Delay(BaseGun.COMBO_ATTACK_INTERVAL_MS);
            Character.Instance.weapon.Shoot(null, dir);    
        }       
    }

    public void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir)
    {        
        Shoot(dir).Forget();
    }
}

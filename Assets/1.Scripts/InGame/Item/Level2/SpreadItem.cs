using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//25% > 35% > 45% 확률로 3발 확산탄 발사 - 미니미 없어짐
public class SpreadItem : Item, IFired
{
    float chance =  0.25f;
    // int triggerCounter;
    // bool active;


    public override string GetDescription()
    {
        return $"{chance * 100}%확률로 분산탄을 발사";
        //return string.Format(TranslateManager.GetText($"{key}_Desc"),chance * 100);
    }

    public void OnFired(ref Bullet bullet, ref CharacterBulletObject playerBulletObject, Vector2 dir)
    {
        if (Random.value > chance )
            return;


        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);
        for (int i = 0; i < 2 * count; i++)
        {
            int sign = (i % 2 == 0) ? 1 : -1;
            float offset = (i / 2 + 1) * 40f;
            float rad = (baseAngle + sign * offset) * Mathf.Deg2Rad;
            Character.Instance.weapon.Shoot(bullet, new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
        }


        Character.Instance.AddHp(-itemData.consumeHp);
    }

}
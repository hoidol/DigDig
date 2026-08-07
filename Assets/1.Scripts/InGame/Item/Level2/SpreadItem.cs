using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//25% > 35% > 45% 확률로 3발 확산탄 발사 - 미니미 없어짐
public class SpreadItem : Item, IFired
{
    float[] chances = { 0.4f, 0.6f, 0.8f };
    // int triggerCounter;
    // bool active;


    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"{chances[lv - 1] * 100}%확률로 분산탄을 발사";
    }

    public void OnFired(ref Bullet bullet, ref CharacterBulletObject playerBulletObject, Vector2 dir)
    {
        if (Random.value > chances[count - 1])
            return;


        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);
        for (int i = 0; i < 2; i++)
        {
            int sign = (i % 2 == 0) ? 1 : -1;
            float offset = (i / 2 + 1) * 40f;
            float rad = (baseAngle + sign * offset) * Mathf.Deg2Rad;
            Character.Instance.weapon.Shoot(bullet, new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
        }


        Character.Instance.AddHp(-itemData.consumeHp);
    }

}
using System.Collections.Generic;
using UnityEngine;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class SlimeGrowth1 : EnhanceSlime
{
    
    public override void Spawn(Vector2 pos, int mLv)
    {
        base.Spawn(pos,mLv);
        int count = transform.Find("StarContainer").childCount; //.Find($"Merge{mLv}");
        for(int i = 0; i < count; i++)
        {
            transform.Find("StarContainer").GetChild(i).gameObject.SetActive(false);
        }
        transform.Find("StarContainer").Find($"Merge{mLv}").gameObject.SetActive(true);

    }
}

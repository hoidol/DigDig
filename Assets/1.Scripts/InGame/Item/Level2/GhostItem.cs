using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//거대한 관통탄 화면끝까지 공격
//통과 : 관통 힘
public class GhostItem : Item, IPreFire, IComboFire
{
    public int triggerCount= 8;
    int triggerCounter;


    public Ghost ghostPrefab;

    CancellationTokenSource cts;

    public override void OnEquip()
    {
        base.OnEquip();
        cts = new CancellationTokenSource();
    }

    public override void OnUnequip()
    {
        cts?.Cancel();
        cts?.Dispose();
        active = false;
        triggerCounter = 0;
    }

    public override string GetDescription()
    {
        return $"{triggerCount}번 공격마다 거대창 발사합니다.";
    }
    bool active;
    public void OnPreFire(ref Bullet bullet, Vector2 dir) //trigger 타이밍마다 Pierce 발사 
    {
        triggerCounter++;
        if (triggerCount <= triggerCounter)
        {
            active = true;
            triggerCounter = 0;
        }
    }
    public async UniTask OnComboFire(Vector2 dir)
    {
        if (!active)
            return;

        for(int i = 0; i < count; i++)
        {
            await UniTask.Delay(Character.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
            Ghost ghost = Instantiate(ghostPrefab);
            ghost.transform.position = Character.Instance.transform.position;
            ghost.damage = Character.Instance.statMgr.AttackPower;
            ghost.Shoot(dir);    
        }
        
        triggerCounter = 0;
        active = false;
    }
}


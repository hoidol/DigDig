using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//거대한 관통탄 화면끝까지 공격
//통과 : 관통 힘
public class GhostItem : Item, IPreFire, IComboFire
{
    public int[] triggerCounts = { 8, 8, 8 };
    int triggerCounter;

    //공격력 +5, 탄 효율 30%
    float[] attackPowers = { 7, 12, 17 };
    float[] ammoEfficiencies = { 0.7f, 0.6f, 0.5f };
    Buff atkPowerBuff;
    Buff ammoEfficiencyBuff;


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
        Release();
    }
    public override void UpdateItem()
    {
        base.UpdateItem();
        Release();
        //공격력
        atkPowerBuff = new Buff(StatType.AttackPower, attackPowers[count - 1], StatOpType.Add);
        Character.Instance.AddBuff(atkPowerBuff);

        //탄 효율
        ammoEfficiencyBuff = new Buff(StatType.AmmoEfficiency, ammoEfficiencies[count - 1], StatOpType.Multiply);
        Character.Instance.AddBuff(ammoEfficiencyBuff);
    }

    void Release()
    {
        if (atkPowerBuff != null)
            Character.Instance.RemoveBuff(atkPowerBuff);

        if (ammoEfficiencyBuff != null)
            Character.Instance.RemoveBuff(ammoEfficiencyBuff);
    }

    public override string GetDescription(int lv = 1, bool detail = false)
    {
        return $"공격력 +{attackPowers[lv - 1]} 탄 효율 +{(1 - ammoEfficiencies[lv - 1]) * 100}%";
    }
    bool active;
    public void OnPreFire(ref Bullet bullet, Vector2 dir) //trigger 타이밍마다 Pierce 발사 
    {
        triggerCounter++;
        if (triggerCounts[count - 1] <= triggerCounter)
        {
            active = true;
            triggerCounter = 0;
        }
    }
    public async UniTask OnComboFire(Vector2 dir)
    {
        if (!active)
            return;

        await UniTask.Delay(Character.COMBO_ATTACK_INTERVAL_MS, cancellationToken: cts.Token);
        Ghost ghost = Instantiate(ghostPrefab);
        ghost.transform.position = Character.Instance.transform.position;
        ghost.damage = Character.Instance.statMgr.AttackPower;
        ghost.Shoot(dir);
        triggerCounter = 0;
        active = false;
    }
}


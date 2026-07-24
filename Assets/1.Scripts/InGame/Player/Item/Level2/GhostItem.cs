using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//거대한 관통탄 화면끝까지 공격
//통과 : 관통 힘
public class GhostItem : Item ,IPreFire, IFired
{
    public int[] pierceCounts = {8,11,15};
    public int[] triggerCounts = {4,4,4};
    int triggerCounter;

    //공격력 +5, 탄 효율 30%
    float[] attackPowers ={7,12,17};
    float[] ammoEfficiencies ={0.6f,0.5f,0.4f};
    Buff atkPowerBuff;
    Buff ammoEfficiencyBuff;
    public override void UpdateItem()
    {   
        Release();
        //공격력
        atkPowerBuff = new Buff(StatType.AttackPower,attackPowers[count-1],StatOpType.Add);
        Player.Instance.AddBuff(atkPowerBuff);

        //탄 효율
        ammoEfficiencyBuff = new Buff(StatType.AmmoEfficiency,ammoEfficiencies[count-1],StatOpType.Multiply);
        Player.Instance.AddBuff(ammoEfficiencyBuff);
    }
    void Release()
    {
        if(atkPowerBuff != null)
            Player.Instance.RemoveBuff(atkPowerBuff);

        if(ammoEfficiencyBuff != null)
            Player.Instance.RemoveBuff(ammoEfficiencyBuff);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"공격력 +{attackPowers[lv-1]} 탄 효율 +{(1 - ammoEfficiencies[lv-1]) *100}%";
    }
    bool active;
    public void OnPreFire(ref Bullet bullet, Vector2 dir) //trigger 타이밍마다 Pierce 발사 
    {
        triggerCounter++;
        if (triggerCounts[count-1] <=triggerCounter)
        {
            if(bullet.bulletData.order <= BulletData.GetBulletData("Pierce").order)
            {
                active=true;
                PierceBullet pierceBullet = new PierceBullet();
                pierceBullet.pierceCount = Player.Instance.bounce + pierceCounts[count-1];
                pierceBullet.multiplyAtk = 1;
                bullet = pierceBullet;    
            }
            triggerCounter=0;
        }
    }

    Ghost ghostPrefab;
    //발사된 탄이 Pierce면 Ghost로 변경
    public void OnFired(ref Bullet bullet, ref PlayerBulletObject playerBulletObject, Vector2 dir)
    {
        if(!active)
            return;

        if(bullet.key == "Pierce")
        {
            Ghost ghost = Instantiate(ghostPrefab);
            ghost.Shoot(dir);
            ghost.damage = Player.Instance.statMgr.AttackPower;
            playerBulletObject.Release();
        }
        
    }
}

    
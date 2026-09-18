using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

//플레이어가 공격하면 같이 방향으로 쏨
//충돌 안하게 하자
public abstract class SlimeEnhanceAbility : MonoBehaviour
{
    public int level;
    public Slime slime;
    public bool isActivate;
    public virtual void Activate(Slime slime, bool a)
    {        
        this.slime = slime;
        isActivate = a;
    }
    
    public virtual void Fire()
    {
        
    }
    //public void 
}

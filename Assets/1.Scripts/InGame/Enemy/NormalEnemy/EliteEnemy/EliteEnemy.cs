using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using System;

public class EliteEnemy : NormalEnemy
{
    [SerializeField] float patternCoolTime;
    [SerializeField] EnemyAttackPattern enemyAttackPattern;
    CancellationTokenSource cts;

    public void Spawn(Vector2 pos, int waveIdx, int spawnCount)
    {
        base.Spawn(pos, waveIdx);
        maxHp = GetHp() * (1 + spawnCount);
    }

    public override void Reward()
    {
        base.Reward();
        //ItemBox 드랍 -> 클릭으로 열기
        EventManager.Instance.Spawn(EventType.ItemBox, transform.position);
    }


    protected override void ReachPathEnd()
    {
        rg2d.linearVelocity = Vector2.zero;
        damageData.damage = 10;
        Character.Instance.TakeDamage(damageData);
        Destroy();
    }
}

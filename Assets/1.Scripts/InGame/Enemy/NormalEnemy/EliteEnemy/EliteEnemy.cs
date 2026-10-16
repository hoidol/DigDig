using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using System;

public class EliteEnemy : NormalEnemy, IEnemySpecialAttackPattern
{
    [SerializeField] float patternCoolTime;
    [SerializeField] EnemyAttackPattern enemyAttackPattern;
    CancellationTokenSource cts;
    public override void StartAttack()
    {
        base.StartAttack();

        cts = new CancellationTokenSource();
        ProcessAttack(cts.Token).Forget();
    }
    public void Spawn(Vector2 pos, int spawnCount)
    {
        base.Spawn(pos);
        maxHp = GetHp() * (1 + spawnCount);
    }
    async UniTask ProcessAttack(CancellationToken ct)
    {
        await enemyAttackPattern.Execute(this, () =>
        {

        });

        await UniTask.Delay(TimeSpan.FromSeconds(patternCoolTime));

        EndAttack();
    }

    public override void CancelAttack()
    {
        base.CancelAttack();
        EndAttack();
    }

    public float PlayAnim(string animName)
    {
        if (animator == null)
            return 0f;

        animator.Play(animName, -1, 0);
        float duration = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        return duration;

    }
    public override void Reward()
    {
        base.Reward();
        //ItemBox 드랍 -> 클릭으로 열기
        EventManager.Instance.Spawn(EventType.ItemBox, transform.position);
    }
}

using System;
using DG.Tweening;
using UnityEngine;

// 덫: 설치 후 적이 밟으면 폭발 데미지 + 스턴
public class Trap : Ally
{
    public float damage = 10f;
    public float slowDuration = 1.5f;
    public float explosionRadius = 1.5f;
    public LayerMask enemyLayer;
    bool readyToBomb; // 폭발 준비 완료

    public override void Spawn(Vector2 pos, int lv)
    {
        base.Spawn(pos, lv);
        transform.position = Player.Instance.transform.position;
        damage = Player.Instance.statMgr.MagicPower;
        readyToBomb = false;
        transform.DOMove(pos, 0.4f).OnComplete(() =>
        {
            readyToBomb = true;
        });
        Destroy(gameObject, 10f);
    }
    TrapItem trapItem;
    public Action onReleased;

    public void Init(TrapItem trapItem)
    {
        this.trapItem = trapItem;
    }

    void OnDestroy()
    {
        onReleased?.Invoke();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!readyToBomb) return;
        if (!other.TryGetComponent<IHittable>(out _)) return;
        if (other.tag == "Player") return;

        EffectManager.Instance.Play(EffectType.SmallExplosion, transform.position);
        AOEUtil.DamageEnemies(transform.position, explosionRadius, damage, enemyLayer);

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var col in cols)
        {
            if (col.TryGetComponent<StatusEffectHandler>(out var handler))
                handler.Apply(new SlowEffect(slowDuration));
        }
        if (trapItem != null) trapItem.TripTrggered();
        Destroy(gameObject);
    }
}

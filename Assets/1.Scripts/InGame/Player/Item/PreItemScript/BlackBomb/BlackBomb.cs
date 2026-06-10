using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class BlackBomb : MonoBehaviour
{
    public Rigidbody2D rg2d;
    public float explosionRadius = 1.25f;
    public LayerMask enemyLayer;

    float damage;
    bool exploded;
    CancellationTokenSource cts;
    public float impulsePower;

    public void Shoot(Vector2 dir, float damage)
    {
        this.damage = damage;
        exploded = false;
        cts = new CancellationTokenSource();
        rg2d.AddForce(dir * impulsePower, ForceMode2D.Impulse);
        WaitAndCheck(cts.Token).Forget();
    }

    async UniTaskVoid WaitAndCheck(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
            if (rg2d.linearVelocity.magnitude < 0.1f)
            {
                rg2d.linearVelocity = Vector2.zero;
                rg2d.bodyType = RigidbodyType2D.Static;
                await UniTask.Delay(1000, cancellationToken: token);
                Explode();
                return;
            }
        }
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;
        EffectManager.Instance.Play(EffectType.SmallExplosion, transform.position);
        AOEUtil.DamageEnemies(transform.position, explosionRadius, damage, enemyLayer);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}

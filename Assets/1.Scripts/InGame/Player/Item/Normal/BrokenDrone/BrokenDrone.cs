using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

// 고장난 드론: 총알이 닿으면 바라보는 방향으로 총알 1발 발사 + 3초마다 랜덤 방향으로 shotCount발 동시 발사
public class BrokenDrone : Drone
{
    public Transform rotation;
    public float rotationSpeed = 0f;

    float damageRate;
    int shotCount;

    CancellationTokenSource cts;

    public void Init(float damageRate, int shotCount)
    {
        this.damageRate = damageRate;
        this.shotCount = shotCount;
    }

    public void StartShooting()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        ShootLoop(cts.Token).Forget();
    }

    public void StopShooting()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    void Update()
    {
        rotation.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bullet")) return;
        if (other.GetComponent<PlayerBullet>() == null) return;

        float damage = Player.Instance.statMgr.MagicPower * damageRate;
        Shoot(rotation.up, damage);
    }

    async UniTaskVoid ShootLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(3f), cancellationToken: token);
            if (token.IsCancellationRequested) return;

            float damage = Player.Instance.statMgr.MagicPower * damageRate;
            for (int i = 0; i < shotCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Shoot(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), damage);
            }
        }
    }

    void Shoot(Vector2 dir, float damage)
    {
        var bullet = AllyBullet.Instantiate();
        bullet.transform.position = transform.position;
        bullet.Shoot(dir, damage);
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}

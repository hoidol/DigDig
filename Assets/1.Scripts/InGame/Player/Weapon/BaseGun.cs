using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class BaseGun : MonoBehaviour, IGun
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;

    [SerializeField] public Transform attackPoint;
    [SerializeField] public Transform dirTr;

    protected Player player;
    protected PlayerStatManager statMgr;
    CameraShake cameraShake;
    Camera mainCamera;

    public Transform AttackPoint => attackPoint;
    public Vector2 LastAttackDir { get; private set; }
    public int CurBulletCount { get; private set; }
    public bool IsReloading { get; private set; }

    int pendingMultiShot = 1;
    int pendingSpread;
    int extraShotCount;
    bool processingExtraShots;

    readonly BulletFiredEvent bulletFiredEvent = new();

    public void Init(Player player)
    {
        this.player = player;
        statMgr = player.statMgr;
        cameraShake = player.cameraShake;
        mainCamera = Camera.main;
    }

    public void AddBullet(int count = 1)
    {
        CurBulletCount = Mathf.Min(CurBulletCount + count, statMgr.BulletCount);
    }

    public void ResetOnUndergroundStart()
    {
        IsReloading = false;
        CurBulletCount = statMgr.BulletCount;
        extraShotCount = 0;
        processingExtraShots = false;
        pendingMultiShot = 1;
        pendingSpread = 0;
        GameEventBus.Publish(new ReloadEndEvent());
    }

    public Vector2 GetAttackDirection()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.WorldToScreenPoint(attackPoint.position).z;
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePosition);
        return (worldMousePos - attackPoint.position).normalized;
    }

    public void UpdateWeapon()
    {
        if (IsReloading) return;

        dirTr.up = GetAttackDirection();

#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        if (Input.GetMouseButton(0))
            dirTr.up = GetAttackDirection();
        if (statMgr != null && Time.timeScale > 0)
        {
            // attackTimer는 Attack()에서 리셋되므로 여기선 조건만 체크
        }
        // PC: 마우스 누르고 있으면 자동 발사 (UpdateAttack 로직)
        UpdateAttackInternal();
#else
        if (attackJoystick.Direction.magnitude > 0)
            dirTr.up = attackJoystick.Direction;
        UpdateAttackInternal();
#endif
    }

    float attackTimer;

    void UpdateAttackInternal()
    {
        attackTimer += Time.deltaTime;

#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        if (attackTimer >= statMgr.AttackSpeed)
            Attack(GetAttackDirection(), true);
#else
        if (attackJoystick.Direction.magnitude > 0 && attackTimer >= statMgr.AttackSpeed)
            Attack(attackJoystick.Direction.normalized, true);
#endif
    }

    public void RequestMulti(int count) => pendingMultiShot += count;
    public void RequestSpread(int count) => pendingSpread += count;

    public void Attack(Vector2 dir, bool fromPlayer)
    {
        LastAttackDir = dir;
        pendingMultiShot = 1;
        pendingSpread = 0;

        foreach (var e in player.itemInventory.preAttackItems)
            e.OnPreAttack(player, dir);
        foreach (var e in player.abilityInventory.preAttackItems)
            e.OnPreAttack(player, dir);

        Vector2 perp = new(-dir.y, dir.x);
        const float MULTI_SPACING = 0.2f;
        float startOffset = -(pendingMultiShot - 1) * 0.5f * MULTI_SPACING;
        for (int i = 0; i < pendingMultiShot; i++)
        {
            Vector2 pos = (Vector2)attackPoint.position + perp * (startOffset + MULTI_SPACING * i);
            Shoot(dir, pos);
        }

        float baseAngle = Vector2.SignedAngle(Vector2.right, dir);
        for (int i = 0; i < pendingSpread; i++)
        {
            int sign = (i % 2 == 0) ? 1 : -1;
            float offset = (i / 2 + 1) * 40f;
            float rad = (baseAngle + sign * offset) * Mathf.Deg2Rad;
            Shoot(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)), attackPoint.position);
        }

        foreach (var e in player.itemInventory.attackItems)
            e.OnAttack(player, dir);
        foreach (var e in player.abilityInventory.attackItems)
            e.OnAttack(player, dir);

        if (fromPlayer)
        {
            RunComboAttacks(dir).Forget();
            cameraShake.Shake(0.15f);
        }

        attackTimer = 0f;

        if (fromPlayer)
        {
            CurBulletCount--;
            if (CurBulletCount <= 0)
                CoReload().Forget();
        }

        bulletFiredEvent.currentBulletCount = CurBulletCount;
        bulletFiredEvent.maxBulletCount = statMgr.BulletCount;
        bulletFiredEvent.fromPlayer = fromPlayer;
        bulletFiredEvent.dir = dir;
        GameEventBus.Publish(bulletFiredEvent);

    }

    public PlayerBullet Shoot(Vector2 dir, Vector2 pos)
    {
        if (pos == Vector2.zero)
            pos = attackPoint.position;

        var bullet = PlayerBullet.Instantiate();
        bullet.ClearBehaviors();
        bullet.ClearBulletForce();
        bullet.transform.position = pos;

        foreach (var e in player.itemInventory.bulletItems)
            e.OnBulletFired(bullet);
        foreach (var e in player.abilityInventory.bulletItems)
            e.OnBulletFired(bullet);

        bullet.Shoot(dir, statMgr.AttackPower);
        return bullet;
    }

    public void QueueExtraShot(int count = 1)
    {
        extraShotCount += count;
        if (!processingExtraShots)
            ProcessExtraShots().Forget();
    }

    async UniTaskVoid ProcessExtraShots()
    {
        processingExtraShots = true;
        var token = this.GetCancellationTokenOnDestroy();
        while (extraShotCount > 0)
        {
            int delay = Mathf.Max(30, 100 - extraShotCount * 10);
            await UniTask.Delay(delay, cancellationToken: token);
            if (token.IsCancellationRequested) return;
            extraShotCount--;
            Attack(GetAttackDirection(), false);
        }
        processingExtraShots = false;
    }

    async UniTaskVoid RunComboAttacks(Vector2 dir)
    {
        foreach (var e in player.itemInventory.comboAttackItems)
            await e.OnAttack(player, dir);
        foreach (var e in player.abilityInventory.comboAttackItems)
            await e.OnAttack(player, dir);
    }

    async UniTaskVoid CoReload()
    {
        IsReloading = true;
        GameEventBus.Publish(new ReloadStartEvent(statMgr.ReloadTime, statMgr.ReloadSpeed));
        float maxCount = statMgr.BulletCount;
        float sec = statMgr.ReloadTime / maxCount;
        CurBulletCount = 0;
        var token = this.GetCancellationTokenOnDestroy();
        while (CurBulletCount < maxCount)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(sec), cancellationToken: token);
            CurBulletCount++;
            GameEventBus.Publish(new BulletChargedEvent(CurBulletCount, (int)maxCount));
        }
        CurBulletCount = statMgr.BulletCount;
        IsReloading = false;
        attackTimer = statMgr.AttackSpeed;
        GameEventBus.Publish(new ReloadEndEvent());
    }

}

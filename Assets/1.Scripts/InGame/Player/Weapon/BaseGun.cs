using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;

public abstract class BaseGun : MonoBehaviour, IGun
{
    public const int COMBO_ATTACK_INTERVAL_MS = 70;
    public int initBulletCount;//초기 개수
    public float reloadTime;

    public BulletInventory bulletInventory;
    [SerializeField] public Transform attackPoint;
    [SerializeField] public Transform dirTr;

    protected Player player;
    protected PlayerStatManager statMgr;
    CameraShake cameraShake;
    Camera mainCamera;

    public Transform AttackPoint => attackPoint;
    public Vector2 LastAttackDir { get; private set; }

    public bool IsReloading { get; private set; }
    public List<string> loadedBullets = new List<string>();

    // public int CurBulletOrder => curBulletOrder;

    // private int curBulletOrder;

    // int pendingMultiShot = 1;
    // int pendingSpread;
    // int extraShotCount;
    // bool processingExtraShots;

    readonly BulletFiredEvent bulletFiredEvent = new();

    // Player 및 의존 컴포넌트 참조 초기화
    public void Init(Player player)
    {
        this.player = player;
        bulletInventory = GetComponentInChildren<BulletInventory>();
        statMgr = player.statMgr;
        cameraShake = player.cameraShake;
        mainCamera = Camera.main;

        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
    }

    void OnStartGame(StartGameEvent e)
    {
        // Debug.Log("OnStartGame BaseGun");
        for (int i = 0; i < initBulletCount; i++)
        {
            AddBullet("Normal");
        }

    }


    public void AddBullet(string key)
    {
        // Debug.Log($"OnStartGame AddBullet {key}");
        AddBullet(BulletData.GetBulletData(key));
    }

    public void AddBullet(BulletData bulletData)
    {
        bulletInventory.AddBullet(bulletData);

        LoadBullet(bulletData.key);
        BulletInventoryUI.Instance.AddedBullet(bulletData.key);
        GameEventBus.Publish(new AddedBulletEvent(bulletData));

    }
    public void ReleaseBullet(string key)
    {
        bulletInventory.ReleaseBullet(key);
        BulletInventoryUI.Instance.RemovedBullet(key);
        GameEventBus.Publish(new RemovedBulletEvent(BulletData.GetBulletData(key)));

    }


    // 시련 진입 시 무기 상태 초기화
    public void ResetOnUndergroundStart()
    {
        IsReloading = false;
        // CurBulletCount = statMgr.BulletCount;
        // extraShotCount = 0;
        // processingExtraShots = false;
        // pendingMultiShot = 1;
        // pendingSpread = 0;
        GameEventBus.Publish(new ReloadEndEvent());
    }

    // 마우스(PC) 기준 공격 방향 계산
    public Vector2 GetAttackDirection()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.WorldToScreenPoint(attackPoint.position).z;
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePosition);
        return (worldMousePos - attackPoint.position).normalized;
    }

    // 매 프레임 호출: 조준 방향 갱신 + 자동 발사 판정
    public void UpdateWeapon()
    {
        dirTr.up = GetAttackDirection();
        if (IsReloading) return;


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

    // AttackSpeed 간격마다 Attack 호출
    void UpdateAttackInternal()
    {
        attackTimer += Time.deltaTime;

#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        if (attackTimer >= statMgr.AttackSpeed)
            Attack(GetAttackDirection());
#else
        if (attackJoystick.Direction.magnitude > 0 && attackTimer >= statMgr.AttackSpeed)
            Attack(attackJoystick.Direction.normalized);
#endif
    }

    // 다음 Attack에서 발사할 멀티샷 수 누적 (아이템/어빌리티에서 호출)
    // public void RequestMulti(int count) => pendingMultiShot += count;
    // 다음 Attack에서 발사할 확산탄 수 누적
    // public void RequestSpread(int count) => pendingSpread += count;

    // 추가 발사 요청 누적 후 순차 처리 시작 (중복 실행 방지) - 뒤따라 발사함
    // public void QueueExtraShot(int count = 1)
    // {
    //     extraShotCount += count;
    //     if (!processingExtraShots)
    //         ProcessExtraShots().Forget();
    // }

    // 실제 발사 처리: preAttack 콜백 → 멀티/확산 Shoot → postAttack 콜백 → 장전 판정
    public void Attack(Vector2 dir) //Player한테서만 불려야함
    {
        if (IsReloading)
            return;
        if (loadedBullets.Count <= 0)
            return;

        LastAttackDir = dir;
        // pendingMultiShot = 1;
        // pendingSpread = 0;
        var (bullet, shotOrder) = SpendBullet();

        foreach (var e in player.itemInventory.preAttacks)
            e.OnPreAttack(player, dir, shotOrder);
        foreach (var e in bulletInventory.preAttacks)
            e.OnPreAttack(player, dir, shotOrder);


        Shoot(bullet, dir, (Vector2)attackPoint.position);


        // 멀티샷: 발사 방향에 수직으로 간격을 두어 여러 발 생성
        // Vector2 perp = new(-dir.y, dir.x);
        // const float MULTI_SPACING = 0.2f;
        // float startOffset = -(pendingMultiShot - 1) * 0.5f * MULTI_SPACING;
        // for (int i = 0; i < pendingMultiShot; i++)
        // {
        //     Vector2 pos = (Vector2)attackPoint.position + perp * (startOffset + MULTI_SPACING * i);
        //     Shoot(bullet, dir, pos);
        // }

        // 확산탄: 기준 방향에서 좌우 교대로 40도씩 벌려서 발사
        // float baseAngle = Vector2.SignedAngle(Vector2.right, dir);
        // for (int i = 0; i < pendingSpread; i++)
        // {
        //     int sign = (i % 2 == 0) ? 1 : -1;
        //     float offset = (i / 2 + 1) * 40f;
        //     float rad = (baseAngle + sign * offset) * Mathf.Deg2Rad;
        //     Shoot(bullet, new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)), attackPoint.position);
        // }

        foreach (var e in player.itemInventory.attacks)
            e.OnAttack(player, dir);
        foreach (var e in bulletInventory.attacks)
            e.OnAttack(player, dir);

        RunComboAttacks(dir).Forget();
        cameraShake.Shake(0.15f);

        attackTimer = 0f;

        if (loadedBullets.Count <= 0)
            CoReload().Forget();

        bulletFiredEvent.bullet = bullet;
        bulletFiredEvent.dir = dir;

        BulletInventoryUI.Instance.FiredBullet(bullet.key, shotOrder);
        GameEventBus.Publish(bulletFiredEvent);
    }

    // 총알 인스턴스 생성 후 아이템/어빌리티 효과 적용하여 발사
    public PlayerBulletObject Shoot(Bullet bullet, Vector2 dir, Vector2 pos)
    {
        if (pos == Vector2.zero)
            pos = attackPoint.position;
        if (dir == Vector2.zero)
            dir = Player.Instance.attackJoystick.Direction.normalized;


        var playerBullet = bullet.GetBulletObject();//bullet.GetBulletObject();
        playerBullet.ClearBehaviors();
        playerBullet.ClearBulletForce();
        playerBullet.transform.position = pos;

        foreach (var e in player.itemInventory.bullets)
            e.OnBulletFired(playerBullet);

        bullet.OnBulletFired(playerBullet);

        playerBullet.Shoot(dir, statMgr.AttackPower);
        return playerBullet;
    }


    // 누적된 추가 발사를 짧은 딜레이 간격으로 순차 처리
    // async UniTaskVoid ProcessExtraShots()
    // {
    //     processingExtraShots = true;
    //     var token = this.GetCancellationTokenOnDestroy();
    //     while (extraShotCount > 0)
    //     {
    //         int delay = Mathf.Max(30, 100 - extraShotCount * 10);
    //         await UniTask.Delay(delay, cancellationToken: token);
    //         if (token.IsCancellationRequested) return;
    //         extraShotCount--;

    //         Shoot(new NormalBullet(), GetAttackDirection(), attackPoint.position);
    //     }
    //     processingExtraShots = false;
    // }

    // 아이템/어빌리티의 콤보 공격을 순서대로 실행
    async UniTaskVoid RunComboAttacks(Vector2 dir)
    {
        foreach (var e in player.itemInventory.comboAttacks)
            await e.OnAttack(player, dir);

        foreach (var e in bulletInventory.comboAttacks)
            await e.OnAttack(player, dir);
    }

    public (Bullet bullet, int shotOrder) SpendBullet()
    {
        if (loadedBullets.Count <= 0)
            return (null, 0);

        int shotOrder = bulletInventory.bullets.Count - loadedBullets.Count;
        Bullet bullet = BulletManager.bullets[loadedBullets[loadedBullets.Count - 1]];

        // Debug.Log($"Shoot {bullet.key} shot Ordre {shotOrder}");
        loadedBullets.RemoveAt(loadedBullets.Count - 1);
        return (bullet, shotOrder);
    }

    public void LoadBullet(string bulletKey)
    {
        if (IsReloading)
            return;

        loadedBullets.Insert(0, bulletKey);
    }

    // 탄약 소진 시 한 발씩 충전하는 장전 처리
    async UniTaskVoid CoReload()
    {
        IsReloading = true;
        GameEventBus.Publish(new ReloadStartEvent(reloadTime, statMgr.ReloadSpeed));
        //float maxCount = bulletInventory.GetMaxBulletCount();
        // float sec = reloadTime / maxCount;
        // Debug.Log("재장전 시작 ");

        var token = this.GetCancellationTokenOnDestroy();

        float timer = 0;
        while (true)
        {
            if (timer > reloadTime)
                break;

            await UniTask.Yield(cancellationToken: token);
            timer += Time.deltaTime * statMgr.ReloadSpeed;

        }
        IsReloading = false;
        // Debug.Log("재장전 끝");
        // await UniTask.Yield(cancellationToken: token);
        for (int i = 0; i < bulletInventory.curBullets.Count; i++)
        {
            LoadBullet(bulletInventory.curBullets[i]);
        }
        BulletInventoryUI.Instance.EndReload();

        await UniTask.Yield(cancellationToken: token);
        attackTimer = statMgr.AttackSpeed;
        GameEventBus.Publish(new ReloadEndEvent());
    }

    public void OnAddedBulletEvent(AddedBulletEvent e)
    {

    }
}

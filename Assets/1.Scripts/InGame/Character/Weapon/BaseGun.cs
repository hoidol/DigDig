using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using LayerLab.ArtMakerUnity;

public abstract class BaseGun : MonoBehaviour, IGun
{
    public const int COMBO_ATTACK_INTERVAL_MS = 100;
    public int initBulletCount;//초기 개수
    public float reloadTime;

    // public BulletInventory bulletInventory;
    [SerializeField] public Transform attackPoint;
    [SerializeField] public Transform dirTr;

    protected Character player;
    protected CharacterStatManager statMgr;
    CameraShake cameraShake;
    Camera mainCamera;

    public Transform AttackPoint => attackPoint;
    public Vector2 LastDir { get; private set; }

    // public bool IsReloading { get; private set; }
    // public List<string> loadedBullets = new List<string>();
    readonly BulletFiredEvent bulletFiredEvent = new();
    public SFXPlayer sfxPlayer;

    // Player 및 의존 컴포넌트 참조 초기화
    public void Init(Character player)
    {
        this.player = player;
        // bulletInventory = GetComponentInChildren<BulletInventory>();
        statMgr = player.statMgr;
        cameraShake = player.cameraShake;
        mainCamera = Camera.main;
        LastDir = Vector2.right;
        dirTr.up = Vector2.right;

        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
    }

    void OnStartGame(StartGameEvent e)
    {

    }

    public LayerMask targetLayerMask;
    // 가장 가까운 적 기준 공격 방향 계산
    public Vector2 GetAttackDirection()
    {
        Transform targetTr = InGameUtil.FindTarget(transform.position, 10, targetLayerMask);

        if (targetTr == null)
            return Character.Instance.MoveDirection;

        return (targetTr.position - transform.position).normalized;
    }

    // 매 프레임 호출: 조준 방향 갱신 + 자동 발사 판정
    public void UpdateWeapon()
    {
        if (!GameManager.Instance.isPlaying) return;

        dirTr.up = GetAttackDirection();

        // #if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        //         if (Input.GetMouseButton(0))
        //             dirTr.up = GetAttackDirection();    
        // #endif
        UpdateAttackInternal();
        LastDir = dirTr.up;
    }

    float attackTimer;

    // AttackSpeed 간격마다 Attack 호출
    void UpdateAttackInternal()
    {
        //statMgr.AttackSpeed
        attackTimer += Time.deltaTime * statMgr.AttackSpeed / 50;

#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        if (attackTimer >= 1)
            Attack(GetAttackDirection());
#else
        if (attackJoystick.Direction.magnitude > 0 && attackTimer >= 1)
            Attack(attackJoystick.Direction.normalized);
#endif
    }

    // 다음 Attack에서 발사할 멀티샷 수 누적 (아이템/어빌리티에서 호출)
    // public void RequestMulti(int count) => pendingMultiShot += count;
    // 다음 Attack에서 발사할 확산탄 수 누적
    // int pendingSpread;
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
        // if (IsReloading)
        //     return;

        // pendingMultiShot = 1;
        // pendingSpread = 0;
        // var (bullet, shotOrder) = SpendBullet();

        BulletSpec bullet = new NormalBulletSpec();

        foreach (var e in player.itemInventory.preFires)
            e.OnPreFire(ref bullet, dir);

        // var bulletObject = bullet.GetBulletObject();

        AllyBulletObject bulletObject = Shoot(bullet, dir);
        // Character.Instance.AddHp(-bullet.bulletData.consumeHp);

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
        // pendingSpread=0;

        foreach (var e in player.itemInventory.fireds)
            e.OnFired(ref bullet, ref bulletObject, dir);

        RunComboAttacks(dir).Forget();
        // cameraShake.Shake(0.15f);

        attackTimer = 0f;

        // if (loadedBullets.Count <= 0)
        //     CoReload().Forget();

        bulletFiredEvent.bullet = bullet;
        bulletFiredEvent.dir = dir;

        // BulletInventoryUI.Instance.FiredBullet(bullet.key, shotOrder);
        GameEventBus.Publish(bulletFiredEvent);
    }

    // 총알 인스턴스 생성 후 아이템/어빌리티 효과 적용하여 발사
    public CharacterBulletObject Shoot(BulletSpec bullet, Vector2 dir)
    {
        if (dir == Vector2.zero)
            dir = GetAttackDirection();

        if (bullet == null)
        {
            bullet = new CharacterBulletSpec();
        }

        CharacterBulletObject characterBulletObject = bullet.Instantiate(Character.Instance) as CharacterBulletObject; // 총알 초기화됨
        characterBulletObject.transform.position = attackPoint.position;

        characterBulletObject.Shoot(dir, Character.Instance.statMgr.AttackPower);
        sfxPlayer.Play();
        return characterBulletObject;
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
        foreach (var e in player.itemInventory.comboFires)
            await e.OnComboFire(dir);
    }




}

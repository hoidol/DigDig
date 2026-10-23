using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting;
using LayerLab.ArtMakerUnity;

public abstract class Weapon : MonoBehaviour, IGun
{
    public const int COMBO_ATTACK_INTERVAL_MS = 100;
    public int initBulletCount;//초기 개수
    public float reloadTime;
    public AttackType attackType;
    // public BulletInventory bulletInventory;
    [SerializeField] public Transform attackPoint;
    // [SerializeField] public Transform dirTr;

    protected Character player;
    protected CharacterStatManager statMgr;
    // CameraShake cameraShake;
    public Transform AttackPoint => attackPoint;
    public Vector2 AttackDir { get; private set; }

    // public bool IsReloading { get; private set; }
    // public List<string> loadedBullets = new List<string>();
    readonly BulletFiredEvent bulletFiredEvent = new();
    [SerializeField] float attackTimer;
    public LayerMask targetLayerMask;
    public SFXPlayer sfxPlayer;

    // Player 및 의존 컴포넌트 참조 초기화
    public void Init(Character player)
    {
        this.player = player;
        // bulletInventory = GetComponentInChildren<BulletInventory>();
        statMgr = player.statMgr;
        // cameraShake = player.cameraShake;
        AttackDir = Vector2.zero;
        // dirTr.up = Vector2.right;

        GameEventBus.Subscribe<StartGameEvent>(OnStartGame);
    }

    void OnStartGame(StartGameEvent e)
    {

    }
    // public float attackFindRange = 10;
    // 가장 가까운 적 기준 공격 방향 계산
    public Vector2 GetAttackDirection()
    {
        Transform targetTr = FindTarget();

        if (targetTr == null)
        {
            // Debug.Log("BaseGun GetAttackDirection if (targetTr == null)");
            return Vector2.zero;
        }


        return (targetTr.position - transform.position).normalized;
    }
    public virtual Transform FindTarget()
    {
        return InGameUtil.FindTarget(transform.position, Character.Instance.statMgr.AttackRange + 0.3f, targetLayerMask, Character.Instance.findTargetType);
    }



    // 매 프레임 호출: 조준 방향 갱신 + 자동 발사 판정
    public void UpdateWeapon()
    {
        if (!GameManager.Instance.isPlaying) return;


        // #if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        //         if (Input.GetMouseButton(0))
        //             dirTr.up = GetAttackDirection();    
        // #endif

        attackTimer += Time.deltaTime * statMgr.AttackSpeed / GameSetting.ATTACK_SPEED_PER_SECOND;
        if (attackTimer >= 1)
        {
            // 랜덤 타겟 차례면 이번 공격만 타겟 방식을 Random으로
            FindTargetType prevType = player.findTargetType;
            if (player.shotModifier.IsRandomShotTurn)
                player.findTargetType = FindTargetType.Random;

            AttackDir = GetAttackDirection();
            // Debug.Log("BaseGun UpdateWeapon ");
            Attack(AttackDir); // 방향을 한 번만 구함 (Random일 때 두 번 구하면 AttackDir과 달라짐)
            player.findTargetType = prevType;
        }

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
        if (dir == Vector2.zero)
            return;
        // if (IsReloading)
        //     return;

        // pendingMultiShot = 1;
        // pendingSpread = 0;
        // var (bullet, shotOrder) = SpendBullet();

        BulletSpec bullet = new CharacterBulletSpec();
        Character.Instance.bodyRootTr.localScale = new Vector3(AttackDir.x >= 0 ? 1 : -1, 1, 1);
        // foreach (var e in player.itemInventory.preFires)
        //     e.OnPreFire(ref bullet, dir);

        // var bulletObject = bullet.GetBulletObject();

        Shoot(bullet, dir); // 메인 탄
        // Character.Instance.AddHp(-bullet.bulletData.consumeHp);

        // 다중 타겟: 추가 탄은 서로 다른 타겟을 노림 (fireds 콜백은 메인 탄에만)
        int count = 1 + player.shotModifier.bonusShootCount;
        if (count > 1)
        {
            List<Transform> targets = InGameUtil.FindTargets(transform.position, Character.Instance.statMgr.AttackRange + 0.3f, targetLayerMask, Character.Instance.findTargetType, count);
            for (int i = 1; i < count && targets.Count > 0; i++)
            {
                Transform target = targets[i % targets.Count];
                Vector2 d = ((Vector2)(target.position - transform.position)).normalized;
                Shoot(bullet, d);
            }
        }
        player.shotModifier.CountAttack();


        // foreach (var e in player.itemInventory.fireds)
        //     e.OnFired(ref bullet, ref bulletObject, dir);
        // RunComboAttacks(dir).Forget();

        attackTimer = 0f;


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
        TileItem tileItem = Character.Instance.shotModifier.tileItem;
        if (tileItem != null)
            tileItem.OnFired(characterBulletObject);
        sfxPlayer.Play();
        return characterBulletObject;
    }





}

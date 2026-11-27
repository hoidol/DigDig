using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NormalEnemy : Enemy
{
    public NormalEnemyState state { get; private set; } // 적 상태 - FSM 패턴
    public Vector3 HpUIPosition => hpPoint.position;

    // protected float attackTimer;
    // protected bool attacking;


    [SerializeField] protected bool moving;
    // protected float moveTimer;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Spawn(Vector2 pos, int wave)
    {
        base.Spawn(pos, wave);
        // moveTimer = MOVE_SPEED;
        //apearTime 초 후에 등장
        moving = false;

        root.localRotation = Quaternion.identity;
        // damageData.damage = GetAttackPower();

        ChangeState(NormalEnemyState.Moving);
    }

    public void ChangeState(NormalEnemyState state)
    {
        this.state = state;
    }

    // 처치 코인 = 1 + KILL_GOLD_WAVE_INTERVAL(20) wave마다 +1 (waveIdx 기준: wave 1~20 = 1, 21~40 = 2, 41~60 = 3)
    public static int GetKillGold(int waveIdx) => 1 + Mathf.Max(0, waveIdx) / GameSetting.KILL_GOLD_WAVE_INTERVAL;

    public override void Reward()
    {
        // 카드 소환 모드: 주사위 코인은 wave 준비 단계 지급만 (처치로는 못 얻음)
        // if (!GameSetting.USE_WAVE_MINION_OFFER && Random.Range(0f, 10f) < 1)
        // {
        //     Slime.Instance.AddDiceCoin(1);
        //     DiceCoinText.Instantiate().SetDiceCoinText(transform.position, $"+{1}");    
        // }
        // // (2026-11-22) 처치 경험치 없음 (경험치는 주사위 코인으로 구매)
        // int gold = GetKillGold(wave);
        // Slime.Instance.AddCoin(gold);
        // SoundManager.Play(SFXType.CoinGet);
        // CoinText.Instantiate().SetCoinText(transform.position, $"+{gold}");
    }

    public override void TakeDamage(DamageData damage)
    {
        base.TakeDamage(damage);
        if (IsDying)
            return;
        //일반형 - 직관적으로 데미지 입음
        root.DOKill();
        root.localRotation = Quaternion.identity;
        root.DOShakeRotation(0.2f, new Vector3(0f, 0f, 5f), 100, 90f, false).OnComplete(() =>
        {
            root.localRotation = Quaternion.identity;
        });
    }

    public override void Destroy()
    {
        base.Destroy();

        CancelAttack();
        ChangeState(NormalEnemyState.Dead);

    }
}

public enum NormalEnemyState
{
    // Idle,
    Moving,
    //Attack,
    Dead
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// [S] 조커 (2026-11-16 신규, 카드를 쓰는 미니언. 이전 조커(변신)는 Darkness로 이름 변경)
// - 평소엔 일반 탄 공격
// - 스킬: SKILL_COOLTIME초마다 카드 한 장을 뽑음 (4종 같은 확률)
//   스페이드 = 공격 사거리 안 적 전부에게 공격력 × SPADE_DAMAGE_MUL 가시 피해
//   다이아   = DIAMOND_DURATION초 동안 공속 × DIAMOND_ATTACK_SPEED_MUL, 공격력 × DIAMOND_ATTACK_POWER_MUL
//   클로버   = (임시) 코인 CLOVER_COIN_BASE + CLOVER_COIN_PER_MERGE × 머지 레벨
//   하트     = 꽝 (아무 효과 없음)
// - 사거리에 적이 없으면 쿨타임이 찬 채로 대기 (공용 규칙), 다이아 버프 중엔 쿨타임 멈춤
// - 뽑은 카드는 머리 위 글씨로 보여줌 (Resources/Effect/JokerCardEffect, EffectType.JokerCard)
// 수치는 임시
public class JokerMinion : Minion
{
    public enum Card { Spade, Diamond, Clover, Heart }

    public const float SKILL_COOLTIME = 5f;
    public const float SPADE_DAMAGE_MUL = 3f;
    // 기본 탄 프리팹 (AddressableResources/BulletObject/Level1/Card: 일반 탄 동작 + 카드 그림 랜덤(BulletRandomSprite) + 회전(BulletSpin))
    public const string CARD_BULLET_KEY = "Card";
    public const float DIAMOND_DURATION = 5f;
    public const float DIAMOND_ATTACK_SPEED_MUL = 5f;
    public const float DIAMOND_ATTACK_POWER_MUL = 0.5f;
    public const int CLOVER_COIN_BASE = 5;
    public const int CLOVER_COIN_PER_MERGE = 2;
    const int SPADE_MAX_TARGETS = 100; // "주변 전부"

    static readonly Color SPADE_COLOR = new Color(0.55f, 0.6f, 0.75f);
    static readonly Color DIAMOND_COLOR = new Color(1f, 0.35f, 0.35f);
    static readonly Color CLOVER_COLOR = new Color(0.45f, 0.9f, 0.4f);
    static readonly Color HEART_COLOR = new Color(1f, 0.3f, 0.6f); // 꽝 = 분홍 (회색은 밝은 타일 위에서 안 보임)

    AllyBulletSpec allyBulletSpec;
    [Tooltip("머리 위에 보여줄 카드 그림 (Card 순서: 스페이드, 다이아, 클로버, 하트)")]
    public Sprite[] cardSprites;
    float skillTimer;
    readonly List<Buff> diamondBuffs = new List<Buff>();
    float diamondTimer;
    bool IsDiamondActive => diamondTimer > 0f;
    readonly AllyUnitDamageData spadeDamageData = new AllyUnitDamageData();

    public Card LastCard { get; private set; }
    public override float SkillProgress => IsDiamondActive ? 0f : Mathf.Clamp01(skillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { skillTimer = SKILL_COOLTIME; }

    public static int GetCloverCoin(int mergeLv) => CLOVER_COIN_BASE + CLOVER_COIN_PER_MERGE * mergeLv;

    public override void Spawn(Vector2 pos, int lv)
    {
        // 풀링 재사용: 이전 다이아 버프 제거
        foreach (Buff buff in diamondBuffs)
            activeBuffs.Remove(buff);
        diamondBuffs.Clear();
        diamondTimer = 0f;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        allyBulletSpec.key = CARD_BULLET_KEY; // 기본 탄 = 카드 (4종 중 랜덤 그림, 빙글빙글)
        skillTimer = 0f;
        base.InitMinion();
    }

    public override void Update()
    {
        base.Update();
        if (UpdateDiamond())
            return; // 다이아 버프 중엔 쿨타임 멈춤
        if (IsBeingDragged) // 드래그 중엔 스킬 발동 안 함
            return;
        if (skillTimer < SKILL_COOLTIME)
        {
            skillTimer += SkillDeltaTime;
            return;
        }
        // 사거리에 적이 없으면 찬 채로 대기
        List<Transform> targets = InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, SPADE_MAX_TARGETS);
        if (targets == null || targets.Count == 0)
            return;
        skillTimer = 0f;
        DrawCard((Card)Random.Range(0, 4), targets);
    }

    public void DrawCard(Card card, List<Transform> targets = null)
    {
        LastCard = card;
        PlaySkillAnim();
        SoundManager.Play(SFXType.JokerCardDraw);
        switch (card)
        {
            case Card.Spade:
                ShowCard("Joker_Card_Spade", SPADE_COLOR);
                Spade(targets ?? InGameUtil.FindTargets(transform.position, AttackRange, targetLayerMask, findTargetType, SPADE_MAX_TARGETS));
                break;
            case Card.Diamond:
                ShowCard("Joker_Card_Diamond", DIAMOND_COLOR);
                Diamond();
                break;
            case Card.Clover:
                int coin = GetCloverCoin(mergeLevel);
                if (KingSlime.Instance != null)
                    KingSlime.Instance.AddCoin(coin);
                ShowCard("Joker_Card_Clover", CLOVER_COLOR, $" +{coin}");
                break;
            default:
                ShowCard("Joker_Card_Heart", HEART_COLOR); // 꽝
                break;
        }
    }

    void Spade(List<Transform> targets)
    {
        if (targets == null)
            return;
        float damage = AttackPower * SPADE_DAMAGE_MUL;
        foreach (Transform target in targets)
        {
            if (target == null || !target.TryGetComponent(out IHittable hittable))
                continue;
            if (EffectManager.Instance != null)
                EffectManager.Instance.Play(EffectType.Hit, Enemy.GetHitPosition(hittable));
            spadeDamageData.Init(this);
            spadeDamageData.damage = damage;
            spadeDamageData.attackType = AttackType;
            AccumulateDamage(damage);
            hittable.TakeDamage(spadeDamageData);
        }
    }

    void Diamond()
    {
        diamondBuffs.Clear();
        diamondBuffs.Add(new Buff(StatType.AttackSpeed, DIAMOND_ATTACK_SPEED_MUL, StatOpType.Multiply));
        diamondBuffs.Add(new Buff(StatType.AttackPower, DIAMOND_ATTACK_POWER_MUL, StatOpType.Multiply));
        foreach (Buff buff in diamondBuffs)
            AddBuff(buff);
        diamondTimer = DIAMOND_DURATION;
        PlayPowerUpEffect(DIAMOND_DURATION);
    }

    // 다이아 버프 시간 처리 (끝나면 RemoveBuff). 버프 중이면 true
    bool UpdateDiamond()
    {
        if (diamondTimer <= 0f)
            return false;
        diamondTimer -= Time.deltaTime;
        if (diamondTimer > 0f)
            return true;
        diamondTimer = 0f;
        foreach (Buff buff in diamondBuffs)
            RemoveBuff(buff);
        diamondBuffs.Clear();
        StopPowerUpEffect();
        return false;
    }

    // 머리 위에 뽑은 카드 그림 + 글씨 (머지 "레벨업!"과 같은 연출)
    void ShowCard(string textKey, Color color, string suffix = "")
    {
        if (EffectManager.Instance == null)
            return;
        Effect effect = EffectManager.Instance.GetEffect(EffectType.JokerCard);
        if (effect == null)
            return;
        TMP_Text text = effect.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = TranslateManager.GetText(textKey) + suffix;
            text.color = color;
        }
        SpriteRenderer cardImage = effect.GetComponentInChildren<SpriteRenderer>(true);
        int cardIdx = (int)LastCard;
        if (cardImage != null && cardSprites != null && cardIdx < cardSprites.Length && cardSprites[cardIdx] != null)
            cardImage.sprite = cardSprites[cardIdx];
        effect.Play(transform.position);
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }
}

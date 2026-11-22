using System.Collections.Generic;
using UnityEngine;

// [S] 다크니스 (2026-11-16 스킬형으로 변경)
// - 스킬: 필드(타일 위)에서 게임 진행 중 SKILL_COOLTIME초가 차면 변신 가능 → 다크니스를 선택하면 머리 위에 변신 목록(DarknessTransformPopup)
// - 목록 = 장착 미니언 중 다크니스 자신 제외. 고르면 같은 타일·같은 머지 레벨로 변신 (실패 없음, 머지 레벨 증가 없음)
// - 쿨타임이 찬 뒤엔 원할 때까지 대기 (아껴 뒀다가 필요한 종이 생기면 변신하는 카드)
// - 충전된 동안 몸 뒤에 검은 연기(Resources/Effect/DarknessReadyEffect, EffectType.DarknessReady)
// - 변신 전까지는 일반 탄 공격, 드래그 중엔 목록 숨김 (쿨타임은 옮겨도 유지)
// (이전: 변신 버튼 + 선택 창(일시정지), 30% 확률 실패 → 머지 레벨 -1)
public class DarknessMinion : MinionGrowth1
{
    public const float SKILL_COOLTIME = 15f;
    public const float TRANSFORM_SUCCESS_CHANCE = 0.7f; // 변신 성공 확률 (연속 실패 보정 MinionChancePity 적용)
    public const float TRANSFORM_GROW_CHANCE = 0.1f; // 변신 성공 시 머지 레벨 +1(다음 단계로 성장) 확률. 최대 레벨이면 그대로
    static readonly Color SUCCESS_TEXT_COLOR = new Color(0.75f, 0.45f, 1f);
    static readonly Color GROW_TEXT_COLOR = new Color(1f, 0.85f, 0.2f);
    static readonly Color FAIL_TEXT_COLOR = EffectManager.POP_TEXT_FAIL_COLOR; // 공용 실패 색

    // 최대 머지 레벨에서는 쓸 수 있는 각성이 없어 진화하지 않음 (머지 레벨을 올리는 슬라임)
    public override bool CanEvolve => false;

    AllyBulletSpec allyBulletSpec;
    float skillTimer;
    bool isDragging;

    public bool IsDragging => isDragging;
    public bool IsSkillReady => skillTimer >= SKILL_COOLTIME;
    public override float SkillProgress => Mathf.Clamp01(skillTimer / SKILL_COOLTIME);
    public override void FillSkillForTest() { skillTimer = SKILL_COOLTIME; }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        skillTimer = 0f;
        isDragging = false;
        base.InitMinion();
    }

    public override void Update()
    {
        base.Update();
        UpdateReadyEffect();
        if (IsSkillReady)
            return;
        if (GameManager.Instance != null && !GameManager.Instance.isPlaying)
            return;
        if (TileManager.Instance == null || TileManager.Instance.GetTile(this) == null)
            return;
        skillTimer += SkillDeltaTime;
        if (IsSkillReady)
            PlaySkillAnim();
    }

    // 충전된 동안 몸 뒤 검은 연기 (따라다님, 미니언이 꺼지면 같이 끝남)
    ParticleEffect readyEffect;
    void UpdateReadyEffect()
    {
        bool show = IsSkillReady;
        bool playing = readyEffect != null && readyEffect.FollowTarget == transform;
        if (show && !playing && EffectManager.Instance != null)
        {
            readyEffect = EffectManager.Instance.GetEffect(EffectType.DarknessReady) as ParticleEffect;
            if (readyEffect != null)
                readyEffect.PlayFollow(transform, float.MaxValue);
        }
        else if (!show && playing)
        {
            StopReadyEffect();
        }
    }

    void StopReadyEffect()
    {
        if (readyEffect != null && readyEffect.FollowTarget == transform)
            readyEffect.EndEffect();
        readyEffect = null;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        StopReadyEffect();
    }

    public override void StartDrag()
    {
        base.StartDrag();
        isDragging = true;
    }

    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        isDragging = false;
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    // 변신 가능한 종: 장착 미니언 중 다크니스 자신 제외
    public List<string> GetTransformKeys()
    {
        List<string> keys = new List<string>();
        foreach (UserMinion userMinion in UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions)
        {
            if (userMinion != null && !string.IsNullOrEmpty(userMinion.key) && userMinion.key != key && !keys.Contains(userMinion.key))
                keys.Add(userMinion.key);
        }
        return keys;
    }

    // 변신: 쿨타임이 찼고 필드(타일 위)에 있을 때만. TRANSFORM_SUCCESS_CHANCE 확률로 성공
    // 성공 = 같은 타일·같은 머지 레벨로 교체(선택 유지는 ReplaceMinion) + "변환 성공!"
    //        그중 TRANSFORM_GROW_CHANCE 확률로 머지 레벨 +1 + "변환 성공! 성장!"
    // 실패 = 다크니스 그대로, 쿨타임만 처음부터 + "변환 실패..." (null 반환)
    public Minion TransformTo(string targetKey) =>
        TransformTo(targetKey, MinionChancePity.RollInGame(key, TRANSFORM_SUCCESS_CHANCE), Random.value < TRANSFORM_GROW_CHANCE);

    public static int GetTransformMergeLevel(int mergeLevel, bool grow) =>
        grow ? Mathf.Min(mergeLevel + 1, MinionData.MAX_MERGE_LEVEL) : mergeLevel;

    public Minion TransformTo(string targetKey, bool success, bool grow = false)
    {
        if (!IsSkillReady || TileManager.Instance.GetTile(this) == null)
            return null;
        Vector3 pos = transform.position;
        SoundManager.Play(SFXType.DarknessTransform); // 성공·실패 공용
        if (!success)
        {
            skillTimer = 0f;
            EffectManager.PlayPopText(pos, TranslateManager.GetText("Darkness_Transform_Fail"), FAIL_TEXT_COLOR);
            return null;
        }
        int nextLevel = GetTransformMergeLevel(mergeLevel, grow);
        bool grew = nextLevel > mergeLevel;
        Minion result = MinionFieldController.Instance.ReplaceMinion(this, targetKey, nextLevel);
        if (result != null)
            EffectManager.PlayPopText(result.transform.position, TranslateManager.GetText(grew ? "Darkness_Transform_Grow" : "Darkness_Transform_Success"), grew ? GROW_TEXT_COLOR : SUCCESS_TEXT_COLOR);
        return result;
    }
}

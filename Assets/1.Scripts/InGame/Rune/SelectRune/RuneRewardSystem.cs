using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-23) 룬 = 엘리트·보스 처치 보상
// 처치 → Slime.pendingRuneRewards +1 (처치 자리에 "다음 웨이브 룬!") → 다음 브레이크타임 시작에 룬 선택 창
// 창 = 룬 3장(SlimeGrowthSetting.runeKeys 중 랜덤) → 하나 고르고 칸 선택 (빈 칸 = 새김 / 같은 룬 칸 = 레벨 +1). 반드시 골라야 닫힘
// 보상이 여러 개면 연속으로 열림. 고르는 동안 Next·레벨업 창은 기다림
public static class RuneRewardSystem
{
    public const int CHOICE_COUNT = 3;

    public static void AddReward(Vector3 pos)
    {
        Slime slime = Slime.Instance;
        if (slime == null)
            return;
        slime.pendingRuneRewards++;
        RuneRewardChest.Spawn(pos); // 처치 자리에 보상 상자 (창에도 같은 상자가 보임)
        HeroUpgradeSystem.OnEliteBossKilled(pos); // (2026-11-24) 용사 강화: 확률로 다음 브레이크타임 주사위 코인 +1
        EffectManager.PlayPopText(pos, "다음 웨이브 룬 선택!", new Color(0.75f, 0.55f, 1f));
    }

    public static bool IsOpen => SelectRuneCanvas.Instance != null && SelectRuneCanvas.Instance.IsRuneRewardOpen;

    // 브레이크타임에 밀린 룬 보상이 있으면 창 열기 (MinionOfferSystem.OnBreakTimeStart, 보상 하나 끝날 때마다)
    public static void TryOpen()
    {
        Slime slime = Slime.Instance;
        SelectRuneCanvas canvas = SelectRuneCanvas.Instance;
        if (slime == null || canvas == null || slime.pendingRuneRewards <= 0 || IsOpen)
            return;
        if (GameManager.Instance == null || !GameManager.Instance.isBreakTime)
            return;
        if ((InGame.LevelUpCanvas.Instance != null && InGame.LevelUpCanvas.Instance.gameObject.activeSelf))
            return; // 레벨업 창이 끝나면 다시 시도 (LevelUpCanvas 닫힐 때)

        List<RuneData> runes = PickChoices();
        if (runes.Count == 0)
        {
            slime.pendingRuneRewards = 0; // 고를 룬이 없음 (설정 비어 있음)
            RuneRewardChest.ClearAll();
            return;
        }
        RuneRewardChest.ConsumeOne(); // 필드의 상자 하나가 사라지고 창에 같은 상자
        canvas.OpenRuneReward(runes, OnRewardDone);
    }

    // 룬 후보 = 레벨업 룬과 같은 풀 (SlimeGrowthSetting.runeKeys). avoid = 가능하면 빼고 뽑을 룬 (광고 갱신)
    public static List<RuneData> PickChoices(ICollection<RuneData> avoid = null)
    {
        if (RuneManager.Instance == null)
            return new List<RuneData>();
        SlimeGrowthSetting setting = SlimeGrowthSetting.Instance;
        return RuneManager.Instance.GetLevelUpRunes(setting != null ? setting.runeKeys : null)
            .OrderBy(d => avoid != null && avoid.Contains(d)).ThenBy(_ => Random.value).Take(CHOICE_COUNT).ToList();
    }

    static void OnRewardDone()
    {
        Slime slime = Slime.Instance;
        if (slime != null && slime.pendingRuneRewards > 0)
            slime.pendingRuneRewards--;
        if (slime != null && slime.pendingRuneRewards > 0)
        {
            TryOpen();
            return;
        }
        InGame.LevelUpCanvas.OpenPending(); // 룬을 고르는 동안 밀린 레벨업
    }
}

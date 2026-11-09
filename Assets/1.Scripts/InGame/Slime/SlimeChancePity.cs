using System.Collections.Generic;
using UnityEngine;

// 슬라임 확률 판정 연속 실패 보정 (조커 변신, 희생/성장 추가 레벨 등)
// - 판정 key(보통 슬라임 key)별로 연속 실패 횟수를 센다. 한 판 동안 유지(GameManager에 하나)
// - 연속 PITY_FAIL_COUNT(4)회 실패하면 그다음 시도부터 성공 확률 +PITY_BONUS_CHANCE(50%p), 성공할 때까지 유지
// - 성공하면 연속 실패 횟수 초기화 (4회 전에 성공해도 초기화)
public class SlimeChancePity
{
    public const int PITY_FAIL_COUNT = 4;
    public const float PITY_BONUS_CHANCE = 0.5f;

    readonly Dictionary<string, int> failStreaks = new Dictionary<string, int>();

    public static float GetChance(float baseChance, int failStreak)
    {
        if (failStreak >= PITY_FAIL_COUNT)
            baseChance += PITY_BONUS_CHANCE;
        return Mathf.Clamp01(baseChance);
    }

    public static int NextFailStreak(int failStreak, bool success) => success ? 0 : failStreak + 1;

    public int GetFailStreak(string key) => failStreaks.TryGetValue(key, out int streak) ? streak : 0;

    // successChance(0~1)로 판정하고 연속 실패 횟수를 갱신. 확률 0 이하·1 이상은 판정 없이 결과만 (횟수 변화 없음)
    public bool Roll(string key, float successChance)
    {
        if (successChance <= 0f)
            return false;
        if (successChance >= 1f)
            return true;

        int streak = GetFailStreak(key);
        bool success = Random.value < GetChance(successChance, streak);
        failStreaks[key] = NextFailStreak(streak, success);
        return success;
    }

    // GameManager가 없을 때(테스트 등)는 보정 없이 판정
    public static bool RollInGame(string key, float successChance)
    {
        if (GameManager.Instance != null)
            return GameManager.Instance.slimeChancePity.Roll(key, successChance);
        return successChance > 0f && Random.value < successChance;
    }
}

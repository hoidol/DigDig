using UnityEngine;

// (2026-11-27) 한 판 결과 통계 (결과 화면 ResultStatsPanel이 읽음). 초기화 = GameManager.StartGame (부활해도 이어서 셈)
// 피해는 DamageMeterSystem(미니언 종별 + 슬라임 기본 공격·스킬), 합성 횟수는 MinionFieldController.mergeCount
public static class GameResultStats
{
    public static float PlayTime { get; private set; }     // 실제 시간(초, 배속·일시정지와 무관하게 흐른 시간)
    public static int KillCount { get; private set; }      // 처치한 적 (경로 끝 도착 제외, 엘리트·보스 포함)
    public static int EliteKillCount { get; private set; } // 엘리트·보너스 엘리트
    public static int BossKillCount { get; private set; }
    public static int LeakCount { get; private set; }      // 경로 끝에 도착한 적
    public static int SlimeDamageTaken { get; private set; }// 슬라임이 받은 피해 (체력)
    public static int PlacedCardCount { get; private set; }// 놓은 미니언 카드 (빈 칸 소환 + 카드 합성)
    public static int EvolveCount { get; private set; }    // 진화한 미니언

    public static void Clear()
    {
        PlayTime = 0;
        KillCount = EliteKillCount = BossKillCount = LeakCount = 0;
        SlimeDamageTaken = PlacedCardCount = EvolveCount = 0;
    }

    public static void AddPlayTime(float seconds) => PlayTime += seconds;

    // 이어하기 때 플레이 기록의 시간을 이어받음
    public static void RestoreFrom(float playTime) => PlayTime = playTime;

    public static void RecordEnemyRemoved(Enemy enemy, bool killed)
    {
        if (enemy == null)
            return;
        if (!killed)
        {
            LeakCount++;
            return;
        }
        KillCount++;
        if (enemy is Boss || enemy.enemyType == EnemyType.Boss)
            BossKillCount++;
        else if (enemy.enemyType == EnemyType.Elite || enemy.enemyType == EnemyType.BonusElite)
            EliteKillCount++;
    }

    public static void RecordSlimeDamage(float damage)
    {
        if (damage > 0)
            SlimeDamageTaken += Mathf.RoundToInt(damage);
    }

    public static void RecordCardPlaced() => PlacedCardCount++;
    public static void RecordEvolve() => EvolveCount++;

    // 125초 → "2:05", 1시간 이상 → "1:02:05"
    public static string FormatTime(float seconds)
    {
        int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return s >= 3600 ? $"{s / 3600}:{s / 60 % 60:00}:{s % 60:00}" : $"{s / 60}:{s % 60:00}";
    }
}

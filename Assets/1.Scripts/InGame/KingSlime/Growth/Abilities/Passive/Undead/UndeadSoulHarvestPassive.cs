using UnityEngine;

// 언데드 · 버프형 · 영혼 수확: 킹 사거리 안에서 적이 처치될 때마다 버프 칸 미니언 스킬 쿨타임 -seconds초 (Minion.AddSkillTime)
// 쿨타임을 채우는 중인 스킬에만 적용 (쿨타임 스킬이 없는 미니언은 효과 없음)
public class UndeadSoulHarvestPassive : PassiveKingAbility
{
    public float[] seconds = { 0.3f, 0.55f, 0.8f };

    protected override bool ListenKills => true;

    public override object[] GetDescArgs(int level) => new object[] { FormatNumber(LV(seconds, level)) };

    protected override void Apply() { }

    protected override void OnEnemyKilled(Enemy enemy, Vector2 pos)
    {
        if (!InKingRange(pos))
            return;
        float sec = LV(seconds);
        foreach (Minion m in AreaMinions)
            m.AddSkillTime(sec);
    }
}

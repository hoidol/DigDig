using UnityEngine;

// 인간 · 공격형 · 일제 사격: 킹이 attackCounts번 공격할 때마다 버프 칸 미니언 전원이 바로 1회 공격 (공격 타이머를 채움)
public class HumanVolleyPassive : PassiveKingAbility
{
    public int[] attackCounts = { 6, 5, 4 };

    int count;

    public override object[] GetDescArgs(int level) => new object[] { LV(attackCounts, level) };

    protected override void Apply() { }

    public override void OnKingAttack()
    {
        count++;
        if (count < LV(attackCounts))
            return;
        count = 0;
        foreach (Minion m in AreaMinions)
            m.attackTimer = Mathf.Max(m.attackTimer, 1f);
    }
}

using UnityEngine;

// 오크 · 버프형 · 피의 함성: 킹 사거리 안에서 적이 처치될 때마다 버프 칸 미니언 공속 +speedPerStack 중첩 (최대 maxStacks)
// 마지막 처치 후 keepTime(실제 게임 시간) 동안 처치가 없으면 중첩 초기화 → 적이 계속 죽는 자리에 킹을 두게 함
public class OrcWarCryPassive : PassiveKingAbility
{
    public float speedPerStack = 0.03f;
    public int[] maxStacks = { 10, 15, 20 };
    public float keepTime = 5f;

    int stack;
    float timer;

    protected override bool ListenKills => true;

    public override object[] GetDescArgs(int level) =>
        new object[] { Percent(speedPerStack), LV(maxStacks, level), FormatNumber(keepTime) };

    protected override void Apply() => ApplyStack();
    protected override void Clear() => Area.RemoveBuffs(this);

    protected override void OnEnemyKilled(Enemy enemy, Vector2 pos)
    {
        if (!InKingRange(pos))
            return;
        timer = keepTime;
        if (stack >= LV(maxStacks))
            return;
        stack++;
        ApplyStack();
    }

    void Update()
    {
        if (stack <= 0)
            return;
        timer -= Time.deltaTime;
        if (timer > 0f)
            return;
        stack = 0;
        ApplyStack();
    }

    void ApplyStack()
    {
        if (stack <= 0)
        {
            Area.RemoveBuffs(this);
            return;
        }
        Area.SetBuffs(this, new[] { new Buff(StatType.AttackSpeed, 1f + speedPerStack * stack, StatOpType.Multiply) });
    }
}

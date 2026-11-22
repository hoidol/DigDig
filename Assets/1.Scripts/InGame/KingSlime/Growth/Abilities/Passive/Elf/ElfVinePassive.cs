using UnityEngine;

// 엘프 · 공격형 · 얽히는 덩굴: 버프 칸 위·근처(칸 중심에서 radius 안)를 지나는 적 이동속도 -slows (VineEffect, 중첩 없음)
// tickInterval마다 확인해 duration 동안 걸어 둠 → 칸을 벗어나면 곧 풀림. 버프 칸을 적 경로 쪽으로 향하게 하는 배치 포인트
public class ElfVinePassive : PassiveKingAbility
{
    public float[] slows = { 0.2f, 0.3f, 0.4f };
    public float radius = 0.9f;
    public float tickInterval = 0.25f;
    public float duration = 0.5f;

    float timer;

    public override object[] GetDescArgs(int level) => new object[] { Percent(LV(slows, level)) };

    protected override void Apply() { }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f)
            return;
        timer = tickInterval;
        if (EnemyFieldController.Instance == null)
            return;
        var tiles = Area.GetTargetTiles();
        if (tiles.Count == 0)
            return;
        float slow = LV(slows);
        float sqr = radius * radius;
        foreach (Enemy enemy in EnemyFieldController.Instance.activeEnemies)
        {
            if (enemy == null || enemy.CurHp <= 0)
                continue;
            Vector2 p = enemy.transform.position;
            foreach (Tile tile in tiles)
            {
                if (((Vector2)tile.transform.position - p).sqrMagnitude <= sqr)
                {
                    enemy.ApplyStatusEffect(new VineEffect(slow, duration));
                    break;
                }
            }
        }
    }
}

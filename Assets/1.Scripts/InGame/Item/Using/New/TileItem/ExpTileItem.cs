using UnityEngine;

// (2026-11-21) 경험치 룬: 이 칸 유닛이 적을 처치하면(마지막 타격, Enemy.LastAttacker) 레벨 값% 확률로 킹 경험치 +exp (Values 30/50/70)
public class ExpTileItem : TileItem
{
    public float chance = 30f;
    public int exp = 1;

    protected override void ApplyLevelValue(float v) => chance = v;
    protected override string FormatLevelValue(float v) => FormatNumber(v);

    void OnEnable() => GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    void OnDisable() => GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (tile == null || e.enemy == null || e.enemy.CurHp > 0)
            return;
        if (tile.tileObject == null || !ReferenceEquals(e.enemy.LastAttacker, tile.tileObject))
            return;
        if (Random.Range(0f, 100f) >= chance)
            return;
        KingSlime.Instance?.AddExp(exp);
        EffectManager.PlayPopText(e.position, $"EXP +{exp}", new Color(0.55f, 0.9f, 1f));
    }
}

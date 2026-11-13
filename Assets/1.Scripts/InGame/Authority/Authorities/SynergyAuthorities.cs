using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 묶음 2 (Lv.10) 공격 형태 · 묶음 3 (Lv.15) 특성 시너지 · 묶음 4 (Lv.20) 보드 규칙

// 분열의 파편: 탄이 적을 처치하면 두 갈래로 쪼개짐 (피해 50%, 쪼개진 탄은 다시 쪼개지지 않음) (AllyBulletObject.Hit)
public class SplitShardAuthority : Authority
{
    public const float DAMAGE_RATIO = 0.5f;
    public const float SPLIT_ANGLE = 30f;

    public static Vector2[] GetSplitDirs(Vector2 dir)
    {
        dir = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.up;
        return new[] { (Vector2)(Quaternion.Euler(0, 0, SPLIT_ANGLE) * dir), (Vector2)(Quaternion.Euler(0, 0, -SPLIT_ANGLE) * dir) };
    }

    public static void Split(AllyBulletObject bullet, Vector2 pos, Vector2 dir)
    {
        if (bullet.isSplitShard || bullet.spec == null || bullet.allyUnit == null)
            return;
        float damage = bullet.shootDamage * DAMAGE_RATIO;
        foreach (Vector2 d in GetSplitDirs(dir))
        {
            AllyBulletObject b = bullet.spec.Instantiate(bullet.allyUnit);
            b.isSplitShard = true;
            b.transform.position = pos;
            b.Shoot(d, damage);
        }
    }
}

// 잔불의 화로: 지뢰·박격포·낙뢰·지옥불이 피해를 준 자리에 3초간 불길 (초당 공격력의 20%)
public class EmberBrazierAuthority : Authority
{
    public const float LIFETIME = 3f;
    public const float DPS_RATIO = 0.2f;

    // 장판 피해 지점에서 호출 (Mine.Explode, MortarBulletObject.Explode, ThunderBulletObject.Strike, InfernoMinion.Fire)
    public static void OnZoneDamage(Vector2 pos, float radius, float attackPower, LayerMask mask)
    {
        if (!AuthoritySystem.Has<EmberBrazierAuthority>())
            return;
        EmberFire.Spawn(pos, radius, attackPower * DPS_RATIO, LIFETIME, mask);
    }

    public override void OnEnd() => EmberFire.ClearAll();
}

// 회오리 구슬: 회전체가 한 바퀴 돌 때마다 바깥으로 탄 1발 (OrbitMachine.Update)
public class WhirlOrbAuthority : Authority
{
    public static void OnRevolution(OrbitMachine machine)
    {
        if (!AuthoritySystem.Has<WhirlOrbAuthority>() || !(machine.owner is Minion minion) || minion == null || !minion.gameObject.activeInHierarchy)
            return;
        var spec = new AllyBulletSpec { damage = minion.AttackPower };
        spec.StartBulletSpec();
        AllyBulletObject b = spec.Instantiate(minion);
        b.transform.position = minion.transform.position;
        b.Shoot(UnityEngine.Random.insideUnitCircle.normalized, minion.AttackPower);
    }
}

// 전염의 성배: 상태이상이 2종 이상 걸린 적이 죽으면 주변 적에게 같은 상태이상이 옮겨감
public class PlagueChaliceAuthority : Authority
{
    public const int MIN_EFFECT_TYPES = 2;
    public const float SPREAD_RADIUS = 2f;
    public const int MAX_SPREAD = 5;

    // 디버프 종류 (버프·방어막·무적 제외, 빙결은 슬로우와 다른 종류로 셈)
    public static List<StatusEffect> GetDebuffs(IEnumerable<StatusEffect> effects)
    {
        return effects.Where(e => e != null && !e.isBuff && !(e is ShieldEffect) && !(e is InvincibleEffect))
            .GroupBy(e => e.GetType()).Select(g => g.First()).ToList();
    }

    // 같은 종류의 새 효과 (적마다 새로 만들어야 함)
    public static StatusEffect Copy(StatusEffect e)
    {
        switch (e)
        {
            case IceEffect ice: return new IceEffect(ice.duration);
            case MudEffect mud: return new MudEffect(mud.duration);
            case SlowEffect slow: return new SlowEffect(slow.duration);
            case StunEffect stun: return new StunEffect(stun.duration);
            case FlameEffect flame: return new FlameEffect(flame.duration, flame.DamagePerSecond);
            case PoisonEffect poison: return new PoisonEffect(poison.duration, poison.DamagePerTick);
            case StigmaEffect stigma: return new StigmaEffect(stigma.maxStack, stigma.damagePerStack);
            default: return null;
        }
    }

    public override void OnBegin() => GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    public override void OnEnd() => GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);

    void OnEnemyDead(EnemyDeadEvent e)
    {
        if (e.enemy == null || e.enemy.CurHp > 0 || !e.enemy.TryGetComponent(out StatusEffectHandler handler))
            return;
        List<StatusEffect> debuffs = GetDebuffs(handler.Effects);
        if (debuffs.Count < MIN_EFFECT_TYPES)
            return;

        var targets = Physics2D.OverlapCircleAll(e.position, SPREAD_RADIUS)
            .Select(c => c.GetComponent<Enemy>())
            .Where(en => en != null && en != e.enemy && en.CurHp > 0 && !en.IsDying)
            .OrderBy(en => Vector2.Distance(e.position, en.transform.position))
            .Take(MAX_SPREAD);
        foreach (Enemy target in targets)
            foreach (StatusEffect d in debuffs)
            {
                StatusEffect copy = Copy(d);
                if (copy != null)
                    target.ApplyStatusEffect(copy);
            }
        EffectManager.Instance.Play(EffectType.Spark, e.position);
    }
}

// 중계의 수정: 킹 주변 3*3 칸 영역에서 일어난 처치도 모든 처치 감지 슬라임이 감지 (Dopamine·Berserker·Scavenger)
public class RelayCrystalAuthority : Authority
{
    public const float AREA_TILES = 1.5f; // 킹 칸 중심에서 가로·세로 1.5칸 = 3*3

    // 감지 범위 판정: 내 범위 안이거나, 권능이 있고 킹 주변 3*3 안
    public static bool InDetectRange(Vector2 minionPos, float range, Vector2 deadPos)
    {
        if (Vector2.Distance(minionPos, deadPos) <= range)
            return true;
        return AuthoritySystem.Has<RelayCrystalAuthority>() && KingSlime.Instance != null && IsInKingArea(KingSlime.Instance.transform.position, deadPos, TileSize());
    }

    public static bool IsInKingArea(Vector2 kingPos, Vector2 pos, float tileSize)
    {
        float half = tileSize * AREA_TILES;
        return Mathf.Abs(pos.x - kingPos.x) <= half && Mathf.Abs(pos.y - kingPos.y) <= half;
    }

    static float cachedTileSize;
    static float TileSize()
    {
        if (cachedTileSize > 0f)
            return cachedTileSize;
        var tiles = TileManager.Instance.tiles;
        Tile a = tiles.FirstOrDefault(t => t.index == Vector2Int.zero);
        Tile b = tiles.FirstOrDefault(t => t.index == new Vector2Int(1, 0));
        cachedTileSize = a != null && b != null ? Vector2.Distance(a.transform.position, b.transform.position) : 1f;
        return cachedTileSize;
    }

    public override void OnBegin() => cachedTileSize = 0f; // 판마다 타일 배치가 다를 수 있음
}

// 지휘의 깃발: 십자 버프가 타일 그룹 경계를 넘어 적용 (CrossBuffMinion)
public class CommandFlagAuthority : Authority
{
    public static bool IgnoreGroupBorder => AuthoritySystem.Has<CommandFlagAuthority>();
}

// 측량사의 나침반: 지뢰·지옥불이 옆 타일 그룹의 경로도 사용 (MineMinion·InfernoMinion)
public class SurveyCompassAuthority : Authority
{
    // 내 그룹 + (권능이 있으면) 가로·세로로 붙은 그룹. 그룹 좌표 = 칸 index / 3
    public static List<TileGroup> GetPathGroups(TileGroup own)
    {
        var result = new List<TileGroup> { own };
        if (own == null || !AuthoritySystem.Has<SurveyCompassAuthority>())
            return result;
        Vector2Int c = GroupCoord(own);
        foreach (TileGroup g in TileManager.Instance.tileGroups)
        {
            if (g == own || g.enemyRoots == null || g.enemyRoots.Length == 0)
                continue;
            Vector2Int d = GroupCoord(g) - c;
            if (Mathf.Abs(d.x) + Mathf.Abs(d.y) == 1)
                result.Add(g);
        }
        return result;
    }

    static Vector2Int GroupCoord(TileGroup g)
    {
        Tile t = g.tiles != null && g.tiles.Length > 0 ? g.tiles[0] : g.GetComponentInChildren<Tile>();
        return t != null ? new Vector2Int(t.index.x / 3, t.index.y / 3) : Vector2Int.zero;
    }
}

// 쌍둥이 반지: 같은 종 2마리가 상하좌우로 붙어 있으면 둘 다 공격력이 머지 레벨 +1 수치 (Minion.twinBonus)
public class TwinRingAuthority : Authority
{
    static readonly Vector2Int[] OFFSETS = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public override void OnBegin()
    {
        GameEventBus.Subscribe<TileObjectChangedEvent>(OnTileChanged);
        Refresh();
    }

    public override void OnEnd()
    {
        GameEventBus.Unsubscribe<TileObjectChangedEvent>(OnTileChanged);
        foreach (Minion s in FieldMinions())
            s.SetTwinBonus(0);
    }

    void OnTileChanged(TileObjectChangedEvent e) => Refresh();

    static IEnumerable<Minion> FieldMinions()
    {
        if (TileManager.Instance == null)
            yield break;
        foreach (Tile t in TileManager.Instance.tiles)
            if (t != null && t.tileObject is Minion s && s != null)
                yield return s;
    }

    static void Refresh()
    {
        foreach (Tile tile in TileManager.Instance.tiles)
        {
            if (tile == null || !(tile.tileObject is Minion minion) || minion == null)
                continue;
            bool twin = TileManager.Instance.GetTilesAround(tile, OFFSETS).Any(t => t.tileObject is Minion other && other != minion && other.key == minion.key);
            minion.SetTwinBonus(twin ? 1 : 0);
        }
    }
}

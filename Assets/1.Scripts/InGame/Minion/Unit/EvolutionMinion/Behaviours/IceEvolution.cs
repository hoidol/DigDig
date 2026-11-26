using System.Collections.Generic;
using UnityEngine;

// 눈송이 (2026-11-26) 발동 스킬: 큰 눈(HailStone) hailCount개가 hailDuration초 동안 내 칸 묶음(TileGroup) 주변 적 경로를 돎
// 닿은 적 = 공격력 × hailDamageMul 피해 + iceChance 확률 빙결(눈송이 감속률, 1 = 100%) + stunChance 확률 스턴. 경로가 없으면 발동 안 함
// 수치 = MinionEvolutionSetting.ice (눈 프리팹 = ice.hailPrefab, 빙석 미니언 눈 재사용)
public class IceEvolution : ActiveSkillEvolution
{
    IceEvolutionConfig Cfg => Config<IceEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 10f;
    protected override float Duration => Cfg != null ? Cfg.hailDuration : 8f;

    class ActiveHail { public HailStone stone; public float travel; }
    readonly List<ActiveHail> hails = new List<ActiveHail>();
    readonly List<HailStone> pool = new List<HailStone>();
    protected override Color SkillColor => new Color(0.6f, 0.9f, 1f); // 하늘색

    protected override bool TryStart()
    {
        IceEvolutionConfig cfg = Cfg;
        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        if (cfg == null || cfg.hailPrefab == null || total <= 0f)
            return false;
        ClearHails();
        float start = PathLoop.GetNearestTravel(path, total, minion.transform.position);
        int count = Mathf.Max(1, cfg.hailCount);
        for (int i = 0; i < count; i++)
        {
            float travel = start + total / count * i;
            hails.Add(new ActiveHail { stone = GetStone(cfg), travel = travel });
            Vector2 at = PathLoop.GetPoint(path, total, Mathf.Repeat(travel, total));
            HeroSkillVisual.PlayRing(at, 0.6f, SkillColor, 0.4f, 0.08f); // 눈이 나타나는 자리
        }
        return true;
    }

    HailStone GetStone(IceEvolutionConfig cfg)
    {
        HailStone stone = pool.Find(s => s != null && !s.gameObject.activeSelf);
        if (stone == null)
        {
            stone = Instantiate(cfg.hailPrefab, transform);
            pool.Add(stone);
        }
        stone.Init(minion, cfg.hitCooldown);
        stone.iceChance = cfg.iceChance * 100f;
        stone.iceDuration = cfg.iceDuration;
        stone.iceSlowRate = minion is IceMinion ice ? ice.iceSlowRate : IceEffect.DEFAULT_SLOW_RATE;
        stone.stunChance = cfg.stunChance;
        stone.stunDuration = cfg.stunDuration;
        stone.gameObject.SetActive(false); // 위치를 정한 뒤 켬
        return stone;
    }

    protected override void RunTick(float dt)
    {
        IceEvolutionConfig cfg = Cfg;
        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        bool show = cfg != null && !minion.IsBeingDragged && total > 0f;
        bool playing = GameManager.Instance == null || GameManager.Instance.isPlaying;
        RotationDirection dir = RotationDirection.Clockwise;
        float step = show ? PathLoop.GetTravelSign(path, dir) * cfg.hailSpeed * dt : 0f;
        foreach (ActiveHail h in hails)
        {
            if (h.stone == null)
                continue;
            if (h.stone.gameObject.activeSelf != show)
                h.stone.gameObject.SetActive(show);
            if (!show)
                continue;
            if (playing)
            {
                h.travel = Mathf.Repeat(h.travel + step, total);
                h.stone.Spin(dir, dt);
            }
            h.stone.damage = minion.AttackPower * cfg.hailDamageMul;
            h.stone.transform.position = PathLoop.GetPoint(path, total, h.travel);
        }
    }

    protected override void End() => ClearHails();

    void ClearHails()
    {
        foreach (ActiveHail h in hails)
            if (h.stone != null)
                h.stone.gameObject.SetActive(false);
        hails.Clear();
    }

    Transform[] GetPath()
    {
        if (TileManager.Instance == null || minion == null)
            return null;
        Tile tile = TileManager.Instance.GetTile(minion);
        TileGroup group = tile != null ? tile.tileGroup : null;
        return group != null && group.enemyRoots != null && group.enemyRoots.Length >= 2 ? group.enemyRoots : null;
    }
}

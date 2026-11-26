using System.Collections.Generic;
using UnityEngine;

// 둥둥이 (2026-11-26) 발동 스킬(머리 위 버튼, 쿨타임 cooldown): duration초 동안 구름 cloudCount개가 미니언 주변(orbitRadius)을 돌며
// 구름마다 strikeInterval초마다 구름 아래로 번개 → 반경 strikeRadius 안 적 전부 공격력 × damageMul + stunDuration초 스턴 (구름끼리 번개 시점을 엇갈림)
// 적이 없어도 발동. 구름 = cloudPrefab (없으면 경고 후 번개만). 수치 = MinionEvolutionSetting.thunder
public class ThunderEvolution : ActiveSkillEvolution
{
    ThunderEvolutionConfig Cfg => Config<ThunderEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 10f;
    protected override float Duration => Cfg != null ? Cfg.duration : 5f;

    class Cloud
    {
        public GameObject go;
        public float strikeTimer;
    }
    readonly List<Cloud> clouds = new List<Cloud>();
    readonly List<GameObject> pool = new List<GameObject>();
    float angle;

    protected override bool TryStart()
    {
        ThunderEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return false;
        if (cfg.cloudPrefab == null)
            Debug.LogWarning("[ThunderEvolution] 구름 프리팹 연결 없음 (MinionEvolutionSetting.thunder.cloudPrefab) → 번개만");
        ClearClouds();
        int count = Mathf.Max(1, cfg.cloudCount);
        float interval = Mathf.Max(0.1f, cfg.strikeInterval);
        for (int i = 0; i < count; i++)
            clouds.Add(new Cloud { go = GetCloud(cfg), strikeTimer = interval * i / count }); // 번개 시점을 엇갈림
        angle = 0f;
        Place(cfg);
        minion.PlayPowerUpEffect(Duration);
        return true;
    }

    GameObject GetCloud(ThunderEvolutionConfig cfg)
    {
        if (cfg.cloudPrefab == null)
            return null;
        GameObject go = pool.Find(g => g != null && !g.activeSelf);
        if (go == null)
        {
            go = Instantiate(cfg.cloudPrefab, transform);
            pool.Add(go);
        }
        go.SetActive(true);
        return go;
    }

    protected override void RunTick(float dt)
    {
        ThunderEvolutionConfig cfg = Cfg;
        if (cfg == null)
            return;
        bool dragging = minion.IsBeingDragged;
        foreach (Cloud c in clouds)
            if (c.go != null && c.go.activeSelf == dragging)
                c.go.SetActive(!dragging);
        if (dragging)
            return;
        angle += cfg.orbitSpeed * dt;
        Place(cfg);
        float interval = Mathf.Max(0.1f, cfg.strikeInterval);
        for (int i = 0; i < clouds.Count; i++)
        {
            clouds[i].strikeTimer -= dt;
            if (clouds[i].strikeTimer > 0f)
                continue;
            clouds[i].strikeTimer += interval;
            Strike(GroundPos(cfg, i), cfg);
        }
    }

    // i번째 구름이 번개를 떨어뜨리는 땅 자리 (미니언 둘레, 같은 간격)
    Vector2 GroundPos(ThunderEvolutionConfig cfg, int i)
    {
        float a = (angle + 360f * i / Mathf.Max(1, clouds.Count)) * Mathf.Deg2Rad;
        return (Vector2)minion.transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * cfg.orbitRadius;
    }

    void Place(ThunderEvolutionConfig cfg)
    {
        for (int i = 0; i < clouds.Count; i++)
            if (clouds[i].go != null)
                clouds[i].go.transform.position = GroundPos(cfg, i) + Vector2.up * cfg.cloudHeight;
    }

    void Strike(Vector2 pos, ThunderEvolutionConfig cfg)
    {
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.Thunder, pos);
        float damage = minion.AttackPower * cfg.damageMul;
        foreach (IHittable e in FindEnemies(minion, pos, cfg.strikeRadius))
        {
            DealDamage(minion, e, damage);
            if (cfg.stunDuration > 0f)
                ApplyStatus(e, new StunEffect(cfg.stunDuration));
        }
    }

    protected override void End()
    {
        ClearClouds();
        if (minion != null)
            minion.StopPowerUpEffect();
    }

    void ClearClouds()
    {
        foreach (Cloud c in clouds)
            if (c.go != null)
                c.go.SetActive(false);
        clouds.Clear();
    }
}

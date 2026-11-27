using System.Collections.Generic;
using UnityEngine;

// 발비마 (2026-11-27) 발동 스킬(쿨타임 10초, 버튼): 폭탄 하나가 내 칸 묶음(TileGroup) 주변 적 경로를 미니언과 가장 가까운 지점부터 한 바퀴 굴러감
// 굴러가다 적에 닿으면(triggerRadius) 그 자리 폭발 = 범위(explodeRadius) 안 적 최대 explodeMaxHit기 공격력 × explodeDamageMul 피해. 같은 적에게 닿아 터지는 건 한 번
// 한 바퀴를 돌면 사라짐 (Duration = 경로 길이 / 속도). 경로가 없으면 발동 안 함. 수치 = MinionEvolutionSetting.mine
// 그림 = 발비 지뢰(MineMinion.minePrefab)의 그림을 빌려 굴림 (임시, 이펙트가 정해지면 교체)
public class MineEvolution : ActiveSkillEvolution
{
    MineEvolutionConfig Cfg => Config<MineEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 10f;
    protected override float Duration => rollDuration;
    protected override Color SkillColor => new Color(1f, 0.55f, 0.2f); // 주황

    float rollDuration = 1f;
    float travel;
    float rolled;
    Transform bomb;
    readonly HashSet<IHittable> triggered = new HashSet<IHittable>();

    protected override bool TryStart()
    {
        MineEvolutionConfig cfg = Cfg;
        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        if (cfg == null || total <= 0f || cfg.rollSpeed <= 0f)
            return false;
        rollDuration = total / cfg.rollSpeed;
        travel = PathLoop.GetNearestTravel(path, total, minion.transform.position);
        rolled = 0f;
        triggered.Clear();
        EnsureBomb(cfg);
        bomb.position = PathLoop.GetPoint(path, total, travel);
        bomb.gameObject.SetActive(true);
        HeroSkillVisual.PlayRing(bomb.position, 0.7f, SkillColor, 0.4f, 0.08f); // 폭탄이 나타나는 자리
        return true;
    }

    void EnsureBomb(MineEvolutionConfig cfg)
    {
        if (bomb != null)
        {
            bomb.localScale = Vector3.one * cfg.bombScale;
            return;
        }
        var go = new GameObject("RollingBomb");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        Sprite sprite = null;
        if (minion is MineMinion mm && mm.minePrefab != null)
        {
            SpriteRenderer src = mm.minePrefab.GetComponentInChildren<SpriteRenderer>(true);
            if (src != null)
            {
                sprite = src.sprite;
                sr.sortingLayerID = src.sortingLayerID;
                sr.sortingOrder = src.sortingOrder + 1;
            }
        }
        sr.sprite = sprite;
        if (sprite == null)
            sr.color = SkillColor;
        go.transform.localScale = Vector3.one * cfg.bombScale;
        bomb = go.transform;
        go.SetActive(false);
    }

    protected override void RunTick(float dt)
    {
        MineEvolutionConfig cfg = Cfg;
        Transform[] path = GetPath();
        float total = PathLoop.GetLength(path);
        if (cfg == null || bomb == null || total <= 0f)
            return;
        bool show = !minion.IsBeingDragged;
        if (bomb.gameObject.activeSelf != show)
            bomb.gameObject.SetActive(show);
        if (!show || (GameManager.Instance != null && !GameManager.Instance.isPlaying))
            return;
        float step = cfg.rollSpeed * dt;
        rolled += step;
        travel = Mathf.Repeat(travel + PathLoop.GetTravelSign(path, RotationDirection.Clockwise) * step, total);
        bomb.position = PathLoop.GetPoint(path, total, travel);
        bomb.Rotate(0f, 0f, -360f * dt); // 굴러가는 모습

        foreach (IHittable touched in FindEnemies(minion, bomb.position, cfg.triggerRadius, 4))
        {
            if (!triggered.Add(touched))
                continue;
            Explode(cfg, bomb.position);
            break; // 한 프레임에 한 번만 터짐
        }
    }

    void Explode(MineEvolutionConfig cfg, Vector2 at)
    {
        foreach (IHittable e in FindEnemies(minion, at, cfg.explodeRadius, Mathf.Max(1, cfg.explodeMaxHit)))
        {
            triggered.Add(e);
            DealDamage(minion, e, minion.AttackPower * cfg.explodeDamageMul);
        }
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.MortarExplosion, at);
        HeroSkillVisual.PlayRing(at, cfg.explodeRadius, SkillColor, 0.3f, 0.1f);
        SoundManager.Play(SFXType.MineExplosion);
    }

    protected override void End()
    {
        if (bomb != null)
            bomb.gameObject.SetActive(false);
        triggered.Clear();
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

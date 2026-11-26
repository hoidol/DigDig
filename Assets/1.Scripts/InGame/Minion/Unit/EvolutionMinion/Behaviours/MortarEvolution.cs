using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 달그락 (2026-11-26) 발동 스킬(머리 위 버튼, 쿨타임 cooldown): 깃발 자리부터 적이 오는 쪽(경로 반대 방향)으로 bombCount발 폭탄 (간격 = 폭발 범위 × spacingMul)
// 맞은 적 stunChance 확률 스턴. 적이 없어도 발동. 수치 = MinionEvolutionSetting.mortar
public class MortarEvolution : ActiveSkillEvolution
{
    MortarEvolutionConfig Cfg => Config<MortarEvolutionConfig>();
    protected override float Cooldown => Cfg != null ? Cfg.cooldown : 15f;
    protected override Color SkillColor => new Color(1f, 0.6f, 0.25f); // 주황
    protected override float Duration => Cfg != null ? Mathf.Max(0.2f, Cfg.dropInterval * Cfg.bombCount) : 0.6f; // 폭탄이 다 떨어질 때까지 버튼 잠김

    // 적이 없어도 발동 (깃발 자리부터 경로를 따라 폭탄)
    protected override bool TryStart()
    {
        MortarEvolutionConfig cfg = Cfg;
        if (cfg == null || !(minion is MortarMinion mortar))
            return false;
        DropBombs(mortar, cfg).Forget();
        return true;
    }

    async UniTaskVoid DropBombs(MortarMinion mortar, MortarEvolutionConfig cfg)
    {
        Vector2 flag = mortar.FlagPos;
        Vector2 back = -GetPathDirection(flag);
        float spacing = mortar.boomRange * cfg.spacingMul;
        var stun = new StunOnHitBehavior(cfg.stunDuration, cfg.stunChance);
        var token = this.GetCancellationTokenOnDestroy();
        int bombs = Mathf.Max(1, cfg.bombCount);
        for (int i = 0; i < bombs; i++) // (2026-11-26) 떨어질 자리 경고 원 (폭발 범위)
            HeroSkillVisual.PlayRing(flag + back * spacing * i, mortar.boomRange, SkillColor, 0.35f + cfg.dropInterval * i, 0.07f);
        for (int i = 0; i < Mathf.Max(1, cfg.bombCount); i++)
        {
            if (i > 0 && cfg.dropInterval > 0f
                && await UniTask.Delay(System.TimeSpan.FromSeconds(cfg.dropInterval), cancellationToken: token).SuppressCancellationThrow())
                return;
            if (mortar == null || !mortar.gameObject.activeInHierarchy)
                return;
            mortar.FireBombAt(flag + back * spacing * i, stun);
        }
    }

    // 깃발 근처 적 이동 방향 (주 경로에서 가장 가까운 구간의 진행 방향). 경로가 없으면 아래쪽
    public static Vector2 GetPathDirection(Vector2 pos)
    {
        EnemyFieldController field = EnemyFieldController.Instance;
        Transform[] route = field != null ? field.MainRoute : null;
        if (route == null || route.Length < 2)
            return Vector2.down;
        float best = float.MaxValue;
        Vector2 dir = Vector2.down;
        for (int i = 0; i < route.Length - 1; i++)
        {
            if (route[i] == null || route[i + 1] == null)
                continue;
            Vector2 a = route[i].position, b = route[i + 1].position;
            Vector2 ab = b - a;
            float len2 = ab.sqrMagnitude;
            if (len2 < 0.0001f)
                continue;
            float t = Mathf.Clamp01(Vector2.Dot(pos - a, ab) / len2);
            float d = (a + ab * t - pos).sqrMagnitude;
            if (d < best)
            {
                best = d;
                dir = ab.normalized;
            }
        }
        return dir;
    }
}

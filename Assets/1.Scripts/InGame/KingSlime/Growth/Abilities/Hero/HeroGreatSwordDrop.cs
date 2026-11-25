using UnityEngine;

// (2026-11-25) 기사 2단계 "심판의 대검": 누른 곳에 경고 원 → 대검이 위에서 떨어져 꽂힘 → 착탄(KnightHero.GreatswordImpact) → 잠깐 남았다 사라짐
public class HeroGreatSwordDrop : MonoBehaviour
{
    const float FALL_HEIGHT = 4f;
    const float STAY_TIME = 0.5f;
    static readonly Color WARN_COLOR = new Color(1f, 0.35f, 0.25f, 0.8f);

    KnightHero owner;
    Vector2 target;
    float radius, fallTime, t;
    bool impacted;
    Transform sword;
    SpriteRenderer swordSprite;
    LineRenderer warn;

    public static HeroGreatSwordDrop Spawn(KnightHero owner, Vector2 pos, float radius, float fallTime, Sprite sprite)
    {
        var go = new GameObject("HeroGreatSword");
        go.transform.position = pos;
        var d = go.AddComponent<HeroGreatSwordDrop>();
        d.owner = owner;
        d.target = pos;
        d.radius = radius;
        d.fallTime = Mathf.Max(0.05f, fallTime);

        d.warn = HeroSkillVisual.CreateLine("Warn", WARN_COLOR, 0.07f, true, go.transform);
        HeroSkillVisual.SetCircle(d.warn, pos, radius);

        if (sprite != null)
        {
            d.swordSprite = HeroSkillVisual.CreateSprite("Sword", sprite, Color.white);
            d.sword = d.swordSprite.transform;
            d.sword.SetParent(go.transform, false);
            d.sword.localRotation = Quaternion.Euler(0, 0, -90f); // 칼끝이 아래 (그림은 칼끝이 오른쪽)
            float size = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            d.sword.localScale = Vector3.one * (radius * 1.6f / Mathf.Max(0.01f, size));
        }
        else
        {
            LineRenderer lr = HeroSkillVisual.CreateLine("Sword", Color.white, 0.3f, false, go.transform);
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(0, radius * 0.1f));
            lr.SetPosition(1, new Vector3(0, radius * 1.6f));
            d.sword = lr.transform;
        }
        d.sword.localPosition = new Vector3(0, FALL_HEIGHT);
        return d;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        t += Time.deltaTime;
        if (!impacted)
        {
            float k = Mathf.Clamp01(t / fallTime);
            sword.localPosition = new Vector3(0, Mathf.Lerp(FALL_HEIGHT, radius * 0.35f, k * k));
            HeroSkillVisual.SetAlpha(warn, WARN_COLOR, 0.5f + 0.5f * Mathf.PingPong(t * 8f, 1f));
            if (k >= 1f)
            {
                impacted = true;
                t = 0f;
                if (owner != null)
                    owner.GreatswordImpact(target);
                HeroSkillVisual.PlayRing(target, radius, new Color(1f, 0.9f, 0.6f, 0.9f), 0.4f, 0.12f);
            }
            return;
        }
        float fade = 1f - Mathf.Clamp01(t / STAY_TIME);
        HeroSkillVisual.SetAlpha(warn, WARN_COLOR, fade);
        if (swordSprite != null)
        {
            Color c = swordSprite.color;
            c.a = fade;
            swordSprite.color = c;
        }
        if (t >= STAY_TIME)
            Destroy(gameObject);
    }
}

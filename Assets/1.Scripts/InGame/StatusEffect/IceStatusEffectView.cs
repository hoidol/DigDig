using UnityEngine;

// 아이스: 몸통 스프라이트(SpriteForStatusEffect)를 하늘색으로 틴트
public class IceStatusEffectView : StatusEffectView
{
    public Color tintColor = new Color(0.6f, 0.85f, 1f, 1f);

    SpriteForStatusEffect[] sprites;

    public override void Play()
    {
        base.Play();
        if (sprites == null)
            sprites = GetComponentInParent<StatusEffectHandler>()?.GetComponentsInChildren<SpriteForStatusEffect>(true);
        if (sprites != null)
        {
            foreach (var sprite in sprites)
                sprite.SetTint(tintColor);
        }

    }

    public override void Stop()
    {
        base.Stop();
        if (sprites == null)
            return;

        foreach (var sprite in sprites)
            sprite.ResetTint();
    }
}

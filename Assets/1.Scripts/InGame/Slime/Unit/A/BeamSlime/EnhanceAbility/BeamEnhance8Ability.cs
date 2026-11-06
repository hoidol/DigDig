using UnityEngine;

public class BeamEnhance8Ability : SlimeEnhanceAbility, IForceSlimeEnhanceAbility
{
    // [각성] 레이저에 닿은 적 감속 (피해 틱마다 slowDuration 슬로우)
    public float slowDuration = 0.5f;
    public void Force(Slime slime)
    {
        BeamSlime beam = slime as BeamSlime;
        beam.slowOnLine = true;
        beam.slowDuration = Mathf.Max(beam.slowDuration, slowDuration);
    }
}

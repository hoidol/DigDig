using UnityEngine;

public class ThunderEnhance8Ability : SlimeEnhanceAbility
{
    //[각성]낙뢰에 맞은 적 10% 확률로 1초간 슬로우 (ThunderSlime.GetBullet에서 확인, 맞은 적마다 판정)
    public float slowChance = 0.1f; // 0~1
    public float slowDuration = 1f;
}

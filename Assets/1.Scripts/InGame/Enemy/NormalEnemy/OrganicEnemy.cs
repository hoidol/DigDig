using UnityEngine;

// 생체형 - 물리 +30% 마법 -30% 진동 0%, 버프 무시
// 피해 배율은 Enemy.TakeDamage에서 DefenceTable(Resources/DefenceTable)로 적용
public class OrganicEnemy : NormalEnemy
{
    public override void ApplyStatusEffect(StatusEffect effect)
    {
        if (!effect.isBuff)
            return;
        base.ApplyStatusEffect(effect);
    }
}

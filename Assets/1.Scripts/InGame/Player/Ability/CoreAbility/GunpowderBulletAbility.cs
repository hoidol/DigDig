using UnityEngine;

// 화약탄 - 확률로 발사된 총알에 폭발 효과 추가
public class GunpowderBulletAbility : Ability, IBulletItem
{
    static readonly float[] probs  = { 0.10f, 0.15f, 0.20f, 0.25f, 0.35f };
    static readonly float[] radii  = { 1.4f,  1.7f,  2.0f,  2.3f,  2.7f  };
    static readonly float[] ratios = { 0.4f,  0.6f,  0.8f,  1.0f,  1.2f  };

    public override void OnEquip(Player player) { }
    public override void OnUnequip(Player player) { }

    public void OnBulletFired(PlayerBullet bullet)
    {
        if (Random.value > probs[count - 1]) return;
        float dmg = Player.Instance.statMgr.AttackPower * ratios[count - 1];
        bullet.AddBehavior(new ExplosionBehaviour(radii[count - 1], dmg, LayerMask.GetMask("Hittable")));
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{probs[c - 1] * 100:0}% 확률로 폭발탄 발사";
    }
}

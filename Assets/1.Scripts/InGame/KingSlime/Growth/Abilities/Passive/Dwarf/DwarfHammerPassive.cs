using UnityEngine;

// 드워프 · 공격형 · 대지의 망치: 킹 attackCount번째 공격마다 탄이 맞은 자리에 범위 폭발 (킹 공격력 × damageMuls)
// 중장갑(Armored) 적에게는 ×armoredMul. 폭발은 킹 공격 속성, 방어 타입 배율표도 그대로 적용
public class DwarfHammerPassive : PassiveKingAbility
{
    public int attackCount = 4;
    public float[] damageMuls = { 1.5f, 2.25f, 3f };
    public float[] radiuses = { 1.2f, 1.4f, 1.6f };
    public float armoredMul = 2f;

    int count;
    bool armed; // 이번 공격 탄에 망치를 붙임
    class Shot { public bool exploded; }
    Shot currentShot; // 이번 공격 (다중 타겟으로 탄이 여러 발이어도 폭발은 한 번)

    public override object[] GetDescArgs(int level) =>
        new object[] { attackCount, Percent(LV(damageMuls, level)), FormatNumber(armoredMul) };

    protected override void Apply() { }

    public override void OnKingFired(AllyBulletObject bullet)
    {
        if (!armed)
            return;
        if (currentShot == null)
            currentShot = new Shot();
        Shot shot = currentShot;
        bullet.AddBehavior(new KingHitBehavior((b, enemy) =>
        {
            if (shot.exploded)
                return;
            shot.exploded = true;
            Explode(enemy.transform.position);
        }));
    }

    public override void OnKingAttack()
    {
        count++;
        currentShot = null;
        armed = count % attackCount == attackCount - 1; // 다음 공격이 attackCount번째
    }

    void Explode(Vector2 pos)
    {
        if (EffectManager.Instance != null)
            EffectManager.Instance.Play(EffectType.MortarExplosion, pos);
        if (EnemyFieldController.Instance == null || king.statMgr == null)
            return;
        float radius = LV(radiuses);
        float damage = king.statMgr.AttackPower * LV(damageMuls);
        var targets = new System.Collections.Generic.List<Enemy>();
        foreach (Enemy e in EnemyFieldController.Instance.activeEnemies)
            if (e != null && e.CurHp > 0 && Vector2.Distance(e.transform.position, pos) <= radius)
                targets.Add(e);
        foreach (Enemy e in targets) // 맞아 죽으면 activeEnemies가 바뀌므로 복사본으로
        {
            var data = new AllyUnitDamageData();
            data.Init(king);
            data.attackType = king.AttackType;
            data.damage = e.DefenceType == DefenceType.Armored ? damage * armoredMul : damage;
            e.TakeDamage(data);
        }
    }
}

using UnityEngine;

// (2026-11-20) 킹 성장 능력 공용 부모. 레벨업 카드로 얻고, 같은 카드를 다시 고르면 level +1 (최대 AbilityData.maxLevel)
// KingAbilityInventory가 prefab을 킹 아래에 생성해서 들고 있음 (한 판 동안)
public abstract class KingAbility : Ability
{
    public AbilityData data;
    public int level; // 1부터
    protected KingSlime king;

    public void Init(KingSlime king, AbilityData data)
    {
        this.king = king;
        this.data = data;
        key = data.key;
        level = 1;
    }

    public override string GetDescription() => data != null ? data.GetDescription(level) : "";

    // 레벨이 오른 직후 (OnEquip 다음에도 한 번 호출 안 함 → 처음 효과는 OnEquip에서)
    public virtual void OnLevelChanged() { }

    // 설명 {0}에 넣을 수치 (그 레벨 기준)
    public virtual string GetValueText(int level) => "";
    // 설명 {0}, {1} … 에 넣을 값. 기본 = {0} 하나(GetValueText). 수치가 여러 개인 능력(패시브)은 재정의
    public virtual object[] GetDescArgs(int level) => new object[] { GetValueText(level) };

    // 킹이 탄을 쏠 때마다 (Weapon.Shoot, 메인·다중 타겟 탄. 연발로 더 쏜 탄은 제외)
    public virtual void OnKingFired(AllyBulletObject bullet) { }

    // 킹 공격 1회가 끝날 때마다 (Weapon.Attack, 다중 타겟·연속탄이어도 1회). 공격 횟수를 세는 능력용
    public virtual void OnKingAttack() { }

    public static string FormatNumber(float v) => v.ToString("0.##");
}

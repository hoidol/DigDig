using System;
using DG.Tweening;
using UnityEngine;

// 타일 위에 설치되는 아이템. 획득할 때마다 객체가 하나씩 새로 생겨 랜덤 타일에 설치된다.
// 수치는 타일(객체)마다 고정 → count로 강해지지 않음
public abstract class TileItem : Item, ITileItem
{
    public Tile tile; // 장착 전에 지정되어 있으면 그 타일에 설치, 없으면 랜덤 타일

    // ---- 강화 (타일 객체마다, 주사위 코인). 레벨 1 시작, 최대 = ItemData.MaxLevel
    public int upgradeLevel = 1;
    public static readonly float[] UPGRADE_CHANCES = { 70f, 50f }; // Lv1→2, Lv2→3 (이후는 마지막 값)
    public static int GetUpgradeCost(int baseDiceCoin, int curLevel) => baseDiceCoin * (curLevel + 1);
    public static float GetUpgradeChance(int curLevel) => UPGRADE_CHANCES[Mathf.Clamp(curLevel - 1, 0, UPGRADE_CHANCES.Length - 1)];

    public bool CanUpgrade => itemData != null && itemData.CanUpgradeItem;
    public bool IsMaxLevel => itemData == null || upgradeLevel >= itemData.MaxLevel;
    public int UpgradeCost => GetUpgradeCost(itemData.diceCoin, upgradeLevel);
    public float UpgradeChance => GetUpgradeChance(upgradeLevel);

    // 레벨 수치(ItemData.csv Values)를 이 아이템의 대표 수치에 넣는다. 수치가 없으면 프리팹 값 그대로
    protected virtual void ApplyLevelValue(float value) { }

    // 설명의 {0}에 들어갈 표시 값 (기본 = 수치 그대로). 배율형은 % 증가량으로 바꿔서 보여준다
    protected virtual string FormatLevelValue(float value) => FormatNumber(value);
    protected static string FormatNumber(float v) => v.ToString("0.##");
    protected static string FormatMulPercent(float mul) => FormatNumber((mul - 1f) * 100f); // 1.5 → "50"

    // ItemData.csv Desc의 {0} = 현재 레벨 수치 (프리팹이면 Lv1). 수치가 없으면 Desc 그대로
    public override string GetDescription() => GetDescription(itemData);

    public string GetDescription(ItemData data)
    {
        if (data == null)
            return "";
        if (!data.HasLevelValues || !data.desc.Contains("{0}"))
            return data.desc;
        return string.Format(data.desc, FormatLevelValue(data.GetLevelValue(upgradeLevel)));
    }

    void ApplyLevel()
    {
        ItemData data = itemData;
        if (data != null && data.HasLevelValues)
            ApplyLevelValue(data.GetLevelValue(upgradeLevel));
    }

    // 성공 판정만 (주사위 코인은 호출 측에서 먼저 냄). 올라간 유닛이 있으면 효과를 뺐다가 새 수치로 다시 건다
    public bool TryUpgrade(float roll)
    {
        if (!CanUpgrade || IsMaxLevel)
            return false;
        if (roll >= UpgradeChance)
            return false;
        ITileObject obj = tile != null ? tile.tileObject : null;
        if (obj != null)
            OnExit(obj);
        upgradeLevel++;
        ApplyLevel();
        if (obj != null)
            OnEnter(obj);
        return true;
    }

    public override void OnEquip()
    {
        if (tile == null)
            tile = TileManager.Instance.GetRandomTileForItem();
        if (tile == null)
        {
            Debug.LogWarning($"TileItem {key} 설치할 타일 없음");
            return;
        }
        ApplyLevel(); // 설치(OnEnter) 전에 레벨 수치 반영
        tile.AddTileItem(this);
        transform.SetParent(tile.transform, false); // 하이어라키에서 설치된 타일 아래에 둠 (타일 위치 기준)
        transform.localScale = Vector3.one;

        // 캐릭터 위치에서 타일로 점프해서 날아감
        transform.DOKill();
        transform.position = KingSlime.Instance.transform.position;
        transform.DOJump(tile.transform.position, JUMP_POWER, 1, JUMP_DURATION).SetEase(Ease.OutQuad);
    }
    const float JUMP_POWER = 1.5f;
    const float JUMP_DURATION = 0.5f;

    public override void OnUnequip()
    {
        transform.DOKill();
        if (tile != null)
            tile.RemoveTileItem(this);
        tile = null;
    }

    // 뽑기 후보로 나올 수 있는지 (ItemManager.GetDrawTileItems)
    public virtual bool CanDraw() => true;

    // 보유 상한(ItemData.maxCount)을 무시하고 CanDraw로만 판단 (진화 타일)
    public virtual bool IgnoreMaxCount => false;

    // null = 일반 후보 (랜덤으로 3개 안에 들 수도 있음). 값이 있으면 뽑기마다 그 확률(%)로 선택지에 확정 등장, 실패하면 후보에서 빠짐
    public virtual float? DrawChance => null;

    // 획득할 때 설치할 타일을 골라야 하는지 (SelectItemCanvas)
    public virtual bool NeedTile => true;

    // 타일 선택 단계에서 이 타일을 고를 수 있는지 / 못 고를 때 토스트 key
    public virtual bool CanPickTile(Tile tile) => tile.CanAddTileItem;
    public virtual string PickFailMessageKey(Tile tile) => tile.isBlocked ? "TileItem_BlockedTile" : "TileItem_AlreadyInstalled";

    // 위에 올라간 유닛이 탄을 쏠 때마다 호출 (Minion.FireBullet, Weapon.Shoot). 발사 직후라 데미지 데이터 초기화가 끝난 상태
    public virtual void OnFired(AllyBulletObject bullet)
    {
        ModifyDamageData(bullet.DamageData);
        IBulletBehavior hitBehavior = HitBehavior; // 확률형은 읽을 때 판정하므로 한 번만 읽음
        if (hitBehavior != null)
            bullet.AddBehavior(hitBehavior);
    }

    // 데미지 데이터 수정 (치명타, 만능 공격). 탄이 없는 Orbit은 적중마다 직접 호출
    public virtual void ModifyDamageData(AllyUnitDamageData data)
    {

    }

    // 적중 후 효과 (둔화, 스턴, 처치 코인). 상태가 없으므로 타일 객체마다 하나를 만들어 모든 탄이 공유
    // 확률형(둔화)은 읽을 때마다 판정해서 null일 수 있음 → 호출 측은 한 번만 읽을 것
    public virtual IBulletBehavior HitBehavior => null;

    public virtual void OnEnter(ITileObject tileObject)
    {

    }
    public virtual void OnExit(ITileObject tileObject)
    {

    }

}

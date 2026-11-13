using System.Collections.Generic;
using UnityEngine;

// [S] 성장: 필드(타일 위)에서 growTime초가 지나면 장착 슬라임 중 랜덤 종(성장 자신 제외)으로 성장, 머지 레벨 +growLevelUp (최대치까지)
// - 시간은 게임 진행 중에만 흐름. 드래그 중에 시간이 다 되면 놓은 뒤 성장
// - 옮겨도 시간은 유지, 합성하면 사라짐 (일반 합성 규칙)
// - 성장 전까지는 일반 탄 공격
// 프리팹: growBar(선택)에 진행도 표시용 Transform 연결 (localScale.x = 진행도 0~1)
public class GrowUpMinion : MinionGrowth1
{
    // 최대 머지 레벨에서는 쓸 수 있는 각성이 없어 진화하지 않음 (머지 레벨을 올리는 슬라임)
    public override bool CanEvolve => false;

    public Transform growBar;

    GrowUpMinionData growUpMinionData;
    AllyBulletSpec allyBulletSpec;

    public float growTime;
    public int growLevelUp;

    float growTimer;
    bool isDragging;
    bool isGrown;

    public override void Spawn(Vector2 pos, int lv)
    {
        growUpMinionData = MinionManager.Instance.GetMinionData(key) as GrowUpMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        growTime = growUpMinionData.growTime;
        growLevelUp = growUpMinionData.growLevelUp;
        growTimer = 0f;
        isDragging = false;
        isGrown = false;
        base.InitMinion();
        UpdateGrowBar();
    }

    public override void Update()
    {
        base.Update();
        UpdateGrow();
    }

    void UpdateGrow()
    {
        if (isGrown || GameManager.Instance == null || !GameManager.Instance.isPlaying)
            return;
        if (TileManager.Instance == null || TileManager.Instance.GetTile(this) == null)
            return;

        growTimer += Time.deltaTime;
        UpdateGrowBar();
        if (growTimer >= growTime && !isDragging)
            Grow();
    }

    void UpdateGrowBar()
    {
        if (growBar == null)
            return;
        Vector3 scale = growBar.localScale;
        scale.x = growTime > 0f ? Mathf.Clamp01(growTimer / growTime) : 1f;
        growBar.localScale = scale;
    }

    void Grow()
    {
        string growKey = PickGrowKey();
        if (string.IsNullOrEmpty(growKey))
            return;

        isGrown = true;
        int lv = Mathf.Min(mergeLevel + growLevelUp, MinionData.MAX_MERGE_LEVEL);
        MinionFieldController.Instance.ReplaceMinion(this, growKey, lv);
    }

    // 장착 슬라임 중 랜덤 (성장 자신 제외, 장착이 성장뿐이면 성장)
    string PickGrowKey()
    {
        UserMinion[] equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
        List<string> keys = new List<string>();
        foreach (UserMinion userMinion in equipped)
        {
            if (userMinion != null && !string.IsNullOrEmpty(userMinion.key) && userMinion.key != key)
                keys.Add(userMinion.key);
        }
        if (keys.Count == 0)
            return key;
        return keys[Random.Range(0, keys.Count)];
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    public override void StartDrag()
    {
        base.StartDrag();
        isDragging = true;
    }

    public override void EndDrag(Tile t)
    {
        base.EndDrag(t);
        isDragging = false;
    }
}

using System.Collections.Generic;
using UnityEngine;

// [S] 성장: 필드(타일 위)에서 growTime초가 지나면 장착 슬라임 중 랜덤 종(성장 자신 제외)으로 성장, 머지 레벨 +growLevelUp (최대치까지)
// - 시간은 게임 진행 중에만 흐름. 드래그 중에 시간이 다 되면 놓은 뒤 성장
// - 옮겨도 시간은 유지, 합성하면 사라짐 (일반 합성 규칙)
// - 성장 전까지는 일반 탄 공격
// - 강화: 5 = 20초, 8 = [각성] 성장 시 50% 확률로 머지 레벨 +1 추가, 11 = 15초
// 프리팹: growBar(선택)에 진행도 표시용 Transform 연결 (localScale.x = 진행도 0~1)
public class GrowUpSlime : SlimeGrowth1
{
    public Transform growBar;

    GrowUpSlimeData growUpSlimeData;
    AllyBulletSpec allyBulletSpec;

    // 강화 능력이 바꿀 수 있음 (InitSlime에서 base 호출 전에 기본값 설정)
    public float growTime;
    public int growLevelUp;
    public float extraLevelChance; // 0~1, 성장 시 머지 레벨 +1 추가 확률 ([각성])

    float growTimer;
    bool isDragging;
    bool isGrown;

    public override void Spawn(Vector2 pos, int lv)
    {
        growUpSlimeData = SlimeManager.Instance.GetSlimeData(key) as GrowUpSlimeData;
        base.Spawn(pos, lv);
    }

    public override void InitSlime()
    {
        allyBulletSpec = new AllyBulletSpec();
        growTime = growUpSlimeData.growTime;
        growLevelUp = growUpSlimeData.growLevelUp;
        extraLevelChance = 0f;
        growTimer = 0f;
        isDragging = false;
        isGrown = false;
        base.InitSlime();
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
        int lv = mergeLevel + growLevelUp;
        if (SlimeChancePity.RollInGame(key, extraLevelChance))
            lv++;
        lv = Mathf.Min(lv, SlimeData.MAX_MERGE_LEVEL);
        SlimeFieldController.Instance.ReplaceSlime(this, growKey, lv);
    }

    // 장착 슬라임 중 랜덤 (성장 자신 제외, 장착이 성장뿐이면 성장)
    string PickGrowKey()
    {
        UserSlime[] equipped = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes;
        List<string> keys = new List<string>();
        foreach (UserSlime userSlime in equipped)
        {
            if (userSlime != null && !string.IsNullOrEmpty(userSlime.key) && userSlime.key != key)
                keys.Add(userSlime.key);
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

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlimeFieldController : MonoSingleton<SlimeFieldController>
{
    readonly Dictionary<string, Stack<Slime>> pool = new(); // 적 종류 별 풀링

    // [field: SerializeField] public int ActiveSlimeCount { get; private set; }

    public readonly HashSet<Slime> activeslimes = new();
    public int slimeMaxCount;
    public int CountedSlimeCount => KingSlime.Instance.slimeInventory.curSlimes.Count; // 최대 수 판정에 쓰는 슬라임 수
    public int mergeCount;
    public int reinforceCount;
    // public int mergeCoinGetCounter;

    public Slime Instantiate(string key)
    {
        SlimeData data = SlimeManager.Instance.slimeDataDic[key];

        if (!pool.ContainsKey(data.prefab.key))
            pool[data.prefab.key] = new Stack<Slime>();

        Slime slime = pool[data.prefab.key].Count > 0
            ? pool[data.prefab.key].Pop()
            : Instantiate(data.prefab);


        slime.gameObject.SetActive(true);
        activeslimes.Add(slime);
        // ActiveSlimeCount++;
        return slime;
    }

    public Slime LevelUp(Slime s)
    {
        string key = s.key;
        int level = s.mergeLevel;
        KingSlime.Instance.RemoveSlime(s);
        Slime slime = KingSlime.Instance.AddSlime(key, level + 1);
        return slime;
    }

    // public void Release(Slime slime)
    // {
    //     if (!pool.ContainsKey(slime.key))
    //         pool[slime.key] = new Stack<Slime>();

    //     activeslimes.Remove(slime);
    //     if (TileManager.Instance != null)
    //         TileManager.Instance.RemoveTileObject(slime);
    //     slime.gameObject.SetActive(false);
    //     pool[slime.key].Push(slime);
    //     // ActiveSlimeCount = Mathf.Max(0, ActiveSlimeCount - 1);
    // }

    public async UniTask<(bool, string, int)> Merge(Slime slime1, Slime slime2)
    {
        // 희생 슬라임은 끌어온 쪽이든 놓인 쪽이든 희생 규칙(상대 종으로 +1)으로 합성
        if (slime2 is SacrificeSlime && slime1 is not SacrificeSlime)
            return await slime2.Merge(slime1);
        return await slime1.Merge(slime2);
    }

    // 필드의 슬라임 제거 (조커 변신 실패 등). 선택 중이던 타일이면 선택 해제
    public void RemoveFieldSlime(Slime slime)
    {
        Tile tile = TileManager.Instance.GetTile(slime);
        bool wasSelected = tile != null && tile == selectedTile;
        if (ReferenceEquals(dragObject, slime))
            dragObject = null;

        TileManager.Instance.RemoveTileObject(slime);
        KingSlime.Instance.RemoveSlime(slime);
        if (wasSelected)
            SelectTile(null);
        KingSlimeManageCanvas.Instance.UpdateCanvas();
    }

    // 필드의 슬라임을 다른 슬라임으로 교체 (같은 타일). 성장·조커 변신 등
    // 선택 중이던 타일이면 새 슬라임이 선택됨
    public Slime ReplaceSlime(Slime old, string key, int lv)
    {
        Tile tile = TileManager.Instance.GetTile(old);
        bool wasSelected = tile != null && tile == selectedTile;
        if (ReferenceEquals(dragObject, old))
            dragObject = null;

        TileManager.Instance.RemoveTileObject(old);
        KingSlime.Instance.RemoveSlime(old);
        Slime slime = KingSlime.Instance.AddSlime(key, lv);
        if (tile != null)
            tile.SetTileObject(slime);
        if (wasSelected)
            SelectTile(tile);
        KingSlimeManageCanvas.Instance.UpdateCanvas();
        return slime;
    }

    public LayerMask slimeLayer;
    public LayerMask characterLayer;
    // public float tileSnapDistance = 1f; // 드롭 위치에서 이 거리 이내의 타일로 이동

    // public bool IsMerging { get; private set; }

    ITileObject dragObject; // 드래그 중인 타일 오브젝트 (슬라임 or 캐릭터)
    Vector3 originPos;
    Vector3 offset;

    void Awake()
    {
        // 로비 업그레이드·장비는 GameManager에서 킹 스탯(MaxSlimeCount)으로 더함 (여기서 또 더하면 두 번 적용됨)
        slimeMaxCount = GameSetting.SLIME_MIN_EXIST_COUNT;

        if (slimeLayer == 0)
            slimeLayer = LayerMask.GetMask("Slime");
        if (characterLayer == 0)
            characterLayer = LayerMask.GetMask("Character");

        GameEventBus.Subscribe<TileObjectChangedEvent>(e => RefreshSelectedTile(e.tile));
        GameEventBus.Subscribe<TileItemChangedEvent>(e => RefreshSelectedTile(e.tile));
    }

    public System.Action<Tile> tilePickCallback; // 있으면 타일 선택 모드: 드래그/선택 대신 누른 타일을 전달 (SelectItemCanvas)

    void Update()
    {
        if (tilePickCallback != null)
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                Tile tile = TileManager.Instance.GetNearestTile(GetMouseWorldPos());
                if (tile != null)
                    tilePickCallback(tile);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // UI 위를 클릭한 경우 드래그 시작하지 않음
            if (IsPointerOverUI())
                return;

            if (TryBeginHandleDrag())
                return;
            BeginDrag();
        }
        else if (dragHandle != null && Input.GetMouseButton(0))
        {
            dragHandle.DragTo(GetMouseWorldPos() + handleOffset);
        }
        else if (dragHandle != null && Input.GetMouseButtonUp(0))
        {
            dragHandle.EndHandleDrag();
            dragHandle = null;
        }
        else if (dragObject != null && Input.GetMouseButton(0))
        {
            dragObject.Transform.position = GetMouseWorldPos() + offset;
        }
        else if (dragObject != null && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

    }



    IAllyUnit targetUnit; // 현재 선택된 아군 (슬라임 or 캐릭터)
    Tile selectedTile; // 정보를 보여주는 타일 (빈 타일 포함). 위에 유닛이 있으면 그 유닛이 targetUnit
    // 클릭한 타일을 선택하고, 위에 슬라임 or 캐릭터가 있으면 드래그 시작
    void BeginDrag()
    {
        Vector3 mousePos = GetMouseWorldPos();
        ITileObject tileObject = FindSlime(mousePos, null);
        if (tileObject == null && IsCharacterAt(mousePos))
            tileObject = KingSlime.Instance;

        // 유닛을 눌렀으면 유닛이 서 있는 타일, 아니면 누른 위치의 타일 (타일 밖이면 null → 선택 해제)
        Tile tile = tileObject != null ? TileManager.Instance.GetTile(tileObject) : null;
        SelectTile(tile ?? TileManager.Instance.GetNearestTile(mousePos));

        if (tileObject == null)
            return;

        tileObject.StartDrag();
        GameEventBus.Publish(new DragStartEvent(tileObject));
        dragObject = tileObject;
        originPos = tileObject.Transform.position;
        offset = originPos - mousePos;
        offset.z = 0;
    }

    DragHandle dragHandle; // 드래그 중인 손잡이 (폭격 슬라임 깃발 등)
    Vector3 handleOffset;

    // 손잡이 드래그: 선택된 유닛의 손잡이는 유닛보다 우선, 다른 손잡이는 유닛이 없는 자리에서만 잡힘
    // 잡으면 손잡이 주인 유닛의 타일을 선택 (범위 표시)
    bool TryBeginHandleDrag()
    {
        Vector3 mousePos = GetMouseWorldPos();
        DragHandle handle = DragHandle.FindAt(mousePos, targetUnit);
        if (handle == null || handle.Owner == null)
            return false;
        if (handle.Owner != targetUnit && (FindSlime(mousePos, null) != null || IsCharacterAt(mousePos)))
            return false;

        if (handle.Owner != targetUnit && handle.Owner is ITileObject ownerObject)
            SelectTile(TileManager.Instance.GetTile(ownerObject));

        dragHandle = handle;
        handleOffset = handle.transform.position - mousePos;
        handleOffset.z = 0;
        handle.BeginHandleDrag();
        return true;
    }

    // 슬라임끼리 놓으면 머지 시도(머지가 안 되면 두 슬라임 자리 교체), 킹 ↔ 슬라임은 자리 교체, 빈 타일이면 이동, 그 외엔 원위치
    void EndDrag()
    {
        ITileObject source = dragObject;
        dragObject = null;

        // 드래그 중 오브젝트가 사라진 경우
        if (source == null || source.Transform == null || !source.Transform.gameObject.activeInHierarchy)
        {
            if (source != null)
                GameEventBus.Publish(new DragEndEvent(source, null));
            SelectTile(null);
            return;
        }

        DropAt(source, GetMouseWorldPos(), originPos);
    }

    // 드래그한 오브젝트를 dropPos에 놓음 (테스트에서 마우스 없이 호출)
    void DropAt(ITileObject source, Vector3 dropPos, Vector3 originPos)
    {
        if (source is Slime sourceSlime)
        {
            Slime target = FindSlime(dropPos, sourceSlime);
            if (target != null)
            {
                source.EndDrag(null);
                GameEventBus.Publish(new DragEndEvent(source, null));
                Merge(sourceSlime, target, originPos).Forget();
                return;
            }
        }

        Tile tile = TileManager.Instance.GetNearestTile(dropPos);
        // 킹 ↔ 슬라임: 머지 대상이 아니므로 자리 교체
        if (tile != null && tile.tileObject != null && tile.tileObject != source
            && (source is KingSlime || tile.tileObject is KingSlime)
            && SwapTileObjects(source, tile.tileObject))
        {
            GameEventBus.Publish(new DragEndEvent(source, tile));
            SelectTile(TileManager.Instance.GetTile(source));
            return;
        }

        if (tile != null && tile.IsEmpty)
        {
            TileManager.Instance.RemoveTileObject(source);
            tile.SetTileObject(source);
        }
        else
        {
            source.Transform.position = originPos;
        }
        source.EndDrag(tile);
        GameEventBus.Publish(new DragEndEvent(source, tile));
        SelectTile(TileManager.Instance.GetTile(source)); // 선택은 옮긴 유닛을 따라감 (원위치면 원래 타일)
    }

    Slime FindSlime(Vector2 pos, Slime exclude)
    {
        Slime closest = null;
        float minDist = float.MaxValue;
        foreach (Collider2D col in Physics2D.OverlapPointAll(pos, slimeLayer))
        {
            Slime slime = col.GetComponent<Slime>();
            if (slime == null || slime == exclude)
                continue;

            float dist = ((Vector2)slime.transform.position - pos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = slime;
            }
        }
        return closest;
    }

    // 두 슬라임의 타일을 맞바꾼다. 타일 아이템 효과는 SetTileObject가 OnExit/OnEnter로 옮김
    // 둘 다 옮겨 놓은 것과 같이 EndDrag(새 타일) 처리 → 지옥불 위치 등 타일 기준 상태 갱신
    public static bool SwapSlimes(Slime a, Slime b) => SwapTileObjects(a, b);

    // 두 타일 오브젝트(슬라임·킹)의 타일을 맞바꾼다. 킹 ↔ 슬라임 교체에도 사용
    public static bool SwapTileObjects(ITileObject a, ITileObject b)
    {
        if (a == null || b == null || a == b)
            return false;
        Tile tileA = TileManager.Instance.GetTile(a);
        Tile tileB = TileManager.Instance.GetTile(b);
        if (tileA == null || tileB == null)
            return false;

        tileA.Clear();
        tileB.Clear();
        tileA.SetTileObject(b);
        tileB.SetTileObject(a);
        a.EndDrag(tileB);
        b.EndDrag(tileA);
        return true;
    }

    async UniTaskVoid Merge(Slime source, Slime target, Vector3 sourceOriginPos)
    {

        var (result, mergeResultKey, lv) = await Merge(source, target);
        if (!result)
        {
            // 머지가 안 되는 슬라임끼리면 자리 교체 (둘 중 하나라도 타일 밖이면 원위치)
            if (!SwapSlimes(source, target))
                source.transform.position = sourceOriginPos;
            SelectTile(TileManager.Instance.GetTile(source)); // 선택은 옮긴 슬라임을 따라감
            return;
        }

        // 머지 결과는 타겟 슬라임이 있던 타일에 배치
        Tile tile = TileManager.Instance.GetTile(target);
        TileManager.Instance.RemoveTileObject(source);
        TileManager.Instance.RemoveTileObject(target);

        KingSlime.Instance.RemoveSlime(source);
        KingSlime.Instance.RemoveSlime(target);
        Slime merged = KingSlime.Instance.AddSlime(mergeResultKey, lv);
        if (tile != null)
            tile.SetTileObject(merged);
        mergeCount++;
        // Character.Instance.AddMergeCoin(1);
        GameEventBus.Publish(new MergedSlimeEvent());
        SelectTile(tile); // 합성 결과 슬라임 선택
        KingSlimeManageCanvas.Instance.UpdateCanvas();
    }

    bool IsCharacterAt(Vector2 pos)
    {
        if (KingSlime.Instance == null)
            return false;

        foreach (Collider2D col in Physics2D.OverlapPointAll(pos, characterLayer))
        {
            if (col.GetComponentInParent<KingSlime>() == KingSlime.Instance)
                return true;
        }
        return false;
    }

    // 타일 선택 (null이면 해제). 위에 아군 유닛이 있으면 그 유닛도 타겟으로 지정
    // 같은 타일을 다시 선택해도 이벤트를 다시 보내 정보 UI가 갱신되게 함
    void SelectTile(Tile tile)
    {
        selectedTile = tile;
        if (tile == null)
        {
            ReleaseTarget();
            GameEventBus.Publish(new TileDeselectedEvent());
            return;
        }

        if (tile.tileObject is IAllyUnit unit)
            SetTarget(unit);
        else
            ReleaseTarget();
        GameEventBus.Publish(new TileSelectedEvent(tile));
    }

    public void DeselectTile()
    {
        SelectTile(null);
    }

    // 선택된 타일의 내용이 바뀌면 (빈 타일에 소환, 아이템 설치 등) 정보 갱신
    void RefreshSelectedTile(Tile tile)
    {
        if (tile != null && tile == selectedTile)
            SelectTile(tile);
    }

    void SetTarget(IAllyUnit unit)
    {
        if (targetUnit == unit)
            return;

        ReleaseTarget();
        targetUnit = unit;
        // AllySettingPanel.Instance.SetSlime(targetUnit);
        targetUnit.StartTarget();
        GameEventBus.Publish(new SetTargetEvent(targetUnit));
    }

    void ReleaseTarget()
    {
        if (targetUnit == null)
            return;

        IAllyUnit releasedUnit = targetUnit;
        targetUnit.EndTarget();
        targetUnit = null;
        // AllySettingPanel.Instance.SetSlime(null);
        GameEventBus.Publish(new ReleaseTargetEvent(releasedUnit));
    }


    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // IsPointerOverGameObject(fingerId)는 터치 시작 프레임에 EventSystem이 아직 갱신 전이라 false가 나옴 (모바일)
        // → UI를 눌러도 드래그/선택 해제가 먼저 돌아 패널이 꺼지고 버튼 onClick이 안 불림
        // 그래서 현재 포인터 위치로 직접 UI 레이캐스트
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults.Count > 0;
    }

    readonly List<RaycastResult> raycastResults = new();

    string GetHierarchyPath(Transform tr)
    {
        string path = tr.name;
        while (tr.parent != null)
        {
            tr = tr.parent;
            path = tr.name + "/" + path;
        }
        return path;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }


}

// 슬라임 or 캐릭터 드래그 시작
public class DragStartEvent
{
    public ITileObject tileObject;
    public DragStartEvent(ITileObject tileObject) { this.tileObject = tileObject; }
}

// 슬라임 or 캐릭터 드래그 종료
public class DragEndEvent
{
    public ITileObject tileObject;
    public Tile tile; // 놓은 위치에서 가장 가까운 타일. 머지했거나 드래그 중 사라졌으면 null
    public DragEndEvent(ITileObject tileObject, Tile tile) { this.tileObject = tileObject; this.tile = tile; }
}

// 슬라임 or 캐릭터 선택됨
public class SetTargetEvent
{
    public IAllyUnit unit;
    public SetTargetEvent(IAllyUnit unit) { this.unit = unit; }
}

// 슬라임 or 캐릭터 선택 해제됨
public class ReleaseTargetEvent
{
    public IAllyUnit unit; // 해제된 대상
    public ReleaseTargetEvent(IAllyUnit unit) { this.unit = unit; }
}

public class MergedSlimeEvent
{
}

// 타일 선택됨 (빈 타일 포함). 위 유닛은 tile.tileObject, 아이템은 tile.tileItem
public class TileSelectedEvent
{
    public Tile tile;
    public TileSelectedEvent(Tile tile) { this.tile = tile; }
}

// 타일 선택 해제됨
public class TileDeselectedEvent
{
}

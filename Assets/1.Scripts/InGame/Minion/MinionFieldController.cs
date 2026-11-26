using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinionFieldController : MonoSingleton<MinionFieldController>
{
    readonly Dictionary<string, Stack<Minion>> pool = new(); // 적 종류 별 풀링

    // [field: SerializeField] public int ActiveMinionCount { get; private set; }

    public readonly HashSet<Minion> activeminions = new();
    // (2026-11-25) 값이 바뀌면 MinionMaxCountChangedEvent → 최대 미니언 수 UI 갱신 (용사 2단계 해방·아이템 등)
    int _minionMaxCount;
    public int minionMaxCount
    {
        get => _minionMaxCount;
        set
        {
            if (_minionMaxCount == value)
                return;
            _minionMaxCount = value;
            GameEventBus.Publish(new MinionMaxCountChangedEvent());
        }
    }
    public int CountedMinionCount => KingSlime.Instance.minionInventory.curMinions.Count; // 최대 수 판정에 쓰는 슬라임 수
    public int mergeCount;
    public int reinforceCount;
    // public int mergeCoinGetCounter;

    public Minion Instantiate(string key)
    {
        MinionData data = MinionManager.Instance.minionDataDic[key];

        if (!pool.ContainsKey(data.prefab.key))
            pool[data.prefab.key] = new Stack<Minion>();

        Minion minion = pool[data.prefab.key].Count > 0
            ? pool[data.prefab.key].Pop()
            : Instantiate(data.prefab);


        minion.gameObject.SetActive(true);
        activeminions.Add(minion);
        // ActiveMinionCount++;
        return minion;
    }

    public Minion LevelUp(Minion s)
    {
        string key = s.key;
        int level = s.mergeLevel;
        KingSlime.Instance.RemoveMinion(s);
        Minion minion = KingSlime.Instance.AddMinion(key, level + 1);
        return minion;
    }

    // public void Release(Minion minion)
    // {
    //     if (!pool.ContainsKey(minion.key))
    //         pool[minion.key] = new Stack<Minion>();

    //     activeminions.Remove(minion);
    //     if (TileManager.Instance != null)
    //         TileManager.Instance.RemoveTileObject(minion);
    //     minion.gameObject.SetActive(false);
    //     pool[minion.key].Push(minion);
    //     // ActiveMinionCount = Mathf.Max(0, ActiveMinionCount - 1);
    // }

    public async UniTask<(bool, string, int)> Merge(Minion minion1, Minion minion2)
    {

        // 희생 슬라임은 끌어온 쪽이든 놓인 쪽이든 희생 규칙(상대 종으로 +1)으로 합성
        if (minion2 is SacrificeMinion && minion1 is not SacrificeMinion)
            return await minion2.Merge(minion1);
        return await minion1.Merge(minion2);
    }

    // 필드의 슬라임 제거 (조커 변신 실패 등). 선택 중이던 타일이면 선택 해제
    public void RemoveFieldMinion(Minion minion)
    {
        Tile tile = TileManager.Instance.GetTile(minion);
        bool wasSelected = tile != null && tile == selectedTile;
        if (ReferenceEquals(dragObject, minion))
            dragObject = null;

        TileManager.Instance.RemoveTileObject(minion);
        KingSlime.Instance.RemoveMinion(minion);
        if (wasSelected)
            SelectTile(null);
        KingSlimeManageCanvas.Instance.UpdateCanvas();
    }

    // 필드의 슬라임을 다른 슬라임으로 교체 (같은 타일). 성장·조커 변신 등
    // 선택 중이던 타일이면 새 슬라임이 선택됨
    public Minion ReplaceMinion(Minion old, string key, int lv)
    {
        Tile tile = TileManager.Instance.GetTile(old);
        bool wasSelected = tile != null && tile == selectedTile;
        if (ReferenceEquals(dragObject, old))
            dragObject = null;

        TileManager.Instance.RemoveTileObject(old);
        KingSlime.Instance.RemoveMinion(old);
        Minion minion = KingSlime.Instance.AddMinion(key, lv);
        if (tile != null)
            tile.SetTileObject(minion);
        if (minion != null)
            minion.CopyUnitOptionsFrom(old); // 그 칸에 있던 미니언 설정 유지 (카드 합성·성장·변신)
        if (wasSelected)
            SelectTile(tile);
        KingSlimeManageCanvas.Instance.UpdateCanvas();
        return minion;
    }

    public LayerMask minionLayer;
    public LayerMask characterLayer;
    // public float tileSnapDistance = 1f; // 드롭 위치에서 이 거리 이내의 타일로 이동

    // public bool IsMerging { get; private set; }

    ITileObject dragObject; // 드래그 중인 타일 오브젝트 (슬라임 or 캐릭터)
    public bool IsDragging => dragObject != null || dragHandle != null; // 유닛·손잡이(깃발) 드래그 중
    Vector3 originPos;
    Vector3 offset;

    void Awake()
    {
        // 로비 업그레이드·장비는 GameManager에서 킹 스탯(MaxMinionCount)으로 더함 (여기서 또 더하면 두 번 적용됨)
        minionMaxCount = GameSetting.MINION_MIN_EXIST_COUNT;

        if (minionLayer == 0)
            minionLayer = LayerMask.GetMask("Minion");
        if (characterLayer == 0)
            characterLayer = LayerMask.GetMask("Character");

        GameEventBus.Subscribe<TileObjectChangedEvent>(e => RefreshSelectedTile(e.tile));
        GameEventBus.Subscribe<TileRuneChangedEvent>(e => RefreshSelectedTile(e.tile));
    }

    // 튜토리얼: 있으면 조건을 통과한 유닛만 드래그할 수 있고(그 외 터치는 선택도 안 함), 조건을 통과한 칸에만 놓을 수 있다
    public System.Func<ITileObject, bool> dragFilter;
    public System.Func<ITileObject, Tile, bool> dropFilter;

    // (2026-11-25) 있으면 위치 선택 모드: 필드를 누른 월드 위치를 한 번 전달하고 끝남 (용사 위치 지정 스킬, HeroSkillHud). 드래그·선택·카메라 이동 없음
    System.Action<Vector2> positionPickCallback;
    public bool IsPickingPosition => positionPickCallback != null;
    public void BeginPositionPick(System.Action<Vector2> callback)
    {
        panCandidate = false;
        panning = false;
        positionPickCallback = callback;
    }
    public void CancelPositionPick() => positionPickCallback = null;

    public System.Action<Tile> tilePickCallback; // 있으면 타일 선택 모드: 드래그/선택 대신 누른 타일을 전달 (SelectRuneCanvas)

    // 카메라 드래그 (빈 곳 드래그, 맵이 화면보다 클 때만 실제로 움직임: CameraManager)
    const float PAN_START_PIXELS = 15f; // 이만큼 움직여야 드래그로 봄 (짧은 터치 = 칸 선택)
    bool panCandidate;
    bool panning;
    Vector2 panStartScreen;
    Vector3 panStartCamera;
    public bool IsPanning => panning;

    void Update()
    {
        if (positionPickCallback != null)
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                var cb = positionPickCallback;
                positionPickCallback = null;
                cb(GetMouseWorldPos());
            }
            return;
        }
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

            if (dragFilter == null && TryBeginHandleDrag())
                return;
            BeginDrag();
            // (2026-11-24) 유닛·손잡이를 잡지 않았으면 화면 드래그 = 카메라 이동 후보 (튜토리얼 중엔 없음)
            if (dragObject == null && dragHandle == null && dragFilter == null && CameraManager.Instance != null)
            {
                panCandidate = true;
                panning = false;
                panStartScreen = Input.mousePosition;
                panStartCamera = CameraManager.Instance.CameraPosition;
            }
        }
        else if (panCandidate && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - panStartScreen;
            if (!panning && delta.magnitude >= PAN_START_PIXELS)
                panning = true;
            if (panning)
                CameraManager.Instance.PanTo(panStartCamera - (Vector3)(delta * CameraManager.Instance.WorldPerPixel));
        }
        else if (panCandidate && Input.GetMouseButtonUp(0))
        {
            panCandidate = false;
            panning = false;
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
    public Tile SelectedTile => selectedTile;
    Tile selectedTile; // 정보를 보여주는 타일 (빈 타일 포함). 위에 유닛이 있으면 그 유닛이 targetUnit
    // 클릭한 타일을 선택하고, 위에 슬라임 or 캐릭터가 있으면 드래그 시작
    void BeginDrag()
    {
        Vector3 mousePos = GetMouseWorldPos();
        ITileObject tileObject = FindMinion(mousePos, null);
        if (tileObject == null && IsCharacterAt(mousePos))
            tileObject = KingSlime.Instance;

        if (dragFilter != null && (tileObject == null || !dragFilter(tileObject)))
            return;

        // (2026-11-20) 유닛이 아닌 곳에서 일반 적을 누르면 wave 적 정보 다시 보기
        if (tileObject == null && dragFilter == null && IsNormalEnemyAt(mousePos) && WaveEnemyInfoPanel.Instance != null)
            WaveEnemyInfoPanel.Instance.ShowByEnemyClick();

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

    const float ENEMY_CLICK_RADIUS = 0.3f; // 적이 작고 움직여서 살짝 넓게 판정

    static bool IsNormalEnemyAt(Vector2 pos)
    {
        foreach (Collider2D col in Physics2D.OverlapCircleAll(pos, ENEMY_CLICK_RADIUS))
        {
            NormalEnemy enemy = col.GetComponentInParent<NormalEnemy>();
            if (enemy != null && enemy.gameObject.activeInHierarchy && !enemy.IsDying)
                return true;
        }
        return false;
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
        if (handle.Owner != targetUnit && (FindMinion(mousePos, null) != null || IsCharacterAt(mousePos)))
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
        Tile tile = TileManager.Instance.GetNearestTile(dropPos);
        bool canDrop = dropFilter == null || dropFilter(source, tile); // false면 원위치

        if (canDrop && source is Minion sourceMinion)
        {
            Minion target = FindMinion(dropPos, sourceMinion);
            if (target != null)
            {
                source.EndDrag(null);
                GameEventBus.Publish(new DragEndEvent(source, null));
                Merge(sourceMinion, target, originPos).Forget();
                return;
            }
        }

        // 킹 ↔ 슬라임: 머지 대상이 아니므로 자리 교체 (카드 소환 모드에선 미니언이 움직이므로 안 함)
        if (canDrop && !LockMinionPosition && tile != null && tile.tileObject != null && tile.tileObject != source
            && (source is KingSlime || tile.tileObject is KingSlime)
            && SwapTileObjects(source, tile.tileObject))
        {
            GameEventBus.Publish(new DragEndEvent(source, tile));
            SelectTile(TileManager.Instance.GetTile(source));
            return;
        }

        // 카드 소환 모드: 미니언은 빈 칸으로 이동 불가(머지만), 킹은 이동 가능
        if (canDrop && tile != null && tile.IsEmpty && !(LockMinionPosition && source is Minion))
        {
            TileManager.Instance.RemoveTileObject(source);
            tile.SetTileObject(source);
            SoundManager.Play(SFXType.DragDrop);
        }
        else
        {
            source.Transform.position = originPos;
        }
        source.EndDrag(tile);
        GameEventBus.Publish(new DragEndEvent(source, tile));
        SelectTile(TileManager.Instance.GetTile(source)); // 선택은 옮긴 유닛을 따라감 (원위치면 원래 타일)
    }

    // (2026-11-19) 카드 소환 모드: 한 번 놓은 미니언은 자리 고정. 드래그는 머지용으로만 (빈 칸 이동·자리 교체 없음)
    public static bool LockMinionPosition => GameSetting.USE_WAVE_MINION_OFFER;

    Minion FindMinion(Vector2 pos, Minion exclude)
    {
        Minion closest = null;
        float minDist = float.MaxValue;
        foreach (Collider2D col in Physics2D.OverlapPointAll(pos, minionLayer))
        {
            Minion minion = col.GetComponent<Minion>();
            if (minion == null || minion == exclude)
                continue;

            float dist = ((Vector2)minion.transform.position - pos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = minion;
            }
        }
        return closest;
    }

    // 두 슬라임의 타일을 맞바꾼다. 타일 아이템 효과는 SetTileObject가 OnExit/OnEnter로 옮김
    // 둘 다 옮겨 놓은 것과 같이 EndDrag(새 타일) 처리 → 지옥불 위치 등 타일 기준 상태 갱신
    public static bool SwapMinions(Minion a, Minion b) => SwapTileObjects(a, b);

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
        SoundManager.Play(SFXType.MinionSwap);
        return true;
    }

    async UniTaskVoid Merge(Minion source, Minion target, Vector3 sourceOriginPos)
    {

        var (result, mergeResultKey, lv) = await Merge(source, target);
        if (!result)
        {
            // 머지가 안 되는 슬라임끼리면 자리 교체 (둘 중 하나라도 타일 밖이면 원위치, 카드 소환 모드는 항상 원위치)
            if (LockMinionPosition || !SwapMinions(source, target))
                source.transform.position = sourceOriginPos;
            SelectTile(TileManager.Instance.GetTile(source)); // 선택은 옮긴 슬라임을 따라감
            return;
        }

        // 머지 결과는 타겟 슬라임이 있던 타일에 배치
        Tile tile = TileManager.Instance.GetTile(target);
        TileManager.Instance.RemoveTileObject(source);
        TileManager.Instance.RemoveTileObject(target);
        // 희생 합성(한쪽만 희생)은 전용 소리
        SoundManager.Play((source is SacrificeMinion) != (target is SacrificeMinion) ? SFXType.SacrificeMerge : SFXType.MinionMerge);

        KingSlime.Instance.RemoveMinion(source);
        KingSlime.Instance.RemoveMinion(target);
        Minion merged = KingSlime.Instance.AddMinion(mergeResultKey, lv);
        if (tile != null)
            tile.SetTileObject(merged);
        if (merged != null)
            merged.CopyUnitOptionsFrom(target); // 드래그한 쪽이 아니라 놓인 칸(target) 미니언의 설정 유지
        if (merged != null)
            merged.PlayMergeHop(); // 떨어지는 등장 대신 제자리에서 살짝 뛰었다 착지
        if (EffectManager.Instance != null && merged != null)
            EffectManager.Instance.Play(EffectType.Merge, merged.transform.position); // 머지 이펙트 (Resources/Effect/MergedEffect)
        mergeCount++;
        AchievementManager.Report(AchievementType.MergeMinion);
        // Character.Instance.AddMergeCoin(1);
        GameEventBus.Publish(new MergedMinionEvent());
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
        // AllySettingPanel.Instance.SetMinion(targetUnit);
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
        // AllySettingPanel.Instance.SetMinion(null);
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

public class MergedMinionEvent
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

// (2026-11-25) 필드 최대 미니언 수가 바뀜 (MinionCountInfoPanel 갱신)
public class MinionMaxCountChangedEvent { }

// (2026-11-25) 레벨업 창을 모두 닫음 (밀린 레벨업 없음) → 하단 정보 UI 갱신
public class LevelUpCanvasClosedEvent { }

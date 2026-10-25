using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlimeSpawner : MonoSingleton<SlimeSpawner>
{
    readonly Dictionary<string, Stack<Slime>> pool = new(); // 적 종류 별 풀링

    // [field: SerializeField] public int ActiveSlimeCount { get; private set; }

    public readonly HashSet<Slime> activeslimes = new();
    public int slimeMaxCount;
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
        Character.Instance.RemoveSlime(s);
        Slime slime = Character.Instance.AddSlime(key, level + 1);
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
        return await slime1.Merge(slime2);
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
        slimeMaxCount = GameSetting.SLIME_MIN_EXIST_COUNT;

        if (slimeLayer == 0)
            slimeLayer = LayerMask.GetMask("Slime");
        if (characterLayer == 0)
            characterLayer = LayerMask.GetMask("Character");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI 위를 클릭한 경우 드래그 시작하지 않음
            if (IsPointerOverUI())
                return;

            BeginDrag();
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
    // 클릭한 위치의 슬라임 or 캐릭터를 선택하고 드래그 시작
    void BeginDrag()
    {
        Vector3 mousePos = GetMouseWorldPos();
        ITileObject tileObject = FindSlime(mousePos, null);
        if (tileObject == null && IsCharacterAt(mousePos))
            tileObject = Character.Instance;

        if (tileObject == null)
        {
            ReleaseTarget();
            return;
        }

        if (tileObject is IAllyUnit allyUnit)
            SetTarget(allyUnit);
        tileObject.StartDrag();
        GameEventBus.Publish(new DragStartEvent(tileObject));
        dragObject = tileObject;
        originPos = tileObject.Transform.position;
        offset = originPos - mousePos;
        offset.z = 0;
    }

    // 슬라임끼리 놓으면 머지 시도, 빈 타일이면 이동, 그 외엔 원위치
    void EndDrag()
    {
        ITileObject source = dragObject;
        dragObject = null;

        // 드래그 중 오브젝트가 사라진 경우
        if (source == null || source.Transform == null || !source.Transform.gameObject.activeInHierarchy)
        {
            if (source != null)
                GameEventBus.Publish(new DragEndEvent(source, null));
            return;
        }

        Vector3 dropPos = GetMouseWorldPos();
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

    async UniTaskVoid Merge(Slime source, Slime target, Vector3 sourceOriginPos)
    {

        var (result, mergeResultKey, lv) = await Merge(source, target);
        if (!result)
        {
            source.transform.position = sourceOriginPos;
            return;
        }

        // 머지 결과는 타겟 슬라임이 있던 타일에 배치
        Tile tile = TileManager.Instance.GetTile(target);
        TileManager.Instance.RemoveTileObject(source);
        TileManager.Instance.RemoveTileObject(target);

        Character.Instance.RemoveSlime(source);
        Character.Instance.RemoveSlime(target);
        Slime merged = Character.Instance.AddSlime(mergeResultKey, lv);
        if (tile != null)
            tile.SetTileObject(merged);
        mergeCount++;
        Character.Instance.AddMergeCoin(1);
        GameEventBus.Publish(new MergedSlimeEvent());
        ReleaseTarget();
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    bool IsCharacterAt(Vector2 pos)
    {
        if (Character.Instance == null)
            return false;

        foreach (Collider2D col in Physics2D.OverlapPointAll(pos, characterLayer))
        {
            if (col.GetComponentInParent<Character>() == Character.Instance)
                return true;
        }
        return false;
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

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlimeSpawner : MonoSingleton<SlimeSpawner>
{
    readonly Dictionary<string, Stack<Slime>> pool = new(); // 적 종류 별 풀링

    [field: SerializeField] public int ActiveSlimeCount { get; private set; }

    public readonly HashSet<Slime> activeslimes = new();
    // public int slimeSlotCount;
    void Start()
    {
        // slimeSlotCount = GameSetting.MIN_SLIME_SLOT_COUNT;
    }
    public void PurchaseSlot(int idx)
    {
        // slimeSlotCount++;
    }
    // int initPrice = 50;
    // int increasePrice = 50;
    // public int GetSlotPrice()
    // {
    //     return initPrice + increasePrice * (slimeSlotCount - GameSetting.MIN_SLIME_SLOT_COUNT);
    // }
    public Slime Instantiate(string key)
    {
        SlimeData data = SlimeManager.Instance.slimeDataDic[key];

        if (!pool.ContainsKey(data.prefab.key))
            pool[data.prefab.key] = new Stack<Slime>();

        Slime slime = pool[data.prefab.key].Count > 0
            ? pool[data.prefab.key].Pop()
            : GameObject.Instantiate(data.prefab);


        slime.gameObject.SetActive(true);
        activeslimes.Add(slime);
        ActiveSlimeCount++;
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

    public void Release(Slime slime)
    {
        if (!pool.ContainsKey(slime.key))
            pool[slime.key] = new Stack<Slime>();

        activeslimes.Remove(slime);
        if (TileManager.Instance != null)
            TileManager.Instance.RemoveSlime(slime);
        slime.gameObject.SetActive(false);
        pool[slime.key].Push(slime);
        ActiveSlimeCount = Mathf.Max(0, ActiveSlimeCount - 1);
    }

    public async UniTask<(bool, string, int)> Merge(Slime slime1, Slime slime2)
    {
        return await slime1.Merge(slime2);
    }

        public LayerMask slimeLayer;
    // public float tileSnapDistance = 1f; // 드롭 위치에서 이 거리 이내의 타일로 이동

    // public bool IsMerging { get; private set; }

    [SerializeField] Slime dragSlime;
    Vector3 originPos;
    Vector3 offset;

    void Awake()
    {
        if (slimeLayer == 0)
            slimeLayer = LayerMask.GetMask("Slime");
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
        else if (dragSlime != null && Input.GetMouseButton(0))
        {
            dragSlime.transform.position = GetMouseWorldPos() + offset;
        }
        else if (dragSlime != null && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
        
    }

    

    [SerializeField] Slime targetSlime;
    // 클릭한 위치의 슬라임 드래그 시작
    void BeginDrag()
    {
        Vector3 mousePos = GetMouseWorldPos();
        Slime slime = FindSlime(mousePos, null);

        if (slime == null)
        {
            ReleaseTarget();
            return;
        }
        else
        {
            if(targetSlime != slime)
                ReleaseTarget();

        }
            
        targetSlime = slime;
        SlimeSettingPanel.Instance.SetSlime(targetSlime);
        targetSlime.StartDrag();
        dragSlime = targetSlime;
        originPos = targetSlime.transform.position;
        offset = originPos - mousePos;
        offset.z = 0;
    }

    // 놓은 위치에 슬라임이 있으면 머지 시도, 빈 타일이면 이동, 그 외엔 원위치
    void EndDrag()
    {
        Slime source = dragSlime;
        dragSlime = null;

        // 드래그 중 슬라임이 사라진 경우
        if (source == null || !source.gameObject.activeInHierarchy)
            return;

        Vector3 dropPos = GetMouseWorldPos();
        Slime target = FindSlime(dropPos, source);
        Tile tile = null;
        if (target == null)
        {
            tile = TileManager.Instance.GetNearestTile(dropPos);
            if (tile != null && tile.IsEmpty)
            {
                TileManager.Instance.RemoveSlime(source);
                tile.SetSlime(source);
            }
            else
            {
                source.transform.position = originPos;
            }
            source.EndDrag(tile);
            return;
        }

        source.EndDrag(tile);
        Merge(source, target, originPos).Forget();
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

        var (result, mergeResultKey, lv) = await SlimeSpawner.Instance.Merge(source, target);
        if (!result)
        {
            source.transform.position = sourceOriginPos;
            return;
        }

        // 머지 결과는 타겟 슬라임이 있던 타일에 배치
        Tile tile = TileManager.Instance.GetTile(target);
        TileManager.Instance.RemoveSlime(source);
        TileManager.Instance.RemoveSlime(target);

        Character.Instance.RemoveSlime(source);
        Character.Instance.RemoveSlime(target);
        Slime merged = Character.Instance.AddSlime(mergeResultKey, lv);
        if (tile != null)
            tile.SetSlime(merged);
        
        ReleaseTarget();        
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    void ReleaseTarget()
    {        
        Debug.Log("SlimeSpanwer ReleaseTarget(). 11");
        if(targetSlime == null)
            return;

        Debug.Log("SlimeSpanwer ReleaseTarget(). 22");
        targetSlime.EndTarget();
        targetSlime = null;
        SlimeSettingPanel.Instance.SetSlime(null); 
    }


    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // 모바일 터치는 fingerId 기준으로 판정
        bool isOver = Input.touchCount > 0
            ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            : EventSystem.current.IsPointerOverGameObject();

        if (isOver)
            LogPointerOverObjects();

        return isOver;
    }

    // 디버그용: 포인터 아래에 걸린 UI 오브젝트 로그
    readonly List<RaycastResult> raycastResults = new();
    void LogPointerOverObjects()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition
        };

        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        if (raycastResults.Count == 0)
        {
            Debug.Log("[IsPointerOverUI] 걸린 오브젝트 없음 (Raycast 결과 0개)");
            return;
        }

        for (int i = 0; i < raycastResults.Count; i++)
        {
            GameObject go = raycastResults[i].gameObject;
            Debug.Log($"[IsPointerOverUI] {i}: {GetHierarchyPath(go.transform)} (module: {(raycastResults[i].module != null ? raycastResults[i].module.GetType().Name : "null")})", go);
        }
    }

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

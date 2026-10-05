using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class MergeManager : MonoSingleton<MergeManager>
{
    public LayerMask slimeLayer;
    public float tileSnapDistance = 0.5f; // 드롭 위치에서 이 거리 이내의 타일로 이동

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

    // 클릭한 위치의 슬라임 드래그 시작
    void BeginDrag()
    {
        // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        // {
        //     Debug.Log("MergeManager BeginDrag() if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())");
        //     return;
        // }


        Vector3 mousePos = GetMouseWorldPos();
        Slime slime = FindSlime(mousePos, null);

        if (slime == null)
            return;
        slime.StartDrag();
        dragSlime = slime;
        originPos = slime.transform.position;
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
            tile = TileManager.Instance.GetNearestTile(dropPos, tileSnapDistance);
            if (tile != null && tile.IsEmpty)
            {
                TileManager.Instance.RemoveSlime(source);
                tile.SetSlime(source);
            }
            else
            {
                source.transform.position = originPos;
            }
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

        CharacterManageCanvas.Instance.UpdateCanvas();
    }


    Vector3 GetMouseWorldPos()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }
}

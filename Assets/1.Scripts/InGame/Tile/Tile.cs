using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int index;
    public TileGroup tileGroup; // 속한 3*3 그룹 (TileManager에서 지정)

    public ITileObject tileObject;
    public ITileItem tileItem; // 타일 하나에 아이템 하나만 설치
    public bool isBlocked; // 사용 불가 타일 (게임 시작 시 TileManager에서 랜덤 지정)

    public bool IsEmpty => !isBlocked && tileObject == null;
    public bool CanAddTileItem => !isBlocked && tileItem == null;

    // 막힌 타일은 오브젝트를 꺼서 안 보이게 함 (콜라이더도 꺼져서 드롭 대상에서도 빠짐)
    public void Block()
    {
        isBlocked = true;
        gameObject.SetActive(false);
    }

    public void Unblock()
    {
        isBlocked = false;
        gameObject.SetActive(true);
    }

    public void SetTileObject(ITileObject obj)
    {
        if (tileObject == obj) return;
        ITileObject prev = tileObject;
        if (tileObject != null)
        {
            tileItem?.OnExit(tileObject);                       // 이전 오브젝트에서 효과 제거
            UnlinkTileItem(tileObject);
        }
        tileObject = obj;
        if (obj != null)
        {
            obj.Transform.position = transform.position;
            tileItem?.OnEnter(obj);                             // 새 오브젝트에 효과 적용
            LinkTileItem(obj);
        }
        // 주변 배치에 따라 효과가 바뀌는 아이템(GrantItem)이 다시 계산하도록 알림
        GameEventBus.Publish(new TileObjectChangedEvent(this, prev, obj));
    }
    public void Clear() => SetTileObject(null);                 // Clear도 반드시 Exit를 타게 한다

    public bool AddTileItem(ITileItem item)
    {
        if (item == null || !CanAddTileItem)
            return false;

        tileItem = item;
        if (tileObject != null)
        {
            item.OnEnter(tileObject);  // 이미 누가 올라가 있으면 바로 적용
            LinkTileItem(tileObject);
        }
        return true;
    }

    public void RemoveTileItem(ITileItem item)
    {
        if (item == null || tileItem != item)
            return;
        if (tileObject != null)
        {
            item.OnExit(tileObject);
            UnlinkTileItem(tileObject);
        }
        tileItem = null;
    }

    // 유닛이 발사할 때 이 타일 아이템의 OnFired를 부를 수 있도록 연결
    void LinkTileItem(ITileObject obj)
    {
        if (obj is IShotModifiable m)
            m.ShotModifier.tileItem = tileItem as TileItem;
    }

    // 다른 타일로 먼저 옮겨져 연결이 바뀐 경우는 건드리지 않음
    void UnlinkTileItem(ITileObject obj)
    {
        if (obj is IShotModifiable m && m.ShotModifier.tileItem == tileItem as TileItem)
            m.ShotModifier.tileItem = null;
    }
}

public class TileObjectChangedEvent
{
    public Tile tile;
    public ITileObject prev;
    public ITileObject cur;
    public TileObjectChangedEvent(Tile tile, ITileObject prev, ITileObject cur)
    {
        this.tile = tile;
        this.prev = prev;
        this.cur = cur;
    }
}

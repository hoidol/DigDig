using UnityEngine;

public class Tile : MonoBehaviour
{
    public ITileObject tileObject;
    public bool isBlocked; // 사용 불가 타일 (게임 시작 시 TileManager에서 랜덤 지정)

    public bool IsEmpty => !isBlocked && tileObject == null;

    // 막힌 타일은 오브젝트를 꺼서 안 보이게 함 (콜라이더도 꺼져서 드롭 대상에서도 빠짐)
    public void Block()
    {
        isBlocked = true;
        gameObject.SetActive(false);
    }

    public void SetTileObject(ITileObject tileObject)
    {
        this.tileObject = tileObject;
        if (tileObject != null)
            tileObject.Transform.position = transform.position;
    }

    public void Clear()
    {
        tileObject = null;
    }
}

using UnityEngine;

// 타일 위에 올라가는 오브젝트 (Slime, Character). 드래그로 다른 타일로 옮길 수 있다.
public interface ITileObject
{
    Transform Transform
    {
        get;
    }

    void StartDrag();
    void EndDrag(Tile tile);
}

using UnityEngine;

// 타일에 설치되는 아이템. 타일 위에 오브젝트(Slime, Character)가 올라오면 OnEnter, 떠나면 OnExit
public interface ITileItem
{
    void OnEnter(ITileObject tileObject);
    void OnExit(ITileObject tileObject);
}

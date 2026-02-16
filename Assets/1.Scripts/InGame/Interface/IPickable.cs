using UnityEngine;
public interface IPickable
{
    string Key
    {
        get;
    }
    bool IsTaken
    {
        get;
        set;
    }

    public void Droped(Vector2 pos, string key);
    void Take(IPicker picker); // 플레이어가 가질때
    void PickedUp(); //플레이어가 얻을떄
}
using UnityEngine;

public class ShopWave : Wave
{
    //위쪽에 배치
    public override void StartWave(int phase)
    {
        Vector2 topPos = InGameUtil.GetSpawnTopPosition();
        EventObject eventObject = EventManager.Instance.Spawn(EventType.ItemShop);

        topPos.y = topPos.y *3/4;
        topPos.x = 0;
        eventObject.gameObject.SetActive(true);
        eventObject.Appear(topPos, () =>
        {
            MoveNextWave();
        });
    }
}
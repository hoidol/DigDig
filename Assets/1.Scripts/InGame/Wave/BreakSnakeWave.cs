using UnityEngine;

public class BreakSnakeWave : Wave
{
    public override void StartWave(int phase)
    {
        Vector2 topPos = InGameUtil.GetSpawnTopPosition();
        EventObject snake = EventManager.Instance.Spawn(EventType.Snake);
        topPos.y = topPos.y *3/4;
        topPos.x = 0;
        snake.gameObject.SetActive(true);
        snake.Appear(topPos, () =>
        {
            MoveNextWave();
        });
    }
}
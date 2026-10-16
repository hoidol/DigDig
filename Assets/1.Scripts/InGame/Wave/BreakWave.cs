using UnityEngine;

public class BreakWave : Wave
{

    // public LifeFountain lifeFountain;
    public override void StartWave(int phase)
    {
        Vector2 topPos = InGameUtil.GetSpawnTopPosition();

        EventObject lifeFountain = EventManager.Instance.Spawn(EventType.LifeFountain);
        topPos.y = topPos.y *3/4;
        topPos.x = 0;
        lifeFountain.gameObject.SetActive(true);
        lifeFountain.Appear(topPos, () =>
        {
            MoveNextWave();
        });
    }
}
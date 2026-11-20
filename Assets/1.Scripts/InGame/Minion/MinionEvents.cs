// 미니언 소환·제거 이벤트 (이전엔 MinionSpawnButton.cs에 있었음, 2026-11-20 소환 버튼 삭제로 이동)
public class SpawnMinionEvent
{
    public string key;
    public int reinforce;
    public SpawnMinionEvent(string k, int r)
    {
        key = k;
        reinforce = r;
    }
}

public class RemoveMinionEvent
{
    public string key;
    public RemoveMinionEvent(string k)
    {
        key = k;
    }
}

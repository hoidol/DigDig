using UnityEngine;


// [S] 성장: 필드(타일 위)에서 growTime초가 지나면 장착 슬라임 중 랜덤 종으로 성장, 머지 레벨 +1
[CreateAssetMenu(fileName = "GrowUp", menuName = "SlimeData/S/GrowUp")]
public class GrowUpSlimeData : EnhanceSlimeData
{
    public float growTime = 25f;   // 성장까지 시간
    public int growLevelUp = 1;    // 성장 시 머지 레벨 증가

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.GrowUp.Desc"), growTime, growLevelUp);
    }
}

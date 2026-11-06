using UnityEngine;


// [B] 산들바람: 기본 탄 발사 + 같은 TileGroup 안 십자 4칸 슬라임에게 [공격속도 증가] 버프 (CrossBuffSlime)
[CreateAssetMenu(fileName = "Breeze", menuName = "SlimeData/B/Breeze")]
public class BreezeSlimeData : CrossBuffSlimeData
{
    protected override float[] DefaultBuffValues => new[] { 1.2f, 1.3f, 1.4f, 1.5f, 1.6f };

    public override string GetDescription(int mergeLv = 0)
    {
        return $"십자범위 4칸 슬라임의 공격속도 {GetBuffPercent(mergeLv)}% 증가";
    }
}

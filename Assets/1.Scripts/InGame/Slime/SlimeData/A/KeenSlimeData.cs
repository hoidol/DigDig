using UnityEngine;


// [A] 설의: 기본 탄 발사 + 같은 TileGroup 안 십자 4칸 슬라임에게 [크리티컬 확률 증가] 버프 (CrossBuffSlime)
// 치명타 확률은 % 단위 합연산 (Add)
[CreateAssetMenu(fileName = "Keen", menuName = "SlimeData/A/Keen")]
public class KeenSlimeData : CrossBuffSlimeData
{
    protected override StatOpType DefaultBuffOpType => StatOpType.Add;
    protected override float[] DefaultBuffValues => new[] { 15f, 20f, 25f, 30f, 35f }; 
    
    public override string GetDescription(int mergeLv = 0)
    {
        return $"십자범위 4칸 슬라임의 크리티컬 확률 {GetBuffValue(mergeLv)}% 증가";
    }
}

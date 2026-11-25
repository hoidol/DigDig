using UnityEngine;


// [A] 설의: 기본 탄 발사 + 같은 TileGroup 안 십자 4칸 슬라임에게 [크리티컬 확률 증가] 버프 (CrossBuffMinion)
// 치명타 확률은 % 단위 합연산 (Add)
[CreateAssetMenu(fileName = "Keen", menuName = "MinionData/A/Keen")]
public class KeenMinionData : CrossBuffMinionData
{
    protected override StatOpType DefaultBuffOpType => StatOpType.Add;
    protected override float[] DefaultBuffValues => new[] { 15f, 20f, 25f, 30f, 35f }; 
    
    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Keen.Desc"), GetBuffValue(mergeLv));
    }
}

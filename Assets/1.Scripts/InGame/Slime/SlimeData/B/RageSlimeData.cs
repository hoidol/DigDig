using UnityEngine;


// [B] 격노: 기본 탄 발사 + 같은 TileGroup 안 십자 4칸 슬라임에게 [공격력 증가] 버프 (CrossBuffSlime)
[CreateAssetMenu(fileName = "Rage", menuName = "SlimeData/B/Rage")]
public class RageSlimeData : CrossBuffSlimeData
{
    protected override float[] DefaultBuffValues => new[] { 1.1f, 1.15f, 1.2f, 1.25f, 1.3f };

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Slime.Rage.Desc"), GetBuffPercent(mergeLv));
    }
}

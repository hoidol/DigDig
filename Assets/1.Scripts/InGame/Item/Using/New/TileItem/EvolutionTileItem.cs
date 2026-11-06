using UnityEngine;

// 진화: 최대 머지 레벨(SlimeData.MAX_MERGE_LEVEL) 슬라임이 올라가면 진화 (Slime.Evolve)
// 진화 내용은 추후 기획. 슬라임마다 한 번만 진화, 타일은 남아서 다른 최대 레벨 슬라임도 진화 가능
// 뽑기: 필드에 진화 안 한 최대 레벨 슬라임이 있을 때만 후보
public class EvolutionTileItem : TileItem //타일 위에 설치되는 아이템
{
    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is Slime slime && slime.CanEvolve)
            slime.Evolve();
    }

    public override bool CanDraw()
    {
        foreach (Slime slime in KingSlime.Instance.slimeInventory.curSlimes)
        {
            if (slime != null && slime.CanEvolve)
                return true;
        }
        return false;
    }
}

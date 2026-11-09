using System.Linq;
using UnityEngine;

// 진화: 최대 머지 레벨(SlimeData.MAX_MERGE_LEVEL) 슬라임이 올라가면 진화 (Slime.Evolve)
// 진화 내용은 추후 기획. 슬라임마다 한 번만 진화, 타일은 남아서 다른 최대 레벨 슬라임도 진화 가능
// 뽑기 (CLAUDE.md "타일 아이템"): 필드의 진화 안 한 최대 레벨 슬라임 수 > 설치된 진화 타일 수일 때만,
// 뽑기 시도마다 DRAW_CHANCE% 확률로 선택지 3개 중 하나로 확정 등장. 보유 상한(MaxCount)은 쓰지 않음
public class EvolutionTileItem : TileItem //타일 위에 설치되는 아이템
{
    public const float DRAW_CHANCE = 40f;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is Slime slime && slime.CanEvolve)
            slime.Evolve();
    }

    // 진화 타일 수가 진화할 슬라임 수보다 적을 때만 (같으면 안 나옴, 많아지는 경우는 없어야 함)
    public static bool CanDrawFor(int evolvableSlimeCount, int evolutionTileCount) => evolutionTileCount < evolvableSlimeCount;

    public static int EvolvableSlimeCount => KingSlime.Instance.slimeInventory.curSlimes.Count(s => s != null && s.CanEvolve);
    public static int InstalledCount => TileManager.Instance.tiles.Count(t => t.tileItem is EvolutionTileItem);

    public override bool CanDraw() => CanDrawFor(EvolvableSlimeCount, InstalledCount);

    public override bool IgnoreMaxCount => true;
    public override float? DrawChance => DRAW_CHANCE;
}

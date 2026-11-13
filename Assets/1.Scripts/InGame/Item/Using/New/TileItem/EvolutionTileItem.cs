using System.Collections;
using System.Linq;
using UnityEngine;

// 진화: 최대 머지 레벨(MinionData.MAX_MERGE_LEVEL) 슬라임이 올라가면 진화 (Minion.Evolve)
// 슬라임마다 한 번만 진화. 진화시키면 타일 아이템은 사라짐(1회용, 2026-11-10)
// 뽑기 (CLAUDE.md "타일 아이템"): 필드의 진화 안 한 최대 레벨 슬라임 수 > 설치된 진화 타일 수일 때만,
// 뽑기 시도마다 DRAW_CHANCE% 확률로 선택지 3개 중 하나로 확정 등장. 보유 상한(MaxCount)은 쓰지 않음
public class EvolutionTileItem : TileItem //타일 위에 설치되는 아이템
{
    public const float DRAW_CHANCE = 40f;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is Minion minion && minion.CanEvolve)
        {
            minion.Evolve();
            if (!isConsumed)
            {
                isConsumed = true;
                StartCoroutine(CoConsume());
            }
        }
    }

    bool isConsumed;

    // OnEnter는 설치(OnEquip → AddTileItem)·타일 이동(SetTileObject) 도중에 불리므로 그 처리가 끝난 다음 프레임에 제거
    IEnumerator CoConsume()
    {
        yield return null;
        if (this != null && KingSlime.Instance != null)
            KingSlime.Instance.RemoveItem(this);
    }

    // 진화 타일 수가 진화할 슬라임 수보다 적을 때만 (같으면 안 나옴, 많아지는 경우는 없어야 함)
    public static bool CanDrawFor(int evolvableMinionCount, int evolutionTileCount) => evolutionTileCount < evolvableMinionCount;

    public static int EvolvableMinionCount => KingSlime.Instance.minionInventory.curMinions.Count(s => s != null && s.CanEvolve);
    public static int InstalledCount => TileManager.Instance.tiles.Count(t => t.tileItem is EvolutionTileItem);

    public override bool CanDraw() => CanDrawFor(EvolvableMinionCount, InstalledCount);

    public override bool IgnoreMaxCount => true;
    public override float? DrawChance => DRAW_CHANCE;
}

using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-20) 종족: 첫 레벨업 창 = 종족 카드 3장 → 고르면 킹 스탯 버프·너프 + 버프 칸(킹 기준 3칸) 미니언 버프
// 킹이 움직이면 대상이 바뀌고, 버프 칸 밖 미니언은 버프 없음
public class KingRacePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Race_FirstLevelUp_AndTileBuff()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        KingSlime king = KingSlime.Instance;
        var setting = KingGrowthSetting.Instance;
        Assert.IsNotNull(setting);

        // 1) 첫 레벨업 창 = 종족 카드 3장
        king.AddExp(king.MaxExp);
        yield return WaitFrames(2);
        var canvas = InGame.LevelUpCanvas.Instance;
        Assert.IsTrue(canvas.gameObject.activeSelf, "레벨업 창 안 열림");
        Assert.AreEqual(0f, Time.timeScale, "레벨업 중 시간 정지 안 됨");
        var shown = canvas.levelUpStatPanels.Where(p => p.gameObject.activeSelf).ToList();
        Assert.AreEqual(3, shown.Count, "종족 카드 3장");

        // 2) 오크를 직접 고름 (첫 패널에 오크 데이터를 넣고 선택 = 실제 클릭과 같은 경로)
        AbilityData orc = setting.raceAbilities.First(d => ((RaceKingAbility)d.prefab).raceType == KingRaceType.Orc);
        float atkBefore = king.statMgr.AttackPower;
        float spdBefore = king.statMgr.AttackSpeed;
        shown[0].SetData(orc, 1);
        shown[0].OnClickedSelect();
        yield return WaitFrames(2);
        Assert.IsFalse(canvas.gameObject.activeSelf, "고른 뒤 창이 안 닫힘");
        Assert.AreNotEqual(0f, Time.timeScale, "닫은 뒤 시간 복구 안 됨");
        Assert.IsTrue(king.abilityInventory.HasCategory(AbilityCategory.Race));
        Assert.AreEqual(atkBefore * 1.15f, king.statMgr.AttackPower, 0.01f, "오크 킹 공격력 +15%");
        Assert.AreEqual(spdBefore * 0.9f, king.statMgr.AttackSpeed, 0.01f, "오크 킹 공속 -10%");

        // 3) 킹을 가운데쯤 칸에 두고 버프 칸(위 (0,-1))·밖 칸에 미니언
        Tile center = TileManager.Instance.GetTile(new Vector2Int(1, 1));
        Tile up = TileManager.Instance.GetTile(new Vector2Int(1, 0));
        Tile outside = TileManager.Instance.GetTile(new Vector2Int(2, 1)); // 오른쪽 = 오크 버프 칸 아님
        foreach (var t in new[] { center, up, outside })
        {
            if (t.isBlocked) t.Unblock();
        }
        MoveKing(king, center);
        yield return WaitFrames(2);

        Minion buffed = SpawnMinion("Iron", up, 0);
        Minion plain = SpawnMinion("Iron", outside, 0);
        yield return WaitFrames(3);
        float baseAtk = plain.statDic[StatType.AttackPower];
        Assert.AreEqual(baseAtk * 1.08f, buffed.statDic[StatType.AttackPower], 0.01f, "버프 칸 미니언 공격력 +8%");

        var area = king.GetComponent<KingTileBuffArea>();
        Assert.IsNotNull(area, "버프 칸 처리 없음");
        CollectionAssert.Contains(area.GetTargetTiles(), up);
        CollectionAssert.DoesNotContain(area.GetTargetTiles(), outside);

        // 4) 킹이 움직이면 버프가 빠짐 (킹을 아래로 한 칸 → 위 칸은 이제 킹 칸 기준 두 칸 위)
        Tile down = TileManager.Instance.GetTile(new Vector2Int(1, 2));
        if (down.isBlocked) down.Unblock();
        MoveKing(king, down);
        yield return WaitFrames(3);
        Assert.AreEqual(baseAtk, buffed.statDic[StatType.AttackPower], 0.01f, "킹이 떠나면 버프 해제");
    }

    // 킹을 빈 칸으로 옮김 (타일 오브젝트 변경 이벤트가 나가도록 Tile 함수 사용)
    static void MoveKing(KingSlime king, Tile to)
    {
        Tile from = TileManager.Instance.GetTile(king);
        if (from == to)
            return;
        if (from != null)
            from.SetTileObject(null);
        to.SetTileObject(king);
        king.transform.position = to.transform.position;
    }
}

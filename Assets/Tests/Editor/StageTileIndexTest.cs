using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// (2026-11-24) 스테이지 맵 프리팹(Resources/StageMap/Stage0~): 타일 24칸 이상·index 겹치지 않음·경로 2개 지점 이상·도착 같음
// Stage0은 저장된 index가 위치 기준 계산과 같음 (예전 맵 그대로)
public class StageTileIndexTest
{
    static Stage[] LoadMaps()
    {
        return Enumerable.Range(0, 20)
            .Select(i => AssetDatabase.LoadAssetAtPath<Stage>($"Assets/Resources/{TileManager.STAGE_MAP_PATH}{i}.prefab"))
            .TakeWhile(s => s != null).ToArray();
    }

    [Test]
    public void StageMapPrefabs_Valid()
    {
        Stage[] maps = LoadMaps();
        Assert.GreaterOrEqual(maps.Length, 6, "Stage0~5 프리팹");
        for (int i = 0; i < maps.Length; i++)
        {
            Stage stage = maps[i];
            Assert.AreEqual(i, stage.mapIndex, $"{stage.name} mapIndex");
            Assert.GreaterOrEqual(stage.tiles.Length, 24, $"{stage.name} 타일 수");
            Assert.IsEmpty(stage.FindDuplicateIndices(), $"{stage.name} index 중복");
            Assert.IsNotEmpty(stage.routes, $"{stage.name} 경로 없음");
            Vector2 end = stage.routes[0].GetValidPoints().Last().position;
            foreach (StageRoute route in stage.routes)
            {
                Transform[] p = route.GetValidPoints();
                Assert.GreaterOrEqual(p.Length, 2, $"{stage.name} 경로 지점 수");
                Assert.Less(Vector2.Distance(end, p.Last().position), 0.01f, $"{stage.name} 도착 위치가 다름");
            }
        }
    }

    [Test]
    public void Stage0_IndicesMatchPosition()
    {
        Stage prefab = LoadMaps()[0];
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
        try
        {
            Stage stage = go.GetComponent<Stage>();
            Vector2Int[] saved = stage.tiles.Select(t => t.index).ToArray();
            Canvas.ForceUpdateCanvases();
            TileManager.AssignIndicesByPosition(stage.tiles);
            for (int i = 0; i < saved.Length; i++)
                Assert.AreEqual(saved[i], stage.tiles[i].index, $"{i}");
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 로드된 모든 슬라임 key × 머지 레벨 0/최대를 하나씩 소환해 싸우게 하고 에러가 없는지 본다
public class SlimePlayTest : InGamePlayTestBase
{
    const float TIME_SCALE = 4f;
    const float SLIME_TEST_TIME = 6f; // 슬라임 하나당 게임 시간(초)

    [UnityTest]
    public IEnumerator AllSlimes_SpawnAndFight_NoErrors()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();

        Time.timeScale = TIME_SCALE;
        var failed = new List<string>();
        foreach (string key in SlimeManager.Instance.slimeDataDic.Keys.ToList())
        {
            foreach (int lv in new[] { 0, SlimeData.MAX_MERGE_LEVEL })
            {
                current = $"{key}/{lv}";
                int before = errors.Count;
                Slime slime = null;
                try
                {
                    Tile tile = GetFreeTile();
                    Assert.IsNotNull(tile, "빈 타일 없음");
                    slime = SpawnSlime(key, tile, lv);
                }
                catch (System.Exception e)
                {
                    errors.Add($"[{current}] 소환 중 예외: {e.GetType().Name} {e.Message.Split('\n')[0]}");
                }

                yield return WaitRealtime(SLIME_TEST_TIME / TIME_SCALE);

                try
                {
                    if (slime != null)
                        SlimeSpawner.Instance.RemoveFieldSlime(slime);
                }
                catch (System.Exception e)
                {
                    errors.Add($"[{current}] 제거 중 예외: {e.GetType().Name} {e.Message.Split('\n')[0]}");
                }
                yield return WaitRealtime(0.2f);

                if (errors.Count > before)
                    failed.Add(current);
            }
        }

        AssertNoErrors($"에러 난 슬라임: {string.Join(", ", failed)}");
    }

    // 장착 여부와 상관없이 SlimeData.csv에 있는 key는 모두 로드돼야 한다 (데이터 에셋 누락 검출)
    [UnityTest]
    public IEnumerator AllCsvSlimes_AreLoaded()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();

        var csvKeys = System.IO.File.ReadAllLines("Assets/Json/SlimeData.csv")
            .Skip(1)
            .Select(l => l.Split('\t')[0].Trim())
            .Where(k => k.Length > 0)
            .ToList();
        var missing = csvKeys.Where(k => !SlimeManager.Instance.slimeDataDic.ContainsKey(k)).ToList();
        Assert.IsEmpty(missing, "CSV에는 있는데 SlimeData 에셋이 없는 슬라임");
    }
}

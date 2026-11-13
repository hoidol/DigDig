using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// 첫 플레이 튜토리얼(TutorialManager가 FirstPlayTutorial 프리팹을 생성)을 처음부터 끝까지 진행
// 대화는 대화창 터치, 킹 이동·합성은 드롭(MinionFieldController.DropAt), 소환은 소환 버튼 클릭으로 흉내
// 확인: wave 시작 블록 전엔 wave 안 시작 / 블록 순서대로 진행 / 킹 칸·소환 종·칸·합성 / 끝나면 완료 기록·오브젝트 삭제·wave 진행
public class TutorialPlayTest : InGamePlayTestBase
{
    const float STEP_TIMEOUT = 20f; // 실시간(초), 블록 하나

    static readonly MethodInfo DROP_AT = typeof(MinionFieldController).GetMethod("DropAt",
        BindingFlags.NonPublic | BindingFlags.Instance);

    [UnityTest]
    public IEnumerator FirstPlay_RunToEnd()
    {
        if (!Application.isPlaying)
        {
            BeforeEnterPlayMode();
            SessionState.SetBool(TutorialManager.DISABLE_FOR_TEST_KEY, false); // 이 테스트만 튜토리얼 켬
            TutorialManager.SetCompleted(TutorialType.FirstPlay, false);        // 완료 기록은 TearDown에서 복원
            yield return new EnterPlayMode();
        }
        yield return WaitUntilReady(() => TutorialManager.Instance != null && TutorialManager.Instance.IsPlaying, "튜토리얼이 시작되지 않음");
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        Tutorial tutorial = TutorialManager.Instance.CurrentTutorial;
        Assert.AreEqual(TutorialType.FirstPlay, tutorial.tutorialType);
        Assert.AreEqual(1, Object.FindObjectsByType<Tutorial>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, "튜토리얼은 매니저가 만든 하나만 있어야 함 (씬에 두지 않음)");
        Assert.IsNotNull(DROP_AT, "MinionFieldController.DropAt 없음");
        GameObject tutorialObject = tutorial.gameObject;
        var steps = tutorial.steps.ToList();
        int startWaveIdx = steps.FindIndex(s => s is StartWaveTutorialStep);

        for (int i = 0; i < steps.Count; i++)
        {
            TutorialStep step = steps[i];
            current = $"{i:00} {step.GetType().Name} ({step.name})";
            double timeout = EditorApplication.timeSinceStartup + STEP_TIMEOUT;
            while (tutorial != null && tutorial.CurrentStep == step)
            {
                Assert.Less(EditorApplication.timeSinceStartup, timeout, $"{current} 블록이 끝나지 않음");
                if (startWaveIdx > i)
                    Assert.IsFalse(GameManager.Instance.isPlaying, $"{current}: wave 시작 블록 전에 wave가 시작됨");

                switch (step)
                {
                    case DialogueTutorialStep:
                        if (tutorial.canvas.dialogueObject.activeInHierarchy)
                            tutorial.canvas.dialogueTouchButton.onClick.Invoke();
                        break;
                    case MoveKingTutorialStep move:
                        Assert.IsFalse(tutorial.canvas.blockObject.activeSelf, $"{current}: 화면 블록이 안 풀림");
                        Assert.IsTrue(tutorial.canvas.finger.gameObject.activeInHierarchy, $"{current}: 손 안내가 안 나옴");
                        yield return Drop(KingSlime.Instance, TileManager.Instance.GetTile(move.targetTileIndex));
                        break;
                    case MergeMinionTutorialStep merge:
                        Tile from = TileManager.Instance.GetTile(merge.fromTileIndex);
                        if (from.tileObject is Minion fromMinion)
                            yield return Drop(fromMinion, TileManager.Instance.GetTile(merge.toTileIndex));
                        break;
                    case SpawnMinionTutorialStep spawn:
                        if (MinionSpawnButton.forcedMinionKey == spawn.minionKey)
                            yield return Spawn(spawn);
                        break;
                }
                yield return WaitRealtime(0.05f);
            }

            if (step is MoveKingTutorialStep moved)
                Assert.AreSame(TileManager.Instance.GetTile(moved.targetTileIndex), TileManager.Instance.GetTile(KingSlime.Instance), $"{current}: 킹이 목표 칸에 없음");
            if (step is SetKingTileTutorialStep set)
                Assert.AreSame(TileManager.Instance.GetTile(set.tileIndex), TileManager.Instance.GetTile(KingSlime.Instance), $"{current}: 킹이 지정 칸에 없음");
            if (step is StartWaveTutorialStep)
                Assert.IsTrue(GameManager.Instance.isPlaying, $"{current}: wave가 시작되지 않음");
            if (step is MergeMinionTutorialStep m)
            {
                Minion merged = TileManager.Instance.GetTile(m.toTileIndex).tileObject as Minion;
                Assert.IsNotNull(merged, $"{current}: 합성 결과가 대상 칸에 없음");
                Assert.AreEqual(1, merged.mergeLevel, $"{current}: 합성 결과 레벨");
                Assert.IsNull(TileManager.Instance.GetTile(m.fromTileIndex).tileObject, $"{current}: 끌어온 칸이 비지 않음");
            }
        }

        yield return WaitFrames(2);
        Assert.IsFalse(TutorialManager.Instance.IsPlaying, "튜토리얼이 끝나지 않음");
        Assert.IsTrue(TutorialManager.IsCompleted(TutorialType.FirstPlay), "완료 기록 없음");
        Assert.IsTrue(tutorialObject == null, "끝난 튜토리얼 오브젝트(Canvas 포함)가 삭제되지 않음");
        Assert.IsTrue(GameManager.Instance.isPlaying, "튜토리얼 뒤 wave 진행 안 됨");
        Assert.IsNull(MinionFieldController.Instance.dragFilter, "드래그 제한이 남음");
        Assert.IsNull(MinionSpawnButton.forcedMinionKey, "지정 소환이 남음");

        MinionSpawnButton spawnButton = Object.FindFirstObjectByType<MinionSpawnButton>(FindObjectsInactive.Include);
        Assert.IsTrue(spawnButton.gameObject.activeInHierarchy, "소환 버튼이 안 보임");
    }

    // 소환 버튼을 눌러 지정 종이 지정 칸에 나오는지
    IEnumerator Spawn(SpawnMinionTutorialStep spawn)
    {
        MinionSpawnButton button = Object.FindFirstObjectByType<MinionSpawnButton>(FindObjectsInactive.Include);
        Assert.IsTrue(button.gameObject.activeInHierarchy, $"{current}: 소환 버튼이 안 보임");
        Assert.AreEqual(spawn.minionKey, MinionSpawnButton.forcedMinionKey, $"{current}: 지정 소환 준비 안 됨");
        Assert.GreaterOrEqual(KingSlime.Instance.coin, MinionSpawnButton.GetSpawnPrice(), $"{current}: 코인을 채워 주지 않음");
        button.OnClickedBtn();
        yield return WaitFrames(3);

        Minion minion = TileManager.Instance.GetTile(spawn.tileIndex).tileObject as Minion;
        Assert.IsNotNull(minion, $"{current}: 소환이 {spawn.tileIndex} 칸에 없음");
        Assert.AreEqual(spawn.minionKey, minion.key, $"{current}: 소환 종");
        Assert.AreEqual(0, minion.mergeLevel, $"{current}: 소환 레벨");
    }

    static IEnumerator Drop(ITileObject source, Tile target)
    {
        Vector3 origin = source.Transform.position;
        source.StartDrag();
        source.Transform.position = target.transform.position;
        DROP_AT.Invoke(MinionFieldController.Instance, new object[] { source, target.transform.position, origin });
        yield return WaitFrames(3);
    }
}

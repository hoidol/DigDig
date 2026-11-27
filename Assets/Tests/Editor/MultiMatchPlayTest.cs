using System.Collections;
using Cysharp.Threading.Tasks;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

// (2026-11-27) 멀티 1단계 흐름 (가짜 서버 FakeMultiBackend: 봇이 채움)
// 대기열 참여 → 8명 → 멀티 InGame + MultiGameUI(Ready 창 8줄) → Ready → 카운트다운 끝 → 동시 시작(배속·광고 제거 효과 없음)
// → (2단계) 오른쪽 위 다른 유저 7줄(봇 상황) · [보기] = 그 유저 배치 + 내 카메라 끔 · 웨이브 50 보스 처치 = 결과 창(우승) + 전적
// → 로비 (2026-11-27 대기 중 로컬 게임·이어하기 삭제)
public class MultiMatchPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Match_Ready_Start_ThenResumeLocal()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    static IEnumerator WaitFor(System.Func<bool> cond, string msg, float seconds = 30f)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        while (!cond())
        {
            Assert.Less(EditorApplication.timeSinceStartup, end, msg);
            if (EditorApplication.isPaused)
                EditorApplication.isPaused = false;
            yield return null;
        }
    }

    IEnumerator Body()
    {
        MultiMatchManager.ResetForTest();
        MultiMatchManager.ForceFakeBackend = true;
        float prevInterval = FakeMultiBackend.botJoinInterval;
        float prevWave = FakeMultiBackend.botWaveInterval;
        FakeMultiBackend.botJoinInterval = 0.05f;
        FakeMultiBackend.botWaveInterval = 0.3f;
        try
        {
            // 2) 대기열 → 8명 → 멀티 InGame
            GameManager singleGm = GameManager.Instance;
            MultiMatchManager.Instance.Join().Forget();
            yield return WaitFor(() => MultiGame.IsActive && GameManager.Instance != null && GameManager.Instance != singleGm
                && Object.FindFirstObjectByType<MultiReadyPanel>() != null, "멀티 InGame + Ready 창이 안 열림");
            Assert.AreEqual(MultiGame.MULTI_STAGE_ORDER, UserDataManager.stageOrder, "멀티 스테이지");
            Assert.IsFalse(UserDataManager.Instance.IsGameSpeedUp, "멀티: 배속 상품 효과 없음");
            Assert.IsFalse(UserDataManager.Instance.IsAdRemovedEffective, "멀티: 광고 제거 효과 없음");

            MultiReadyPanel ready = Object.FindFirstObjectByType<MultiReadyPanel>();
            yield return WaitFor(() => ready.GetComponentsInChildren<MultiPlayerRow>(false).Length == MultiGame.ROOM_SIZE, "Ready 창 8줄");
            Assert.IsFalse(GameManager.Instance.IsGameStarted, "카운트다운 전에 시작함");

            // 3) Ready → 내 Ready 표시 → 카운트다운 끝 → 시작
            ready.readyButton.onClick.Invoke();
            yield return WaitFrames(3);
            Assert.IsTrue(MultiGame.Room.IsReady(MultiMatchManager.Instance.MyUid), "Ready 기록");
            Assert.IsFalse(ready.readyButton.interactable, "Ready는 한 번만");
            yield return WaitFor(() => GameManager.Instance.IsGameStarted, "카운트다운이 끝났는데 시작 안 함", 25f);
            Assert.IsTrue(MultiGame.CanStart);
            Assert.IsFalse(ready.root.activeSelf, "시작하면 Ready 창 닫힘");

            // 4) 다른 유저 상황: 7줄 (나 제외), 봇 wave가 오르면 갱신
            MultiPlayersPanel players = Object.FindFirstObjectByType<MultiPlayersPanel>();
            Assert.IsNotNull(players, "다른 유저 목록 없음");
            yield return WaitFor(() => players.GetComponentsInChildren<MultiStatusRow>(false).Length == MultiGame.ROOM_SIZE - 1, "다른 유저 7줄");
            yield return WaitFor(() => MultiGame.Room.players.Any(p => p.uid != MultiMatchManager.Instance.MyUid && p.wave >= 3), "봇 wave가 안 오름");
            Assert.IsTrue(MultiGame.Room.Get(MultiMatchManager.Instance.MyUid).wave >= 1, "내 상황 올림");

            // 5) 보기: 그 유저 배치 + 메인 카메라 끔 → 닫기 = 카메라 다시
            MultiStatusRow row = players.GetComponentsInChildren<MultiStatusRow>(false).First();
            Camera cam = Camera.main;
            row.viewButton.onClick.Invoke();
            yield return WaitFrames(2);
            MultiSpectatePanel spectate = players.spectatePanel;
            Assert.IsTrue(spectate.IsOpen, "보기 창이 안 열림");
            Assert.AreEqual(row.Uid, spectate.TargetUid);
            Assert.IsFalse(cam.enabled, "보기 중 내 카메라 끔");
            Assert.Greater(spectate.GetComponentsInChildren<MultiSpectateCell>(false).Length, 10, "보기 칸");
            Assert.IsTrue(GameManager.Instance.isPlaying, "보기 중에도 내 게임 진행");
            spectate.closeButton.onClick.Invoke();
            yield return WaitFrames(2);
            Assert.IsFalse(spectate.IsOpen);
            Assert.IsTrue(cam.enabled, "닫으면 카메라 다시 켬");

            // 6) 웨이브 50 보스 처치(클리어) → 결과 창 + 우승·클리어·판수
            var user = UserDataManager.Instance.userData;
            int champ = user.multiChampion, clear = user.multiClear, games = user.multiGames;
            GameManager.Instance.isCleared = true;
            ResultCanvas.Instance.OpenCanvas();
            MultiResultPanel result = Object.FindFirstObjectByType<MultiResultPanel>(FindObjectsInactive.Include);
            yield return WaitFor(() => result.root.activeSelf, "멀티 결과 창이 안 열림");
            Assert.IsFalse(ResultCanvas.Instance.gameObject.activeSelf, "멀티는 싱글 결과 창 대신 멀티 결과 창");
            Assert.AreEqual(champ + 1, user.multiChampion, "1등 = 우승 +1");
            Assert.AreEqual(clear + 1, user.multiClear, "1등 = 클리어도 +1");
            Assert.AreEqual(games + 1, user.multiGames, "판수 +1");
            Assert.AreEqual(MultiMatchManager.Instance.MyUid, MultiGame.Room.winnerUid);
            Assert.AreEqual(MultiGame.ROOM_SIZE, result.GetComponentsInChildren<MultiResultRow>(false).Length, "결과 8줄");

            // 7) 로비로 → 이어하기
            result.lobbyButton.onClick.Invoke();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Lobby", "로비로 안 감");
            Assert.IsFalse(MultiGame.IsActive);
        }
        finally
        {
            FakeMultiBackend.botJoinInterval = prevInterval;
            FakeMultiBackend.botWaveInterval = prevWave;
            MultiMatchManager.ForceFakeBackend = null;
            MultiMatchManager.ResetForTest();
        }
    }
}

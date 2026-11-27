using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-27) 방만들기 · 코드입력 · 방장 · 공유 (가짜 서버)
// 방만들기 = 내가 방장 + 7자리 코드 → [친구 초대] = 게임 링크 공유 · [코드 공유] = 코드 공유 → 취소
// 코드입력 = 봇이 먼저 있는 방(방장 = 봇1) → 방장 나감 → 두 번째 사람 → … → 내가 방장 → 내가 8명 차면 시작 기준(fullAt) 기록 → 멀티 판
public class MultiRoomPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator CreateRoom_JoinByCode_HostHandover()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    static IEnumerator WaitFor(System.Func<bool> cond, string msg, float seconds = 20f)
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
        try
        {
            MultiMatchManager mm = MultiMatchManager.Instance;

            // 1) 방만들기: 방장 = 나, 코드 7자리, 공유 버튼
            FakeMultiBackend.botJoinInterval = 60f; // 봇이 안 들어오게
            mm.CreateRoom().Forget();
            yield return WaitFor(() => mm.State == MultiMatchManager.MatchState.Waiting && mm.Room != null, "방만들기 대기가 안 됨");
            Assert.IsTrue(mm.IsHost, "방을 만든 사람 = 방장");
            Assert.IsTrue(MultiGame.IsValidCode(mm.RoomCode), $"7자리 코드 ({mm.RoomCode})");
            Assert.IsFalse(mm.Room.isPrivate, "방만들기 방도 다른 방과 똑같음 (빠른 매칭·코드 모두)");
            MultiMatchStatusPanel status = mm.GetComponentInChildren<MultiMatchStatusPanel>(true);
            Assert.IsNotNull(status, "대기 표시 없음");
            Assert.IsTrue(status.codeText.text.Contains(mm.RoomCode), "대기 표시에 코드");
            Assert.IsTrue(status.hostMark.activeSelf, "방장 표시");
            yield return WaitFrames(2);
            Assert.IsTrue(status.timerText.gameObject.activeSelf, "AI로 채우기까지 남은 시간");
            Assert.AreEqual(TranslateManager.GetText("Multi_AiNotice"), status.noticeText.text, "AI 대체 안내");
            var u = UserDataManager.Instance.userData;
            Assert.AreEqual(string.Format(TranslateManager.GetText("Multi_Record"), u.multiChampion, u.multiClear), status.recordText.text, "대기 표시에 우승·클리어");
            status.inviteButton.onClick.Invoke();
            StringAssert.Contains(MultiShare.GameLink, NativeShareUtil.LastShared, "친구 초대 = 게임 링크");
            status.shareCodeButton.onClick.Invoke();
            StringAssert.Contains(mm.RoomCode, NativeShareUtil.LastShared, "코드 공유 = 7자리 코드");
            mm.Cancel().Forget();
            yield return WaitFrames(2);
            Assert.AreEqual(MultiMatchManager.MatchState.Idle, mm.State);

            // 2) 코드입력: 봇 3명이 먼저 있는 방 → 방장 = 봇1
            mm.JoinByCode("1234567").Forget();
            yield return WaitFor(() => mm.State == MultiMatchManager.MatchState.Waiting && mm.Room != null, "코드 입장이 안 됨");
            Assert.AreEqual("1234567", mm.RoomCode);
            Assert.AreEqual("bot1", mm.Room.HostUid, "먼저 들어온 사람이 방장");
            Assert.IsFalse(mm.IsHost);

            // 3) 방장이 나가면 두 번째로 들어온 사람 → … → 나
            mm.SimulateLeaveForTest("bot1");
            yield return WaitFrames(1);
            Assert.AreEqual("bot2", mm.Room.HostUid, "방장이 나가면 두 번째 사람");
            mm.SimulateLeaveForTest("bot2");
            mm.SimulateLeaveForTest("bot3");
            yield return WaitFrames(1);
            Assert.IsTrue(mm.IsHost, "남은 사람 중 가장 먼저 들어온 나 = 방장");

            // 4) 방장(나)이 8명 차면 시작 기준을 씀 → 멀티 판
            FakeMultiBackend.botJoinInterval = 0.05f;
            yield return WaitFor(() => mm.State == MultiMatchManager.MatchState.Starting, "방이 찼는데 시작 안내로 안 넘어감", 20f);
            Assert.IsTrue(mm.Room.fullAtMs > 0, "방장이 시작 기준(fullAt)을 씀");
            yield return WaitFrames(2);
            StringAssert.Contains(TranslateManager.GetText("Multi_StartIn").Split('{')[0], status.noticeText.text, "\"5초 후 시작합니다\"");
            Assert.IsFalse(mm.CanCancel, "찬 뒤엔 취소 안 됨");
            yield return WaitFor(() => MultiGame.IsActive, "5초 뒤 멀티 판으로 안 넘어감", 20f);
            yield return WaitFor(() => Object.FindFirstObjectByType<MultiReadyPanel>() != null, "Ready 창이 안 열림 (씬에서 꺼 둬도 열려야 함)");
        }
        finally
        {
            FakeMultiBackend.botJoinInterval = prevInterval;
            MultiMatchManager.ForceFakeBackend = null;
            MultiMatchManager.ResetForTest();
            LocalGameSave.Clear();
        }
    }
}

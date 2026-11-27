using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-27) 플레이 기록 (가짜 서버)
// 싱글: 브레이크타임마다 장면 → 결과 화면 = 로컬 백업 + 서버(300칸)
// 멀티: 빠른 매칭 방이 안 차면 방장(나)이 플레이 기록 재생 AI 봇 7명으로 채움 → 시작 → 봇 wave가 기록대로 오름 · 보기 배치
public class PlayRecordPlayTest : InGamePlayTestBase
{
    static IEnumerator WaitFor(System.Func<bool> cond, string msg, float seconds = 30f)
    {
        double end = EditorApplication.timeSinceStartup + seconds;
        while (!cond())
        {
            Assert.Less(EditorApplication.timeSinceStartup, end, msg);
            if (EditorApplication.isPaused)
                EditorApplication.isPaused = false;
            if (KingSlime.Instance != null)
                KingSlime.Instance.AddHp(100000, false);
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator Single_RecordsBreakTimes_ThenBackupAndUpload()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return SingleBody();
        AssertNoErrors();
    }

    IEnumerator SingleBody()
    {
        PlayRecordStore.ForceFake = true;
        FakePlayRecordStore.Clear();
        PlayRecordBackup.ClearForTest();
        try
        {
            yield return WaitFor(() => PlayRecorder.Current != null && PlayRecorder.Current.frames.Count >= 1, "첫 브레이크타임 기록 없음");
            Assert.AreEqual(PlayRecord.MODE_SINGLE, PlayRecorder.Current.mode);
            Assert.AreEqual(1, PlayRecorder.Current.frames[0].wave);
            yield return SkipWave();
            yield return WaitFor(() => PlayRecorder.Current.frames.Count >= 2, "다음 브레이크타임 기록 없음");
            yield return SkipWave();
            yield return WaitFor(() => PlayRecorder.Current.frames.Count >= 3, "세 번째 브레이크타임 기록 없음 (3장 이상이어야 올림)");
            var frame = PlayRecorder.Current.frames.Last();
            Assert.AreEqual(3, frame.wave);
            Assert.IsFalse(string.IsNullOrEmpty(frame.board), "판 배치 기록");
            Assert.Greater(frame.t, PlayRecorder.Current.frames[0].t, "시간 기록");
            Assert.Greater(frame.at, 1_700_000_000_000L, "브레이크타임 시작 서버 시각 (ms)");
            Assert.GreaterOrEqual(frame.at, PlayRecorder.Current.frames[0].at);

            ResultCanvas.Instance.OpenCanvas(); // 싱글 판 끝
            yield return WaitFor(() => FakePlayRecordStore.records.Count == 1, "서버에 안 올라감");
            Assert.IsNull(PlayRecorder.Current, "끝나면 진행 중 기록 없음");
            var backup = PlayRecordBackup.Load().entries;
            Assert.AreEqual(1, backup.Count, "로컬 백업");
            Assert.IsTrue(backup[0].uploaded, "올린 표시");
            PlayRecord uploaded = FakePlayRecordStore.records[0];
            Assert.AreEqual(3, uploaded.frames.Count);
            Assert.AreEqual(3, uploaded.finalWave);
        }
        finally
        {
            PlayRecordStore.ForceFake = null;
            PlayRecordBackup.ClearForTest();
        }
    }

    [UnityTest]
    public IEnumerator Multi_FillsWithAiReplay()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return MultiBody();
        AssertNoErrors();
    }

    IEnumerator MultiBody()
    {
        MultiMatchManager.ResetForTest();
        MultiMatchManager.ForceFakeBackend = true;
        PlayRecordStore.ForceFake = true;
        FakePlayRecordStore.Clear();
        for (int i = 0; i < 8; i++)
        {
            FakePlayRecordStore.owner = $"user{i}"; // 유저당 칸 수 제한이 있어 서로 다른 유저로
            PlayRecordStore.Instance.UploadAsync(FakePlayRecordStore.MakeSample(i, 0.5f)); // 0.5초마다 wave +1
        }
        FakePlayRecordStore.owner = "me";
        float prevInterval = FakeMultiBackend.botJoinInterval;
        FakeMultiBackend.botJoinInterval = 600f; // 사람(봇)이 안 들어옴
        MultiMatchManager.AiFillWaitForTest = 0.5f;
        try
        {
            MultiMatchManager mm = MultiMatchManager.Instance;
            mm.Join().Forget();
            yield return WaitFor(() => mm.Room != null && mm.Room.players.Count(p => p.ai) == MultiGame.ROOM_SIZE - 1, "AI 봇 7명으로 안 채움");
            Assert.IsTrue(mm.IsHost, "방장 = 나 (AI는 방장 X)");
            Assert.IsTrue(mm.Room.players.Where(p => p.ai).All(p => p.ready && p.aiSlot >= 0 && !string.IsNullOrEmpty(p.nickname)), "봇 = Ready + 기록 칸 + 닉네임");
            yield return WaitFor(() => MultiGame.IsActive && GameManager.Instance != null && GameManager.Instance.IsGameStarted, "AI로 채운 방이 시작 안 함");
            yield return WaitFor(() => MultiGame.Room.players.Any(p => p.ai && p.wave >= 3), "AI 봇이 기록대로 진행 안 함");
            MultiPlayerState bot = MultiGame.Room.players.First(p => p.ai && p.wave >= 3);
            Assert.IsFalse(string.IsNullOrEmpty(bot.board), "봇 판 배치 (보기)");
            Assert.IsFalse(string.IsNullOrEmpty(bot.minions), "봇 미니언");
        }
        finally
        {
            FakeMultiBackend.botJoinInterval = prevInterval;
            MultiMatchManager.AiFillWaitForTest = null;
            MultiMatchManager.ForceFakeBackend = null;
            PlayRecordStore.ForceFake = null;
            MultiMatchManager.ResetForTest();
            LocalGameSave.Clear();
            PlayRecordBackup.ClearForTest();
        }
    }
}

using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

// (2026-11-27) 플레이 기록 규칙: 서버 100칸 돌려쓰기 · 재생 장면 · AI 봇 기록 고르기 · AI는 방장 X
public class PlayRecordRuleTest
{
    [TestCase(1, 0)]
    [TestCase(100, 99)]
    [TestCase(101, 0)]   // 101번째 판 = 가장 오래된 1번째 자리
    [TestCase(205, 4)]
    public void Slot_RingOf100(long number, int slot) => Assert.AreEqual(slot, PlayRecordStore.SlotOf(number));

    [Test]
    public void FrameAt_LastPassedFrame()
    {
        var r = new PlayRecord();
        r.frames.Add(new PlayRecord.Frame { t = 0, wave = 1 });
        r.frames.Add(new PlayRecord.Frame { t = 10, wave = 2 });
        r.frames.Add(new PlayRecord.Frame { t = 25, wave = 3 });
        Assert.AreEqual(1, MultiGame.FrameAt(r, -5).wave, "시작 전 = 첫 장면");
        Assert.AreEqual(1, MultiGame.FrameAt(r, 9.9f).wave);
        Assert.AreEqual(2, MultiGame.FrameAt(r, 10f).wave);
        Assert.AreEqual(3, MultiGame.FrameAt(r, 100f).wave);
        Assert.IsNull(MultiGame.FrameAt(new PlayRecord(), 3f));
    }

    [Test]
    public void PickAiRecords_FiltersAndPrefersSameStage()
    {
        var all = new List<PlayRecordSummary>
        {
            new PlayRecordSummary { slot = 0, stageOrder = 2, difficulty = DifficultyType.Normal, frameCount = 10 },
            new PlayRecordSummary { slot = 1, stageOrder = MultiGame.MULTI_STAGE_ORDER, difficulty = MultiGame.MULTI_DIFFICULTY, frameCount = 10 },
            new PlayRecordSummary { slot = 3, stageOrder = MultiGame.MULTI_STAGE_ORDER, difficulty = MultiGame.MULTI_DIFFICULTY, frameCount = 2 },  // 너무 짧음 X
        };
        var picks = MultiGame.PickAiRecords(all, 5, new System.Random(1));
        CollectionAssert.AreEqual(new[] { 1, 0 }, picks.ConvertAll(p => p.slot), "같은 맵 먼저, 짧은 판 제외");
        Assert.AreEqual(1, MultiGame.PickAiRecords(all, 1, new System.Random(1)).Count);
    }

    // 100칸이 다 찬 뒤부터 유저당 최대 5칸: 다 차면 그 유저의 가장 오래된 칸을 덮어씀 · uid는 해시로만
    [Test]
    public void OwnSlot_CapPerUser()
    {
        var index = new List<PlayRecordSummary>();
        for (int i = 0; i < 10; i++)
            index.Add(new PlayRecordSummary { slot = 60 + i, owner = "a", uploadedAt = i });
        Assert.AreEqual(-1, PlayRecordStore.PickOwnSlot(index, "a"), "100칸이 다 차기 전엔 제한 없음 (10칸이어도 새 칸)");
        index.Clear();
        for (int i = 0; i < PlayRecordStore.MAX_RECORDS - PlayRecordStore.MAX_PER_USER; i++)
            index.Add(new PlayRecordSummary { slot = 100 + i, owner = "x" + i, uploadedAt = 1000 + i }); // 다른 유저로 채움
        for (int i = 0; i < PlayRecordStore.MAX_PER_USER - 1; i++)
            index.Add(new PlayRecordSummary { slot = 10 + i, owner = "a", uploadedAt = 100 + i });
        index.Add(new PlayRecordSummary { slot = 50, owner = "b", uploadedAt = 1 });
        Assert.AreEqual(-1, PlayRecordStore.PickOwnSlot(index, "a"), "100칸 다 찼어도 4칸이면 새 칸");
        index.Add(new PlayRecordSummary { slot = 20, owner = "a", uploadedAt = 50 });
        Assert.AreEqual(20, PlayRecordStore.PickOwnSlot(index, "a"), "5칸이면 그 유저의 가장 오래된 칸");
        Assert.AreEqual(-1, PlayRecordStore.PickOwnSlot(index, "b"));
        string h = PlayRecordStore.OwnerHash("uid-123");
        Assert.AreEqual(16, h.Length);
        Assert.AreNotEqual("uid-123", h);
        Assert.AreEqual(h, PlayRecordStore.OwnerHash("uid-123"), "같은 uid = 같은 해시");
    }

    [Test]
    public void FakeStore_PerUserCap_AndShortSkip()
    {
        FakePlayRecordStore.Clear();
        FakePlayRecordStore.owner = "spam";
        var store = new FakePlayRecordStore();
        for (int i = 0; i < PlayRecordStore.MAX_RECORDS + 20; i++)
            store.UploadAsync(FakePlayRecordStore.MakeSample(i % 4));
        Assert.AreEqual(PlayRecordStore.MAX_RECORDS, FakePlayRecordStore.index.Count, "100칸 다 차기 전엔 한 유저도 계속 새 칸 → 100칸");
        FakePlayRecordStore.owner = "other";
        for (int i = 0; i < 8; i++)
            store.UploadAsync(FakePlayRecordStore.MakeSample(i % 4));
        Assert.AreEqual(PlayRecordStore.MAX_PER_USER, FakePlayRecordStore.index.Values.Count(v => v.owner == "other"), "다 찬 뒤엔 한 유저 5칸까지");
        var shortRecord = new PlayRecord();
        shortRecord.frames.Add(new PlayRecord.Frame());
        FakePlayRecordStore.owner = "other";
        store.UploadAsync(shortRecord);
        Assert.AreEqual(PlayRecordStore.MAX_PER_USER, FakePlayRecordStore.index.Values.Count(v => v.owner == "other"), "장면 3장 미만은 안 올림");
        FakePlayRecordStore.Clear();
        FakePlayRecordStore.owner = "me";
    }

    // 남기는 판 = 스테이지 1 보통만
    [Test]
    public void Record_OnlyStage1Normal()
    {
        Assert.IsTrue(PlayRecorder.ShouldRecord(StageData.FIRST_STAGE_ORDER, DifficultyType.Normal));
        Assert.IsFalse(PlayRecorder.ShouldRecord(StageData.FIRST_STAGE_ORDER, DifficultyType.Hard));
        Assert.IsFalse(PlayRecorder.ShouldRecord(StageData.FIRST_STAGE_ORDER + 1, DifficultyType.Normal));
    }

    [Test]
    public void Host_NeverAi()
    {
        var players = new List<MultiPlayerState>
        {
            new MultiPlayerState { uid = "ai_1", joinedAt = 1, ai = true },
            new MultiPlayerState { uid = "me", joinedAt = 5 },
        };
        Assert.AreEqual("me", MultiGame.PickHost(players), "AI 봇은 방장이 안 됨");
    }
}

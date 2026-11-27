using NUnit.Framework;
using UnityEngine;

// (2026-11-27) 멀티 규칙: 카운트다운 시작 시각 (10초, Ready 1명당 1초) · 남은 초 · 방이 찼는지 · 판 저장 JSON
public class MultiRuleTest
{
    [TestCase(0, 10)]
    [TestCase(3, 7)]
    [TestCase(8, 2)]
    [TestCase(20, 2)] // 인원보다 많아도 8명까지
    public void StartAt_ReadyReduces(int ready, int seconds)
    {
        long fullAt = 1_000_000;
        Assert.AreEqual(fullAt + MultiGame.PRE_START_MS + MultiGame.LOAD_BUFFER_MS + seconds * 1000L, MultiGame.GetStartAtMs(fullAt, ready));
    }

    [Test]
    public void RemainSeconds()
    {
        Assert.AreEqual(10, MultiGame.GetRemainSeconds(20_000, 10_000));
        Assert.AreEqual(10, MultiGame.GetRemainSeconds(30_000, 10_000), "최대 10초");
        Assert.AreEqual(1, MultiGame.GetRemainSeconds(10_001, 10_000), "올림");
        Assert.AreEqual(0, MultiGame.GetRemainSeconds(10_000, 10_000));
        Assert.AreEqual(0, MultiGame.GetRemainSeconds(5_000, 10_000));
    }

    [Test]
    public void Room_IsFull_NeedsEightAndFullAt()
    {
        var room = new MultiRoomState();
        for (int i = 0; i < MultiGame.ROOM_SIZE; i++)
            room.players.Add(new MultiPlayerState { uid = $"u{i}", ready = i < 3 });
        Assert.IsFalse(room.IsFull, "fullAt이 없으면 아직");
        room.fullAtMs = 5000;
        Assert.IsTrue(room.IsFull);
        Assert.AreEqual(3, room.ReadyCount);
        Assert.IsTrue(room.IsReady("u0"));
        Assert.IsFalse(room.IsReady("u7"));
    }

    // 판 저장은 파일(JsonUtility)로 남아야 함 (이어하기)
    [Test]
    public void Snapshot_JsonRoundTrip()
    {
        var s = new GameSnapshot { nextWaveIdx = 7, lv = 3, exp = 2, diceCoin = 4, hasSlimeTile = true, slimeTile = new Vector2Int(2, 3) };
        s.minions.Add(new GameSnapshot.MinionSnap { tile = new Vector2Int(1, 1), key = "Flame", mergeLevel = 2, hasRotation = true, rotation = RotationDirection.Clockwise });
        s.runes.Add(new GameSnapshot.RuneSnap { tile = new Vector2Int(0, 0), key = "AttackPowerTile", level = 2 });
        s.abilities.Add(new GameSnapshot.AbilitySnap { dataKey = "HeroKnight", level = 1, unlockedSkills = 1, heroOffsets = { new Vector2Int(-1, 0) } });
        s.enhances.Add(new GameSnapshot.EnhanceSnap { key = "Flame", stat = StatType.AttackPower, lv = 2 });
        var back = JsonUtility.FromJson<GameSnapshot>(JsonUtility.ToJson(s));
        Assert.AreEqual(7, back.nextWaveIdx);
        Assert.AreEqual(new Vector2Int(2, 3), back.slimeTile);
        Assert.AreEqual("Flame", back.minions[0].key);
        Assert.AreEqual(RotationDirection.Clockwise, back.minions[0].rotation);
        Assert.AreEqual(2, back.runes[0].level);
        Assert.AreEqual("HeroKnight", back.abilities[0].dataKey);
        Assert.AreEqual(new Vector2Int(-1, 0), back.abilities[0].heroOffsets[0]);
        Assert.AreEqual(StatType.AttackPower, back.enhances[0].stat);
    }

    // AI로 채우기까지 남은 시간 (방 생성부터 300초) · 방이 차면 5초 뒤 입장 · AI 이름 [AI]
    [Test]
    public void AiFill_Remain_And_EnterAt()
    {
        Assert.AreEqual(300f, MultiGame.AI_FILL_WAIT_SEC);
        Assert.AreEqual(300f, MultiGame.GetAiFillRemain(10_000, 10_000, 300f));
        Assert.AreEqual(100f, MultiGame.GetAiFillRemain(10_000, 210_000, 300f), 0.001f);
        Assert.AreEqual(0f, MultiGame.GetAiFillRemain(10_000, 999_000, 300f));
        Assert.AreEqual(15_000, MultiGame.GetEnterAtMs(10_000), "방이 차면 5초 뒤");
        StringAssert.Contains("[AI]", MultiGame.DisplayName(new MultiPlayerState { nickname = "봇", ai = true }));
        Assert.AreEqual("사람", MultiGame.DisplayName(new MultiPlayerState { nickname = "사람" }));
    }

    // 방은 시작(차기 전이면 생성) 5시간 뒤 지움
    [Test]
    public void Room_ExpiresAfterFiveHours()
    {
        long h = 60L * 60 * 1000;
        Assert.IsFalse(MultiGame.IsExpired(0, 10 * h, 14 * h), "시작 4시간");
        Assert.IsTrue(MultiGame.IsExpired(0, 10 * h, 15 * h + 1), "시작 5시간 넘음");
        Assert.IsTrue(MultiGame.IsExpired(10 * h, 0, 15 * h + 1), "안 찬 방은 생성 기준");
        Assert.IsFalse(MultiGame.IsExpired(10 * h, 14 * h, 15 * h + 1), "시작이 있으면 시작 기준");
    }

    // 순위: 도착(빠른 순) → 높은 wave → 같으면 진행 중 > 늦게 탈락
    [Test]
    public void Rank_Order()
    {
        var players = new System.Collections.Generic.List<MultiPlayerState>
        {
            new MultiPlayerState { uid = "playing30", wave = 30 },
            new MultiPlayerState { uid = "reachedLate", wave = 51, reachedAt = 200, ended = true },
            new MultiPlayerState { uid = "out30early", wave = 30, ended = true, endedAt = 50 },
            new MultiPlayerState { uid = "reachedFast", wave = 51, reachedAt = 100, ended = true },
            new MultiPlayerState { uid = "out30late", wave = 30, ended = true, endedAt = 80 },
            new MultiPlayerState { uid = "playing40", wave = 40 },
        };
        CollectionAssert.AreEqual(new[] { "reachedFast", "reachedLate", "playing40", "playing30", "out30late", "out30early" },
            System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(MultiGame.Rank(players), p => p.uid)));
    }

    [Test]
    public void Minions_FormatParse()
    {
        string text = MultiGame.FormatMinions(new[] { ("Flame", 2), ("Random", 3), ("Random", 1) });
        Assert.AreEqual("Random:3,Flame:2,Random:1", text, "레벨 높은 순");
        var back = MultiGame.ParseMinions(text);
        Assert.AreEqual(3, back.Count);
        Assert.AreEqual(("Random", 3), back[0]);
        Assert.AreEqual(0, MultiGame.ParseMinions("").Count);
    }

    // 방장 = 접속 중 가장 먼저 들어온 사람, 나가면(오프라인) 다음 사람
    [Test]
    public void Host_FirstOnline()
    {
        var players = new System.Collections.Generic.List<MultiPlayerState>
        {
            new MultiPlayerState { uid = "b", joinedAt = 20 },
            new MultiPlayerState { uid = "a", joinedAt = 10 },
            new MultiPlayerState { uid = "c", joinedAt = 30 },
        };
        Assert.AreEqual("a", MultiGame.PickHost(players));
        players[1].online = false;
        Assert.AreEqual("b", MultiGame.PickHost(players), "방장이 나가면 두 번째로 들어온 사람");
        players.RemoveAt(0);
        Assert.AreEqual("c", MultiGame.PickHost(players));
    }

    [TestCase("1234567", true)]
    [TestCase("0123456", false)]
    [TestCase("123456", false)]
    [TestCase("12345a7", false)]
    public void Code_Valid(string code, bool ok) => Assert.AreEqual(ok, MultiGame.IsValidCode(code));
}

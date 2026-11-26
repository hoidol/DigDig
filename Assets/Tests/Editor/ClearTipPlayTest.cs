using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-26) 로비 전투 화면 하단 클리어 팁 띠지 + 결과 화면 팁 입력 (서버 대신 FetchForTest/UploadForTest)
public class ClearTipPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Ticker_And_Upload()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(LOBBY_SCENE_PATH); yield return new EnterPlayMode(); }
        yield return WaitLobbyStart();
        try { yield return Body(); }
        finally
        {
            ClearTipManager.FetchForTest = null;
            ClearTipManager.UploadForTest = null;
            ClearTipManager.ClearCacheForTest();
            ClearTipTicker.PinnedShownThisRun = false;
        }
        AssertNoErrors();
    }

    static float ScreenMinY(RectTransform rt) { var c = new Vector3[4]; rt.GetWorldCorners(c); return c[0].y; }
    static float ScreenMaxY(RectTransform rt) { var c = new Vector3[4]; rt.GetWorldCorners(c); return c[1].y; }

    IEnumerator Body()
    {
        var server = new Dictionary<string, List<ClearTip>>();
        ClearTipManager.FetchForTest = key => server.TryGetValue(key, out var l) ? l.ToList() : new List<ClearTip>();
        ClearTipManager.UploadForTest = (key, tip) =>
        {
            if (!server.TryGetValue(key, out var l)) server[key] = l = new List<ClearTip>();
            l.RemoveAll(t => t.uid == "me");
            tip.uid = "me";
            tip.updatedAt = 100;
            l.Add(tip);
            return true;
        };
        string key = ClearTipManager.CurrentStageKey;
        server[key] = new List<ClearTip>
        {
            new ClearTip { uid = "u1", nickname = "슬라임왕", text = "얼음을 앞에 두세요", updatedAt = 2 },
            new ClearTip { uid = "u2", nickname = "Guest#ab12", text = "시발 보스 어려움", updatedAt = 1 },
        };

        Lobby.LobbyManager.Instance.OpenCanvas(LobbyState.Battle);
        yield return WaitFrames(3);
        var battle = Object.FindFirstObjectByType<BattleCanvas>(FindObjectsInactive.Include);
        Assert.IsTrue(battle != null && battle.gameObject.activeInHierarchy);
        var ticker = battle.clearTipTicker;
        Assert.IsNotNull(ticker, "띠지");
        // 하단 메뉴 바(더 위 레이어)에 가리지 않음 + Play·난이도 버튼과 안 겹침 (화면 좌표)
        var bar = GameObject.Find("LobbyStateButtons_Hor")?.GetComponent<RectTransform>();
        float bandBottom = ScreenMinY(ticker.band), bandTop = ScreenMaxY(ticker.band);
        if (bar != null)
            Assert.GreaterOrEqual(bandBottom, ScreenMaxY(bar) - 0.5f, "띠지가 하단 메뉴 바에 가려짐");
        var diff = battle.GetComponentInChildren<DifficultyDisplayButton>(true);
        if (diff != null)
            Assert.LessOrEqual(bandTop, ScreenMinY((RectTransform)diff.transform) + 0.5f, "띠지가 난이도 버튼과 겹침");
        ticker.Refresh(true);
        yield return WaitFrames(3);
        Assert.AreEqual(2, ticker.Tips.Count);
        Assert.AreEqual("슬라임왕 : 얼음을 앞에 두세요", ticker.CurrentText, "최신 팁 먼저, 닉네임 : 팁");
        ticker.Next();
        yield return WaitRealtime(0.6f);
        Assert.AreEqual("Guest#ab12 : ** 보스 어려움", ticker.CurrentText, "누르면 다음 팁 + 금칙어 ***");

        // (2026-11-26) 고정 팁(pinned)은 앱을 켤 때마다 처음 한 번만 → 넘기면 유저 팁만, 다시 불러와도 안 나옴
        ClearTipTicker.PinnedShownThisRun = false;
        server[key].Add(new ClearTip { uid = "admin_1", nickname = "운영자", text = "고정 팁", updatedAt = 0, pinned = true });
        ClearTipManager.ClearCacheForTest();
        ticker.Refresh(true);
        yield return WaitFrames(3);
        Assert.AreEqual("운영자 : 고정 팁", ticker.CurrentText, "처음엔 고정 팁이 첫 줄");
        ticker.Next();
        yield return WaitRealtime(0.6f);
        Assert.IsTrue(ClearTipTicker.PinnedShownThisRun);
        Assert.IsFalse(ticker.Tips.Any(t => t.pinned), "한 번 보여 준 뒤엔 목록에서 뺌");
        Assert.AreEqual("슬라임왕 : 얼음을 앞에 두세요", ticker.CurrentText, "다음 = 유저 팁");
        ticker.Refresh(true);
        yield return WaitFrames(3);
        Assert.IsFalse(ticker.Tips.Any(t => t.pinned), "다시 불러와도 이번 실행에선 고정 팁 없음");

        // 팁이 없는 스테이지 = 안내 문구
        server.Remove(key);
        ClearTipManager.ClearCacheForTest();
        ticker.Refresh(true);
        yield return WaitFrames(3);
        Assert.AreEqual(TranslateManager.GetText("ClearTip_Empty"), ticker.CurrentText);

        // 팁 입력: 클리어하지 않았으면 못 올림, 클리어하면 올리고 띠지에 보임
        var input = new GameObject("TipInputTest", typeof(RectTransform)).AddComponent<ClearTipInputPanel>();
        input.transform.SetParent(battle.transform, false);
        input.BuildUI(); // 실제 화면은 InGame 씬 ResultCanvas에 빌더로 만들어 둠 (여기선 같은 모양을 임시로)
        UserDataManager.Instance.userStageManager.ClearStage(key);
        Assert.IsTrue(ClearTipManager.CanWrite(key), "클리어하면 팁 자격");
        input.Show(key);
        input.inputField.text = "  킹을 가운데에  ";
        input.OnClickedSubmit();
        yield return WaitFrames(3);
        Assert.IsTrue(input.Submitted, "등록 완료");
        Assert.IsFalse(input.inputField.interactable, "등록 후 입력 잠김");
        ticker.Refresh(true);
        yield return WaitFrames(3);
        StringAssert.EndsWith(": 킹을 가운데에", ticker.CurrentText, "올린 팁이 띠지에 (다른 팁이 없으면 첫 줄)");

        // 다른 팁이 있으면 내 팁은 2번째 줄
        server[key].Add(new ClearTip { uid = "u9", nickname = "고수", text = "보스 전엔 룬을", updatedAt = 999 });
        ClearTipManager.ClearCacheForTest();
        ClearTipManager.UploadAsync(key, "킹을 가운데에").Forget(); // 앱을 켠 동안 내 팁 기억 (캐시 초기화로 지워져서 다시)
        yield return WaitFrames(2);
        ticker.Refresh(true);
        yield return WaitFrames(3);
        Assert.AreEqual(2, ticker.Tips.Count);
        Assert.AreEqual("고수 : 보스 전엔 룬을", ticker.CurrentText, "첫 줄 = 다른 사람 팁");
        StringAssert.EndsWith(": 킹을 가운데에", ticker.Tips[1].DisplayText, "내 팁 = 2번째");
        Object.Destroy(input.gameObject);
    }
}

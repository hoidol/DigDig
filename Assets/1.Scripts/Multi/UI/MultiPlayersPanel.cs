using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 멀티 판 중 오른쪽 위 다른 유저 상황 (MultiGameUI 씬): 순위 순 7명 (나 제외), 접기 버튼, [보기] → MultiSpectatePanel
// 줄 = 씬의 템플릿(rowTemplate) 복사. 게임이 시작되면 보임
public class MultiPlayersPanel : MonoBehaviour
{
    public GameObject listRoot;          // 접으면 숨기는 부분
    public Button toggleButton;          // 접기·펴기
    public TMP_Text toggleText;          // "참가자 ▲/▼" (선택)
    public TMP_Text myRankText;          // "내 순위 3/8" (선택)
    public MultiStatusRow rowTemplate;
    public Transform rowContainer;
    public MultiSpectatePanel spectatePanel;

    readonly List<MultiStatusRow> rows = new List<MultiStatusRow>();
    bool open = true;
    bool shown;

    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(() => { open = !open; ApplyOpen(); });
        gameObject.SetActive(MultiGame.IsActive);
        if (MultiMatchManager.Exists)
            MultiMatchManager.Instance.RoomChanged += Refresh;
        ApplyOpen();
        Refresh(MultiGame.Room);
    }

    void OnDestroy()
    {
        if (MultiMatchManager.Exists)
            MultiMatchManager.Instance.RoomChanged -= Refresh;
    }

    void Update()
    {
        // 카운트다운이 끝나 게임이 시작되면 보임
        bool show = MultiGame.IsActive && MultiGame.CanStart;
        if (show != shown)
        {
            shown = show;
            if (listRoot != null && open) listRoot.SetActive(show);
            if (toggleButton != null) toggleButton.gameObject.SetActive(show);
            if (myRankText != null) myRankText.gameObject.SetActive(show);
        }
    }

    void ApplyOpen()
    {
        if (listRoot != null)
            listRoot.SetActive(open && shown);
        if (toggleText != null)
            toggleText.text = $"{TranslateManager.GetText("Multi_Players")} {(open ? "▲" : "▼")}";
    }

    void Refresh(MultiRoomState room)
    {
        if (room == null)
            return;
        if (rowTemplate == null)
        {
            Debug.LogWarning("[MultiPlayersPanel] rowTemplate 없음 — 메뉴 Tools/Multi/Create MultiGameUI Scene");
            return;
        }
        string me = MultiMatchManager.Exists ? MultiMatchManager.Instance.MyUid : null;
        List<MultiPlayerState> ranked = MultiGame.Rank(room.players);
        List<(int rank, MultiPlayerState p)> others = ranked.Select((p, i) => (i + 1, p)).Where(x => x.p.uid != me).ToList();
        rowTemplate.gameObject.SetActive(false);
        Transform parent = rowContainer != null ? rowContainer : rowTemplate.transform.parent;
        while (rows.Count < others.Count)
            rows.Add(Instantiate(rowTemplate, parent));
        for (int i = 0; i < rows.Count; i++)
        {
            bool active = i < others.Count;
            rows[i].gameObject.SetActive(active);
            if (active)
                rows[i].Set(others[i].rank, others[i].p, OnView);
        }
        if (myRankText != null)
        {
            int myRank = ranked.FindIndex(p => p.uid == me) + 1;
            myRankText.text = string.Format(TranslateManager.GetText("Multi_MyRank"), myRank, ranked.Count);
        }
        if (spectatePanel != null)
            spectatePanel.Refresh(room);
    }

    void OnView(string uid)
    {
        if (spectatePanel == null)
        {
            Debug.LogWarning("[MultiPlayersPanel] spectatePanel 없음 — 메뉴 Tools/Multi/Create MultiGameUI Scene");
            return;
        }
        spectatePanel.Open(uid);
    }
}

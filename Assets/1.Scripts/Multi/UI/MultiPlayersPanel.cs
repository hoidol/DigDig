using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 멀티 판 중 오른쪽 위 다른 유저 상황 (MultiGameUI 씬): 방에 들어온 순서로 7명 (나 제외, 순위 표시 없음), 접기 버튼, [보기] → MultiSpectatePanel
// 줄 = 씬의 템플릿(rowTemplate) 복사. 게임이 시작되면 보임
public class MultiPlayersPanel : MonoBehaviour
{
    public GameObject listRoot;          // 접으면 숨기는 부분
    public Button toggleButton;          // 접기·펴기
    public TMP_Text toggleText;          // "참가자 ▲/▼" (선택)
    public TMP_Text myRankText;          // (2026-11-27) 순위 표시 삭제 → 씬에 남아 있으면 숨김 (메뉴 Tools/Multi/Remove Rank From Players Panel)
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
            if (myRankText != null) myRankText.gameObject.SetActive(false);
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
        // (2026-11-27) 순위 대신 방에 들어온 순서 (joinedAt)
        List<MultiPlayerState> others = room.players.Where(p => p != null && p.uid != me)
            .OrderBy(p => p.joinedAt).ThenBy(p => p.uid).ToList();
        rowTemplate.gameObject.SetActive(false);
        Transform parent = rowContainer != null ? rowContainer : rowTemplate.transform.parent;
        while (rows.Count < others.Count)
            rows.Add(Instantiate(rowTemplate, parent));
        for (int i = 0; i < rows.Count; i++)
        {
            bool active = i < others.Count;
            rows[i].gameObject.SetActive(active);
            if (active)
                rows[i].Set(others[i], OnView);
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

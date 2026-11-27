using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 멀티 시작 전 Ready 창 (MultiGameUI 씬, 메뉴 Tools/Multi/Create MultiGameUI Scene)
// 8명 닉네임 + Ready 표시 + 카운트다운(10초, Ready 1명당 1초 감소, 서버 시각 기준이라 모두 같은 순간에 끝남) → 0이면 창을 닫고 게임 시작 (MultiGame.CanStart)
public class MultiReadyPanel : MonoBehaviour
{
    public GameObject root;            // 창 전체 (비우면 이 오브젝트)
    public TMP_Text countdownText;     // 남은 초
    public TMP_Text titleText;         // "곧 시작합니다" (선택)
    public Button readyButton;
    public TMP_Text readyButtonText;   // "READY" / 누른 뒤 "준비 완료"
    public MultiPlayerRow rowTemplate; // 꺼 둔 템플릿 1개 → 인원수만큼 복사
    public Transform rowContainer;

    readonly List<MultiPlayerRow> rows = new List<MultiPlayerRow>();
    bool pressed;

    GameObject Root => root != null ? root : gameObject;

    void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(OnClickedReady);
        if (!MultiGame.IsActive)
        {
            Root.SetActive(false);
            return;
        }
        Root.SetActive(true);
        if (MultiMatchManager.Exists)
            MultiMatchManager.Instance.RoomChanged += OnRoomChanged;
        OnRoomChanged(MultiGame.Room);
    }

    void OnDestroy()
    {
        if (MultiMatchManager.Exists)
            MultiMatchManager.Instance.RoomChanged -= OnRoomChanged;
    }

    void OnRoomChanged(MultiRoomState room)
    {
        if (room == null)
            return;
        if (rowTemplate == null)
        {
            Debug.LogWarning("[MultiReadyPanel] rowTemplate 없음 — 메뉴 Tools/Multi/Create MultiGameUI Scene");
            return;
        }
        rowTemplate.gameObject.SetActive(false);
        Transform parent = rowContainer != null ? rowContainer : rowTemplate.transform.parent;
        while (rows.Count < room.players.Count)
            rows.Add(Instantiate(rowTemplate, parent));
        string me = MultiMatchManager.Exists ? MultiMatchManager.Instance.MyUid : null;
        for (int i = 0; i < rows.Count; i++)
        {
            bool active = i < room.players.Count;
            rows[i].gameObject.SetActive(active);
            if (active)
                rows[i].Set(room.players[i], room.players[i].uid == me, room.players[i].uid == room.HostUid);
        }
        UpdateReadyButton(room.IsReady(me));
    }

    void Update()
    {
        if (!MultiGame.IsActive || MultiGame.CanStart)
            return;
        MultiRoomState room = MultiGame.Room;
        if (room == null || room.fullAtMs <= 0)
            return;
        long now = MultiMatchManager.Exists ? MultiMatchManager.Instance.ServerNowMs : System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int remain = MultiGame.GetRemainSeconds(room.StartAtMs, now);
        if (countdownText != null)
            countdownText.text = remain.ToString();
        if (now >= room.StartAtMs)
        {
            MultiGame.CanStart = true;
            Root.SetActive(false);
        }
    }

    void OnClickedReady()
    {
        if (pressed)
            return;
        pressed = true;
        UpdateReadyButton(true);
        SoundManager.Play(SFXType.TabChange);
        MultiMatchManager.Instance.SetReady();
    }

    void UpdateReadyButton(bool ready)
    {
        pressed |= ready;
        if (readyButton != null)
            readyButton.interactable = !pressed;
        if (readyButtonText != null)
            readyButtonText.text = TranslateManager.GetText(pressed ? "Multi_ReadyDone" : "Multi_Ready");
    }
}

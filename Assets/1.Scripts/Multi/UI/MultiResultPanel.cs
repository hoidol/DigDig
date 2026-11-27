using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 멀티 결과 창 (MultiGameUI 씬): 내 판이 끝나면 (클리어 또는 부활 포기) 순위 8명 + 내 결과 (우승!/클리어!/게임 종료 — 탈락은 화면에 안 씀) + 전적(우승·클리어)
// 다른 사람은 계속하므로 창이 열려 있는 동안 순위가 바뀜 (방 구독). [로비로] = 멀티 나가기. 싱글 판 저장본은 로비 [이어하기]
public class MultiResultPanel : MonoBehaviour
{
    public GameObject root;
    public TMP_Text titleText;    // 우승! / 클리어! / 탈락
    public TMP_Text recordText;   // "우승 3 · 클리어 5"
    public MultiResultRow rowTemplate;
    public Transform rowContainer;
    public Button lobbyButton;

    readonly List<MultiResultRow> rows = new List<MultiResultRow>();
    MultiInGameSession session;

    GameObject Root => root != null ? root : gameObject;

    void Awake()
    {
        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(() => MultiMatchManager.Instance.ExitMultiGame());
    }

    void Start()
    {
        if (session == null)
            Root.SetActive(false);
    }

    void OnDestroy()
    {
        if (MultiMatchManager.Exists)
            MultiMatchManager.Instance.RoomChanged -= Refresh;
    }

    public void Show(MultiInGameSession s)
    {
        session = s;
        Root.SetActive(true);
        FindFirstObjectByType<MultiSpectatePanel>(FindObjectsInactive.Include)?.Close();
        if (titleText != null)
            titleText.text = TranslateManager.GetText(s.IsChampion ? "Multi_ResultChampion" : s.Reached ? "Multi_ResultClear" : "Multi_ResultEnd");
        UserData user = UserDataManager.Instance.userData;
        if (recordText != null)
            recordText.text = string.Format(TranslateManager.GetText("Multi_Record"), user.multiChampion, user.multiClear);
        SoundManager.Play(s.Reached ? SFXType.RewardGet : SFXType.Fail_NotEnough);
        MultiMatchManager.Instance.RoomChanged -= Refresh;
        MultiMatchManager.Instance.RoomChanged += Refresh;
        Refresh(MultiGame.Room);
    }

    void Refresh(MultiRoomState room)
    {
        if (room == null || rowTemplate == null)
            return;
        string me = MultiMatchManager.Instance.MyUid;
        List<MultiPlayerState> ranked = MultiGame.Rank(room.players);
        rowTemplate.gameObject.SetActive(false);
        Transform parent = rowContainer != null ? rowContainer : rowTemplate.transform.parent;
        while (rows.Count < ranked.Count)
            rows.Add(Instantiate(rowTemplate, parent));
        for (int i = 0; i < rows.Count; i++)
        {
            bool active = i < ranked.Count;
            rows[i].gameObject.SetActive(active);
            if (active)
                rows[i].Set(i + 1, ranked[i], ranked[i].uid == me, ranked[i].uid == room.winnerUid, room.StartAtMs);
        }
    }
}

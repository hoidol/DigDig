using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 보기 화면 (MultiGameUI 씬): 다른 유저의 판 배치(마지막 브레이크타임·wave 시작 때 올린 것) + wave·슬라임·미니언
// 내 게임은 그대로 돌아가고 메인 카메라만 끔 (화면 = 이 창). 닫으면 카메라 다시 켬
// 칸 배치 = 내 맵(멀티는 모두 같은 스테이지)의 타일 위치를 창 크기에 맞춰 줄임, 칸 = 씬의 템플릿(cellTemplate) 복사
public class MultiSpectatePanel : MonoBehaviour
{
    public GameObject root;            // 창 전체 (비우면 이 오브젝트)
    public TMP_Text nicknameText;
    public TMP_Text infoText;          // "W23 · Lv.5 기사 · 진행 중"
    public TMP_Text minionsText;
    public RectTransform boardRoot;    // 칸이 놓일 영역
    public MultiSpectateCell cellTemplate;
    public Button closeButton;

    readonly List<MultiSpectateCell> cells = new List<MultiSpectateCell>();
    string targetUid;
    Camera hiddenCamera;

    GameObject Root => root != null ? root : gameObject;
    public bool IsOpen => Root.activeSelf && targetUid != null;
    public string TargetUid => targetUid;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    void Start()
    {
        if (targetUid == null)
            Root.SetActive(false);
    }

    void OnDestroy() => RestoreCamera();

    public void Open(string uid)
    {
        targetUid = uid;
        Root.SetActive(true);
        BuildCells();
        if (hiddenCamera == null && Camera.main != null)
        {
            hiddenCamera = Camera.main;
            hiddenCamera.enabled = false; // 내 판은 계속 돌고 그리기만 끔
        }
        Refresh(MultiGame.Room);
    }

    public void Close()
    {
        targetUid = null;
        Root.SetActive(false);
        RestoreCamera();
    }

    void RestoreCamera()
    {
        if (hiddenCamera != null)
            hiddenCamera.enabled = true;
        hiddenCamera = null;
    }

    public void Refresh(MultiRoomState room)
    {
        if (!IsOpen || room == null)
            return;
        MultiPlayerState p = room.Get(targetUid);
        if (p == null)
        {
            Close();
            return;
        }
        if (nicknameText != null) nicknameText.text = MultiGame.DisplayName(p);
        if (infoText != null) infoText.text = $"{MultiUiText.Wave(p)} · {MultiUiText.Slime(p)} · {MultiUiText.Status(p, room.StartAtMs)}";
        if (minionsText != null) minionsText.text = MultiUiText.Minions(p.minions, 12);
        MultiBoard board = null;
        if (!string.IsNullOrEmpty(p.board))
        {
            try { board = JsonUtility.FromJson<MultiBoard>(p.board); }
            catch (System.Exception) { board = null; }
        }
        var byIndex = new Dictionary<Vector2Int, MultiBoard.Cell>();
        if (board != null)
            foreach (MultiBoard.Cell c in board.cells)
                byIndex[new Vector2Int(c.x, c.y)] = c;
        foreach (MultiSpectateCell cell in cells)
            cell.Set(byIndex.TryGetValue(cell.Index, out MultiBoard.Cell c) ? c : null);
    }

    // 내 맵 타일 위치 → 창 안 위치 (한 번만)
    void BuildCells()
    {
        if (cells.Count > 0 || cellTemplate == null || boardRoot == null || TileManager.Instance == null)
        {
            if (cellTemplate == null || boardRoot == null)
                Debug.LogWarning("[MultiSpectatePanel] cellTemplate·boardRoot 없음 — 메뉴 Tools/Multi/Create MultiGameUI Scene");
            return;
        }
        cellTemplate.gameObject.SetActive(false);
        List<Tile> tiles = TileManager.Instance.tiles.Where(t => t != null).ToList();
        if (tiles.Count == 0)
            return;
        Vector2[] pos = tiles.Select(t => (Vector2)t.transform.position).ToArray();
        Vector2 min = new Vector2(pos.Min(v => v.x), pos.Min(v => v.y));
        Vector2 max = new Vector2(pos.Max(v => v.x), pos.Max(v => v.y));
        float spacing = MinSpacing(pos);
        Vector2 range = max - min + Vector2.one * spacing;
        Rect rect = boardRoot.rect;
        float scale = Mathf.Min(rect.width / Mathf.Max(range.x, 0.01f), rect.height / Mathf.Max(range.y, 0.01f));
        Vector2 center = (min + max) * 0.5f;
        float size = spacing * scale * 0.92f;
        for (int i = 0; i < tiles.Count; i++)
        {
            MultiSpectateCell cell = Instantiate(cellTemplate, boardRoot);
            cell.gameObject.SetActive(true);
            cell.Index = tiles[i].index;
            var rt = (RectTransform)cell.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = (pos[i] - center) * scale;
            rt.sizeDelta = new Vector2(size, size);
            cells.Add(cell);
        }
    }

    static float MinSpacing(Vector2[] pos)
    {
        float best = float.MaxValue;
        for (int i = 0; i < pos.Length; i++)
            for (int j = i + 1; j < pos.Length; j++)
            {
                float d = Vector2.Distance(pos[i], pos[j]);
                if (d > 0.01f && d < best)
                    best = d;
            }
        return best == float.MaxValue ? 1f : best;
    }
}

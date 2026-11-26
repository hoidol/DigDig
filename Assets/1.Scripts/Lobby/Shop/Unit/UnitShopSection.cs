using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-26) 상점 [용사]·[미니언] 줄 하나. 씬에 줄 + 칸 템플릿(UnitShopPanel) 1개를 두면 열 때 데이터 수만큼 템플릿을 복사해 채움
// 용사 = HeroData.csv(숨김 제외) / 미니언 = GameSetting.MINION_LEVEL_UNLOCKS 순서. 줄 높이 = 줄 수만큼 (rowContainer에 GridLayoutGroup)
// 씬에 만드는 메뉴: Tools/Lobby/Add Hero·Minion Shop Sections (UnitShopBuilder). 칸 모양·위치는 씬에서 템플릿을 고치면 됨
public class UnitShopSection : MonoBehaviour
{
    public UnitShopPanel.UnitKind kind;
    public UnitShopPanel template;      // 씬에 둔 칸 하나 (첫 칸으로도 씀)
    public RectTransform rowContainer;  // 칸이 놓이는 곳 (비우면 template의 부모)

    readonly List<UnitShopPanel> panels = new List<UnitShopPanel>();

    public List<string> Keys => kind == UnitShopPanel.UnitKind.Hero
        ? HeroCollectionContainer.ShownHeroes().Select(h => h.key).ToList()
        : GameSetting.MINION_LEVEL_UNLOCKS.Select(e => e.key)
            .Where(k => MinionManager.Instance == null || MinionManager.Instance.GetMinionData(k) != null).ToList();

    // ShopCanvas.Init에서 1번 (ProductContainer가 칸을 모으기 전에)
    public void Fill()
    {
        if (template == null)
        {
            Debug.LogWarning($"[UnitShopSection] {name}: 칸 템플릿 연결 없음");
            return;
        }
        RectTransform row = rowContainer != null ? rowContainer : (RectTransform)template.transform.parent;
        List<string> keys = Keys;
        if (panels.Count == 0)
            panels.Add(template);
        while (panels.Count < keys.Count)
            panels.Add(Instantiate(template, row));
        for (int i = 0; i < panels.Count; i++)
        {
            bool active = i < keys.Count;
            panels[i].gameObject.SetActive(active);
            if (active)
            {
                panels[i].name = $"{kind}_{keys[i]}";
                panels[i].Setup(kind, keys[i]);
            }
        }
        FitHeight(row, keys.Count);
    }

    // 줄 수만큼 높이 (부모 세로 배치용 LayoutElement 포함)
    void FitHeight(RectTransform row, int count)
    {
        var grid = row.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid == null || count <= 0)
            return;
        int cols = grid.constraint == UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount ? Mathf.Max(1, grid.constraintCount) : 3;
        int rows = Mathf.CeilToInt(count / (float)cols);
        float rowHeight = rows * grid.cellSize.y + (rows - 1) * grid.spacing.y + grid.padding.top + grid.padding.bottom;
        float extra = Mathf.Max(0f, ((RectTransform)transform).sizeDelta.y - row.sizeDelta.y);
        row.sizeDelta = new Vector2(row.sizeDelta.x, rowHeight);
        var rt = (RectTransform)transform;
        float height = rowHeight + extra;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        var le = GetComponent<UnityEngine.UI.LayoutElement>();
        if (le != null)
            le.preferredHeight = le.minHeight = height;
    }
}

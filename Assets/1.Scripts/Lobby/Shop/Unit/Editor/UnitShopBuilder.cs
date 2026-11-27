using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 상점에 [용사]·[미니언] 판매 줄을 Lobby 씬에 실제로 만듦 (에디터 메뉴, 한 번 실행 후 씬 저장)
// 모양 = 기존 다이아 상품 줄(섹션 제목 + Dia_SlotContainer)을 복사: 칸 틀·제목·그림·가격 버튼은 그대로, 다이아 상품 기능만 빼고 UnitShopPanel 템플릿 1개
// 자리 = 예전 미니언 뽑기 줄(Title_DailyDeals (1) + DrawMinion_SlotContainer (1), 스크립트 삭제됨)을 꺼 두고 그 자리. 이미 있으면 지우고 새로
// 실행 중엔 UnitShopSection이 템플릿을 데이터 수만큼 복사. 이후 모양은 씬에서 직접 수정
// (2026-11-27) 다이아·골드 상품 줄 삭제로 복사할 원본(Dia_SlotContainer)이 없음 → 이 메뉴는 원본이 없으면 아무것도 지우지 않고 멈춤. 지금 줄은 Hero_Scroll·Minion_Scroll 안에 있음
public static class UnitShopBuilder
{
    public const int COLUMN_COUNT = 3;
    const string DIA_SECTION = "Dia_SlotContainer";
    const string OLD_DRAW_SECTION = "DrawMinion_SlotContainer (1)";

    [MenuItem("Tools/Lobby/Add Hero·Minion Shop Sections")]
    public static void Build()
    {
        ShopCanvas shop = Object.FindFirstObjectByType<ShopCanvas>(FindObjectsInactive.Include);
        if (shop == null)
        {
            Debug.LogWarning("[UnitShopBuilder] ShopCanvas가 열린 씬에 없음 (Lobby 씬을 열고 실행)");
            return;
        }
        if (FindChild(shop.transform, DIA_SECTION) == null)
        {
            Debug.LogWarning("[UnitShopBuilder] 복사할 원본(Dia_SlotContainer)이 없어 멈춤 — 용사·미니언 줄은 씬에서 직접 수정");
            return;
        }
        foreach (UnitShopSection old in shop.GetComponentsInChildren<UnitShopSection>(true))
        {
            Transform oldTitle = PrevSibling(old.transform);
            if (oldTitle != null && oldTitle.name.StartsWith("Title_") && oldTitle.name.EndsWith("Shop"))
                Undo.DestroyObjectImmediate(oldTitle.gameObject);
            Undo.DestroyObjectImmediate(old.gameObject);
        }
        Transform dia = FindChild(shop.transform, DIA_SECTION);
        if (dia == null)
        {
            Debug.LogWarning("[UnitShopBuilder] 상점에 Dia_SlotContainer가 없음");
            return;
        }
        Transform contents = dia.parent;
        Transform diaTitle = PrevSibling(dia);
        int insertIdx = dia.GetSiblingIndex() - (diaTitle != null ? 1 : 0);
        Transform oldDraw = FindChild(shop.transform, OLD_DRAW_SECTION);
        if (oldDraw != null && oldDraw.parent == contents)
        {
            Transform oldTitle = PrevSibling(oldDraw);
            insertIdx = (oldTitle != null ? oldTitle : oldDraw).GetSiblingIndex();
            Undo.RecordObject(oldDraw.gameObject, "Hide old draw section");
            oldDraw.gameObject.SetActive(false);
            if (oldTitle != null)
            {
                Undo.RecordObject(oldTitle.gameObject, "Hide old draw section");
                oldTitle.gameObject.SetActive(false);
            }
        }

        insertIdx = AddSection(contents, diaTitle, dia, insertIdx, "Shop_Hero", UnitShopPanel.UnitKind.Hero);
        AddSection(contents, diaTitle, dia, insertIdx, "Shop_Minion", UnitShopPanel.UnitKind.Minion);
        EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);
        Debug.Log("[UnitShopBuilder] 상점 [용사]·[미니언] 줄 생성 (씬 저장은 직접)");
    }

    static int AddSection(Transform contents, Transform titleSrc, Transform sectionSrc, int insertIdx, string titleKey, UnitShopPanel.UnitKind kind)
    {
        if (titleSrc != null)
        {
            GameObject title = (GameObject)Object.Instantiate(titleSrc.gameObject, contents);
            Undo.RegisterCreatedObjectUndo(title, "Add shop section");
            title.name = $"Title_{kind}Shop";
            title.SetActive(true);
            title.transform.SetSiblingIndex(insertIdx++);
            TranslateText tt = title.GetComponentInChildren<TranslateText>(true);
            if (tt != null)
            {
                tt.key = titleKey;
                tt.formatArg = null;
                TMP_Text label = tt.text != null ? tt.text : tt.GetComponent<TMP_Text>();
                if (label != null)
                    label.text = TranslateManager.GetText(titleKey);
            }
        }

        GameObject section = (GameObject)Object.Instantiate(sectionSrc.gameObject, contents);
        Undo.RegisterCreatedObjectUndo(section, "Add shop section");
        section.name = $"{kind}Shop_SlotContainer";
        section.transform.SetSiblingIndex(insertIdx++);
        foreach (ProductContainer pc in section.GetComponents<ProductContainer>())
            Object.DestroyImmediate(pc);
        section.AddComponent<ProductContainer>();

        RectTransform row = section.transform.childCount > 0 ? (RectTransform)section.transform.GetChild(0) : (RectTransform)section.transform;
        ProductPanel[] oldPanels = row.GetComponentsInChildren<ProductPanel>(true);
        if (oldPanels.Length == 0)
            return insertIdx;
        GameObject template = oldPanels[0].gameObject;
        for (int i = 1; i < oldPanels.Length; i++)
            Object.DestroyImmediate(oldPanels[i].gameObject);
        UnitShopPanel templatePanel = ConvertPanel(template);
        templatePanel.kind = kind;
        templatePanel.name = $"{kind}_Template";
        Vector2 cell = ((RectTransform)template.transform).sizeDelta;

        foreach (LayoutGroup lg in row.GetComponents<LayoutGroup>())
            Object.DestroyImmediate(lg);
        GridLayoutGroup grid = row.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = COLUMN_COUNT;
        grid.cellSize = cell;
        grid.spacing = new Vector2(10f, 20f);
        grid.childAlignment = TextAnchor.UpperCenter;

        if (section.GetComponent<LayoutElement>() == null)
            section.AddComponent<LayoutElement>();
        UnitShopSection unitSection = section.AddComponent<UnitShopSection>();
        unitSection.kind = kind;
        unitSection.template = templatePanel;
        unitSection.rowContainer = row;
        return insertIdx;
    }

    // 다이아 상품 칸 → 용사·미니언 칸 템플릿: 제목·그림·가격 버튼은 그대로 쓰고 다이아 상품 기능만 뺌
    static UnitShopPanel ConvertPanel(GameObject go)
    {
        Transform titleTr = FindChild(go.transform, "Text_Title");
        TMP_Text title = titleTr != null ? titleTr.GetComponent<TMP_Text>() : null;
        if (title != null)
            foreach (TranslateText tt in title.GetComponents<TranslateText>())
                Object.DestroyImmediate(tt);
        Transform iconTr = FindChild(go.transform, "Icon");
        Image icon = iconTr != null ? iconTr.GetComponent<Image>() : null;
        ProductPurchaseButton ppb = go.GetComponentInChildren<ProductPurchaseButton>(true);
        Button button = ppb != null ? ppb.GetComponent<Button>() : go.GetComponentInChildren<Button>(true);
        CurrencyDisplayPanel price = go.GetComponentInChildren<CurrencyDisplayPanel>(true);
        TMP_Text priceText = ppb != null ? ppb.inAppPurchasePriceText : null;
        if (ppb != null)
            Object.DestroyImmediate(ppb);
        if (button != null)
        {
            // 다이아 구매 연결(씬에 저장된 onClick) 제거
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
        if (price == null && button != null)
            price = MakePriceDisplay(button, priceText);
        else if (priceText != null)
            priceText.gameObject.SetActive(false);
        foreach (ProductPanel p in go.GetComponents<ProductPanel>())
            Object.DestroyImmediate(p);
        if (price != null)
            price.gameObject.SetActive(true);

        UnitShopPanel panel = go.AddComponent<UnitShopPanel>();
        panel.titleText = title;
        panel.iconImage = icon;
        panel.buyButton = button;
        panel.priceDisplay = price;
        if (title != null)
            title.text = "이름";
        return panel;
    }

    // 가격 버튼 안: 왼쪽 재화 아이콘 + 기존 가격 글씨
    static CurrencyDisplayPanel MakePriceDisplay(Button button, TMP_Text priceText)
    {
        RectTransform brt = (RectTransform)button.transform;
        if (priceText == null)
            priceText = button.GetComponentInChildren<TMP_Text>(true);
        if (priceText == null)
        {
            var tgo = new GameObject("PriceText", typeof(RectTransform), typeof(TextMeshProUGUI));
            var trt = (RectTransform)tgo.transform;
            trt.SetParent(brt, false);
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            priceText = tgo.GetComponent<TextMeshProUGUI>();
            priceText.fontSize = 40f;
            priceText.alignment = TextAlignmentOptions.Center;
        }
        priceText.gameObject.SetActive(true);
        foreach (TranslateText tt in priceText.GetComponents<TranslateText>())
            Object.DestroyImmediate(tt);
        priceText.text = "0";
        var igo = new GameObject("CurrencyIcon", typeof(RectTransform), typeof(Image));
        var irt = (RectTransform)igo.transform;
        irt.SetParent(brt, false);
        irt.anchorMin = irt.anchorMax = new Vector2(0f, 0.5f);
        irt.pivot = new Vector2(0f, 0.5f);
        float size = Mathf.Max(40f, brt.rect.height * 0.6f);
        irt.sizeDelta = new Vector2(size, size);
        irt.anchoredPosition = new Vector2(16f, 0f);
        var iconImg = igo.GetComponent<Image>();
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;
        iconImg.sprite = CurrencyInfo.GetSprite(CurrencyType.Ad);
        var display = button.gameObject.AddComponent<CurrencyDisplayPanel>();
        display.currencyImage = iconImg;
        display.valueText = priceText;
        return display;
    }

    static Transform PrevSibling(Transform t)
    {
        int idx = t.GetSiblingIndex();
        return idx > 0 ? t.parent.GetChild(idx - 1) : null;
    }

    static Transform FindChild(Transform root, string name) =>
        root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
}

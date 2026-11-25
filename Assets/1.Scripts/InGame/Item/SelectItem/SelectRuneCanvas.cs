using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// SelectItem(랜덤 3개 중 선택, 화면 전체를 덮는 창) → SelectTile(타일을 눌러 설치)
// 아이템 하나 반드시 선택해야됌. 열려 있어도 게임은 멈추지 않음
// (2026-11-24) 뽑기 확률 연출(Draw 단계·DrawTileItemContainer·OpenDraw) 삭제: 룬은 엘리트·보스 보상·권능으로만 받음
public class SelectRuneCanvas : CanvasUI<SelectRuneCanvas>
{
    public const int SELECT_ITEM_COUNT = 3;

    public GameObject selectItemContainer; // 화면 전체를 덮음 (레벨업 창처럼)
    public GameObject selectTileContainer; // 타일을 눌러야 하므로 레이캐스트로 필드를 가리면 안 됨
    public SelectRunePanel[] selectItemPanels;
    public SelectRunePanel selectedPanel;
    [NonSerialized] public SelectItemStep step;
    public TMPro.TMP_Text selectTileGuideText; // 타일 선택 안내 글씨 (비우면 selectTileContainer 바로 아래 글씨)

    // (2026-11-25) 레벨업 룬 카드(OpenRuneTilePick) 삭제 → 룬은 엘리트·보스 보상(OpenRuneReward)만
    string defaultGuide;

    // (2026-11-23) 엘리트·보스 룬 보상 (RuneRewardSystem): 룬 3장 선택 → 칸 선택(레벨업 룬과 같은 규칙). 반드시 골라야 닫힘
    Action runeRewardDone;
    List<ItemData> runeRewardItems;
    bool finishing;
    public bool IsRuneRewardOpen => runeRewardDone != null;

    // (2026-11-23) 보상 상자: 엘리트·보스 처치 자리에 떨어지는 상자(RuneRewardChest)와 같은 그림을 룬 보상 창 제목 위에 보여 줌
    //   → "그 상자를 열어서 룬을 고르는 것"으로 이어짐. rewardChestRoot를 비우면 실행 중 selectItemContainer 제목 위에 기본 모양 생성
    public Sprite rewardChestSprite;
    public RectTransform rewardChestRoot;
    public UnityEngine.UI.Image rewardChestImage;
    public TMPro.TMP_Text rewardChestText;

    ItemData selectedItem;
    List<ItemData> shownItems = new(); // 지금 선택지 (갱신 시 겹치지 않게)
    public IReadOnlyList<ItemData> shownItemsForTest => shownItems;
    

    // 아이템 선택부터 시작 (사냥꾼의 부적 등)
    public override void OpenCanvas(Action closeCallback = null)
    {
        Open(closeCallback);
        ShowSelectItem();
    }

    void Open(Action closeCallback)
    {
        base.OpenCanvas(closeCallback);
        selectItemPanels = GetComponentsInChildren<SelectRunePanel>(true);
        FixSelectedPanelRefs();
        runeRewardDone = null;
        SetGuide(null);
        SetRewardChest(false);
    }

    public void OpenRuneReward(List<ItemData> items, Action onDone)
    {
        Open(null);
        runeRewardDone = onDone;
        runeRewardItems = items;
        ShowRuneRewardItems();
        SetRewardChest(true);
    }

    // 룬 보상일 때만 상자 표시. 열리면 상자가 튀어나오며 흔들림 (실제 시간)
    void SetRewardChest(bool show)
    {
        if (show)
            EnsureRewardChest();
        if (rewardChestRoot == null)
            return;
        rewardChestRoot.gameObject.SetActive(show);
        if (!show || rewardChestImage == null)
            return;

        if (rewardChestSprite != null)
            rewardChestImage.sprite = rewardChestSprite;
        if (rewardChestText != null)
            rewardChestText.text = TranslateManager.GetText("RuneReward_ChestTitle");

        Transform chest = rewardChestImage.transform;
        chest.DOKill();
        chest.localScale = Vector3.zero;
        chest.localRotation = Quaternion.identity;
        DOTween.Sequence()
            .Append(chest.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
            .Append(chest.DOShakeRotation(0.4f, new Vector3(0, 0, 18f), 12, 90, true))
            .SetUpdate(true)
            .SetLink(chest.gameObject);
    }

    // 씬에 연결이 없으면 selectItemContainer의 제목 글씨 위에 상자 그림 + 설명 글씨를 만듦 (글씨는 제목을 복사해 폰트 유지)
    void EnsureRewardChest()
    {
        if (rewardChestRoot != null || selectItemContainer == null)
            return;
        TMPro.TMP_Text title = null;
        foreach (Transform child in selectItemContainer.transform)
            if (child.TryGetComponent(out TMPro.TMP_Text t)) { title = t; break; }

        var root = new GameObject("RewardChest", typeof(RectTransform));
        rewardChestRoot = (RectTransform)root.transform;
        rewardChestRoot.SetParent(selectItemContainer.transform, false);
        rewardChestRoot.anchorMin = rewardChestRoot.anchorMax = new Vector2(0.5f, 0.5f);
        float titleY = title != null ? ((RectTransform)title.transform).anchoredPosition.y : 255f;
        rewardChestRoot.anchoredPosition = new Vector2(0, titleY + 150f);
        rewardChestRoot.sizeDelta = new Vector2(600, 220);

        var imgGo = new GameObject("ChestImage", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        var imgRt = (RectTransform)imgGo.transform;
        imgRt.SetParent(rewardChestRoot, false);
        imgRt.anchoredPosition = new Vector2(0, -10);
        imgRt.sizeDelta = new Vector2(170, 170);
        rewardChestImage = imgGo.GetComponent<UnityEngine.UI.Image>();
        rewardChestImage.preserveAspect = true;
        rewardChestImage.raycastTarget = false;

        if (title != null)
        {
            rewardChestText = Instantiate(title, rewardChestRoot);
            rewardChestText.name = "ChestText";
            var textRt = (RectTransform)rewardChestText.transform;
            textRt.anchorMin = textRt.anchorMax = new Vector2(0.5f, 0.5f);
            textRt.anchoredPosition = new Vector2(0, 105);
            textRt.sizeDelta = new Vector2(600, 60);
            rewardChestText.fontSize = title.fontSize * 0.7f;
            rewardChestText.color = new Color(1f, 0.85f, 0.35f);
            rewardChestText.raycastTarget = false;
            if (rewardChestText.TryGetComponent(out TranslateText tt))
                Destroy(tt); // 복사한 제목의 번역 글씨 컴포넌트가 글씨를 덮지 않게
        }
    }

    void ShowRuneRewardItems()
    {
        MinionFieldController.Instance.tilePickCallback = null;
        SetGuide(null);
        shownItems = runeRewardItems;
        ShowPanels(runeRewardItems);
    }

    // 선택 단계 + 카드 표시. 카드는 레벨업 카드처럼 차례로 튀어나옴 (실제 시간)
    void ShowPanels(List<ItemData> items)
    {
        SetStep(SelectItemStep.SelectItem);
        for (int i = 0; i < selectItemPanels.Length; i++)
        {
            bool active = i < items.Count;
            selectItemPanels[i].gameObject.SetActive(active);
            if (!active)
                continue;
            selectItemPanels[i].SetItemData(items[i]);
            Transform tr = selectItemPanels[i].transform;
            tr.DOKill();
            tr.localScale = Vector3.zero;
            tr.DOScale(1.1f, 0.2f + i * 0.15f).SetUpdate(true).SetLink(tr.gameObject)
                .OnComplete(() => tr.DOScale(1f, 0.25f).SetUpdate(true).SetLink(tr.gameObject));
        }
    }

    // (2026-11-22) 씬의 타일 선택 패널(selectedPanel)이 설명 글씨를 안내 문구에, 아이콘을 배경 그림에 연결하고 있어
    //   설명이 안내 문구에 덮이고 패널엔 "Lv.0"이 남던 문제 → 패널 안의 DescText·아이콘으로 바로잡음
    void FixSelectedPanelRefs()
    {
        if (selectedPanel == null || selectTileContainer == null)
            return;
        Transform container = selectTileContainer.transform;
        if (selectedPanel.descText != null && selectedPanel.descText.transform.parent == container)
        {
            if (selectTileGuideText == null)
                selectTileGuideText = selectedPanel.descText;
            foreach (var t in container.GetComponentsInChildren<TMPro.TMP_Text>(true))
                if (t.transform.parent != container) { selectedPanel.descText = t; break; }
        }
        if (selectedPanel.thumImage != null && selectedPanel.thumImage.transform.parent == container)
            foreach (var img in container.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                if (img.transform.parent != container && img.name.StartsWith("Image")) { selectedPanel.thumImage = img; break; }
    }

    void SetGuide(string text)
    {
        if (selectTileGuideText == null && selectTileContainer != null)
            foreach (Transform child in selectTileContainer.transform)
                if (child.TryGetComponent(out TMPro.TMP_Text t)) { selectTileGuideText = t; break; }
        if (selectTileGuideText == null)
            return;
        if (defaultGuide == null)
            defaultGuide = selectTileGuideText.text;
        selectTileGuideText.text = text ?? defaultGuide;
    }

    void OnRuneTilePicked(Tile tile)
    {
        if (tile == null || selectedItem == null)
            return;
        if (tile.tileItem is TileItem placed)
        {
            if (placed.key != selectedItem.key)
            {
                ShowPopup(TranslateManager.GetText("Rune_OtherInstalled"));
                return;
            }
            if (!placed.ForceUpgrade())
            {
                ShowPopup(TranslateManager.GetText("Rune_MaxLevel"));
                return;
            }
            SoundManager.Play(SFXType.TileItemUpgradeSuccess);
            ToastCanvas.Toast(TranslateManager.GetText("Rune_Upgraded"));
            EffectManager.PlayPopText(tile.transform.position, $"Lv.{placed.upgradeLevel}!", new Color(1f, 0.85f, 0.25f));
            if (IsRuneRewardOpen)
                HeroUpgradeSystem.OnRuneRewardPlaced(placed, tile.transform.position); // (2026-11-24) 용사 강화: 확률로 1레벨 더
            FinishRunePick();
            return;
        }
        TileItem prefab = selectedItem.itemPrefab as TileItem;
        if (prefab != null && !prefab.CanPickTile(tile))
        {
            ToastCanvas.Toast(TranslateManager.GetText(prefab.PickFailMessageKey(tile)));
            return;
        }
        KingSlime.Instance.AddItem(selectedItem.key, tile).Forget();
        SoundManager.Play(SFXType.ItemInstall);
        ToastCanvas.Toast(TranslateManager.GetText(prefab is PioneerTileItem ? "Rune_TileRepaired" : "Rune_Applied"));
        if (IsRuneRewardOpen && tile.tileItem is TileItem installed)
            HeroUpgradeSystem.OnRuneRewardPlaced(installed, tile.transform.position); // (2026-11-24) 용사 강화: 확률로 Lv.2
        FinishRunePick();
    }

    static void ShowPopup(string body)
    {
        if (YesOrNoCanvas.Instance != null)
            YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("Rune_PopupTitle"), body, true);
        else
            ToastCanvas.Toast(body);
    }

    void FinishRunePick()
    {
        Action done = runeRewardDone;
        runeRewardDone = null;
        finishing = true;
        CloseCanvas();
        finishing = false;
        done?.Invoke();
    }

    public override void CloseCanvas()
    {
        // 룬 보상은 반드시 골라야 함: 닫으려 하면(칸 선택 취소 등) 룬 3장 화면으로 돌아감
        if (!finishing && runeRewardDone != null && gameObject.activeSelf)
        {
            ShowRuneRewardItems();
            return;
        }
        MinionFieldController.Instance.tilePickCallback = null;
        base.CloseCanvas();
    }

    void SetStep(SelectItemStep step)
    {
        this.step = step;
        selectItemContainer.SetActive(step == SelectItemStep.SelectItem);
        selectTileContainer.SetActive(step == SelectItemStep.SelectTile);
    }

    // 선택 단계에서 선택지 다시 뽑기 (AdRefreshItemButton). 가능하면 지금 보이는 아이템은 빼고 뽑는다
    public bool RefreshItems()
    {
        if (step != SelectItemStep.SelectItem)
            return false;
        if (runeRewardDone != null) // 룬 보상: 룬 후보 안에서 다시 뽑기 (가능하면 지금 보이는 룬 제외)
        {
            runeRewardItems = RuneRewardSystem.PickChoices(runeRewardItems);
            ShowRuneRewardItems();
            return true;
        }
        ShowSelectItem(shownItems);
        return true;
    }

    void ShowSelectItem(ICollection<ItemData> avoid = null)
    {
        List<ItemData> items = ItemManager.Instance.GetDrawTileItems(SELECT_ITEM_COUNT, avoid);
        if (items.Count == 0)
        {
            CloseCanvas();
            return;
        }
        shownItems = items;
        ShowPanels(items);
    }

    public void Selected(ItemData data)
    {
        selectedItem = data;
        if (runeRewardDone != null) // 룬 보상: 레벨업 룬과 같은 칸 선택 (빈 칸 = 새김, 같은 룬 칸 = 레벨 +1)
        {
            SetStep(SelectItemStep.SelectTile);
            selectedPanel.SetItemData(data);
            SetGuide(TranslateManager.GetText(data.itemPrefab is PioneerTileItem ? "Rune_SelectBlockedTile" : "Rune_SelectTile"));
            MinionFieldController.Instance.tilePickCallback = OnRuneTilePicked;
            return;
        }
        if (data.itemPrefab is TileItem tileItem && tileItem.NeedTile)
        {
            SetStep(SelectItemStep.SelectTile);
            selectedPanel.SetItemData(data);
            MinionFieldController.Instance.tilePickCallback = OnTilePicked;
            return;
        }

        Install(null);
    }

    void OnTilePicked(Tile tile)
    {
        TileItem tileItem = selectedItem.itemPrefab as TileItem;
        if (tileItem != null && !tileItem.CanPickTile(tile))
        {
            ToastCanvas.Toast(TranslateManager.GetText(tileItem.PickFailMessageKey(tile)));
            return;
        }
        Install(tile);
    }

    void Install(Tile tile)
    {
        KingSlime.Instance.AddItem(selectedItem.key, tile).Forget();
        SoundManager.Play(SFXType.ItemInstall);
        CloseCanvas();
    }
}

public enum SelectItemStep
{
    SelectItem,
    SelectTile
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Draw(확률 연출) → SelectItem(랜덤 3개 중 선택) → SelectTile(타일을 눌러 설치)
// 아이템 하나 반드시 선택해야됌. 열려 있어도 게임은 멈추지 않음
public class SelectItemCanvas : CanvasUI<SelectItemCanvas>
{
    protected override void PlayOpenSound() { } // 뽑기 소리(DrawItemRoll)가 따로 남
    public const int SELECT_ITEM_COUNT = 3;

    public DrawTileItemContainer drawTileItemContainer;
    public GameObject selectItemContainer;
    public GameObject selectTileContainer; // 타일을 눌러야 하므로 레이캐스트로 필드를 가리면 안 됨
    public SelectItemPanel[] selectItemPanels;
    public SelectItemPanel selectedPanel;
    public SelectItemStep step;
    public float drawDuration = 1f; // Draw 연출 시간(초). 0이면 연출 없이 바로 판정
    public TMPro.TMP_Text selectTileGuideText; // 타일 선택 안내 글씨 (비우면 selectTileContainer 바로 아래 글씨)

    // (2026-11-21) 레벨업 룬 카드: 타일 선택만 (OpenRuneTilePick). 같은 룬 칸 = 레벨 +1, 다른 룬 칸 = 팝업
    Action runePickDone;
    string defaultGuide;
    public bool IsRunePicking => runePickDone != null;

    ItemData selectedItem;
    List<ItemData> shownItems = new(); // 지금 선택지 (갱신 시 겹치지 않게)
    

    // 확률 판정 없이 아이템 선택부터 시작
    public override void OpenCanvas(Action closeCallback = null)
    {
        Open(closeCallback);
        ShowSelectItem();
    }

    // 뽑기 버튼용: 확률(%) 판정 후 성공하면 아이템 선택. 가격은 버튼에서 미리 냄
    // onFail: 실패했을 때 (장인의 망치 환불 등)
    public void OpenDraw(int chance, Action closeCallback = null, Action onFail = null)
    {
        Open(closeCallback);
        Draw(chance, onFail).Forget();
    }

    void Open(Action closeCallback)
    {
        base.OpenCanvas(closeCallback);
        selectItemPanels = GetComponentsInChildren<SelectItemPanel>(true);
        FixSelectedPanelRefs();
        runePickDone = null;
        SetGuide(null);
    }

    // 레벨업 룬 카드 → "룬을 적용할 타일을 선택해주세요." → 칸을 누르면 새김/레벨 +1 → onDone
    public void OpenRuneTilePick(ItemData data, Action onDone)
    {
        Open(null);
        runePickDone = onDone;
        selectedItem = data;
        SetStep(SelectItemStep.SelectTile);
        selectedPanel.SetItemData(data);
        SetGuide(TranslateManager.GetText(data.itemPrefab is PioneerTileItem ? "Rune_SelectBlockedTile" : "Rune_SelectTile"));
        MinionFieldController.Instance.tilePickCallback = OnRuneTilePicked;
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
        Action done = runePickDone;
        runePickDone = null;
        CloseCanvas();
        done?.Invoke();
    }

    public override void CloseCanvas()
    {
        MinionFieldController.Instance.tilePickCallback = null;
        base.CloseCanvas();
        // 룬 카드 타일 선택을 칸을 고르지 않고 닫음 → 레벨업 카드로 돌아감
        if (runePickDone != null)
        {
            runePickDone = null;
            InGame.LevelUpCanvas.Instance?.CancelRunePick();
        }
    }

    void SetStep(SelectItemStep step)
    {
        this.step = step;
        drawTileItemContainer.gameObject.SetActive(step == SelectItemStep.Draw);
        selectItemContainer.SetActive(step == SelectItemStep.SelectItem);
        selectTileContainer.SetActive(step == SelectItemStep.SelectTile);
    }

    // 실패하면 코인만 소모되고 가격/확률은 그대로
    async UniTaskVoid Draw(int chance, Action onFail = null)
    {
        SetStep(SelectItemStep.Draw);
        bool isSuccess = UnityEngine.Random.Range(0, 100) < chance;
        drawTileItemContainer.StartDraw(chance);
        SoundManager.Play(SFXType.DrawItemRoll);
        if (drawDuration > 0f)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(drawDuration), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (canceled)
                return;
        }

        if (!isSuccess)
        {
            SoundManager.Play(SFXType.TileItemUpgradeFail); // 확률 실패 (강화 실패와 같은 소리)
            ToastCanvas.Toast(TranslateManager.GetText("Draw_Fail"));
            onFail?.Invoke();
            CloseCanvas();
            return;
        }
        else
        {
            SoundManager.Play(SFXType.DrawItemSuccess);
            ToastCanvas.Toast(TranslateManager.GetText("Draw_Success"));
        }

        GameManager.Instance.drawItemCount++;
        ShowSelectItem();
    }

    // 선택 단계에서 선택지 다시 뽑기 (AdRefreshItemButton). 가능하면 지금 보이는 아이템은 빼고 뽑는다
    public bool RefreshItems()
    {
        if (step != SelectItemStep.SelectItem)
            return false;
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

        SetStep(SelectItemStep.SelectItem);
        for (int i = 0; i < selectItemPanels.Length; i++)
        {
            bool active = i < items.Count;
            selectItemPanels[i].gameObject.SetActive(active);
            if (active)
                selectItemPanels[i].SetItemData(items[i]);
        }
    }

    public void Selected(ItemData data)
    {
        selectedItem = data;
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
    Draw,
    SelectItem,
    SelectTile
}

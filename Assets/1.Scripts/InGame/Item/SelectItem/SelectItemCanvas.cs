using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Draw(확률 연출) → SelectItem(랜덤 3개 중 선택) → SelectTile(타일을 눌러 설치)
// 아이템 하나 반드시 선택해야됌. 열려 있어도 게임은 멈추지 않음
public class SelectItemCanvas : CanvasUI<SelectItemCanvas>
{
    public const int SELECT_ITEM_COUNT = 3;

    public DrawTileItemContainer drawTileItemContainer;
    public GameObject selectItemContainer;
    public GameObject selectTileContainer; // 타일을 눌러야 하므로 레이캐스트로 필드를 가리면 안 됨
    public SelectItemPanel[] selectItemPanels;
    public SelectItemStep step;
    public float drawDuration = 1f; // Draw 연출 시간(초). 0이면 연출 없이 바로 판정

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
    }

    public override void CloseCanvas()
    {
        SlimeFieldController.Instance.tilePickCallback = null;
        base.CloseCanvas();
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
        drawTileItemContainer.StartDraw();
        if (drawDuration > 0f)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(drawDuration), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (canceled)
                return;
        }

        if (!isSuccess)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Draw_Fail"));
            onFail?.Invoke();
            CloseCanvas();
            return;
        }
        else
        {
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
            SlimeFieldController.Instance.tilePickCallback = OnTilePicked;
            return;
        }

        Install(null);
    }

    void OnTilePicked(Tile tile)
    {
        if (!tile.CanAddTileItem)
        {
            ToastCanvas.Toast(TranslateManager.GetText("TileItem_AlreadyInstalled"));
            return;
        }
        Install(tile);
    }

    void Install(Tile tile)
    {
        KingSlime.Instance.AddItem(selectedItem.key, tile).Forget();
        CloseCanvas();
    }
}

public enum SelectItemStep
{
    Draw,
    SelectItem,
    SelectTile
}

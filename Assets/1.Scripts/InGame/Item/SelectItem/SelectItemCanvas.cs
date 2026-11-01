using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// Draw(확률 연출) → SelectItem(랜덤 3개 중 선택) → SelectTile(타일을 눌러 설치)
// 아이템 하나 반드시 선택해야됌. 열려 있는 동안 게임은 일시정지 (고민해서 배치하도록)
public class SelectItemCanvas : CanvasUI<SelectItemCanvas>
{
    const int SELECT_ITEM_COUNT = 3;

    public GameObject startDrawContainer;
    public GameObject selectItemContainer;
    public GameObject selectTileContainer; // 타일을 눌러야 하므로 레이캐스트로 필드를 가리면 안 됨
    public SelectItemPanel[] selectItemPanels;
    public SelectItemStep step;
    public float drawDuration = 1f; // Draw 연출 시간(초). 0이면 연출 없이 바로 판정

    ItemData selectedItem;
    float prevTimeScale;

    // 확률 판정 없이 아이템 선택부터 시작
    public override void OpenCanvas(Action closeCallback = null)
    {
        Open(closeCallback);
        ShowSelectItem();
    }

    // 뽑기 버튼용: 확률(%) 판정 후 성공하면 아이템 선택. 가격은 버튼에서 미리 냄
    public void OpenDraw(int chance, Action closeCallback = null)
    {
        Open(closeCallback);
        Draw(chance).Forget();
    }

    void Open(Action closeCallback)
    {
        base.OpenCanvas(closeCallback);
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0;
        selectItemPanels = GetComponentsInChildren<SelectItemPanel>(true);
    }

    public override void CloseCanvas()
    {
        SlimeSpawner.Instance.tilePickCallback = null;
        Time.timeScale = prevTimeScale;
        base.CloseCanvas();
    }

    void SetStep(SelectItemStep step)
    {
        this.step = step;
        startDrawContainer.SetActive(step == SelectItemStep.Draw);
        selectItemContainer.SetActive(step == SelectItemStep.SelectItem);
        selectTileContainer.SetActive(step == SelectItemStep.SelectTile);
    }

    // 실패하면 코인만 소모되고 가격/확률은 그대로
    async UniTaskVoid Draw(int chance)
    {
        SetStep(SelectItemStep.Draw);
        if (drawDuration > 0f)
        {
            bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(drawDuration), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (canceled)
                return;
        }

        if (UnityEngine.Random.Range(0, 100) >= chance)
        {
            ToastCanvas.Toast("뽑기 실패");
            CloseCanvas();
            return;
        }

        GameManager.Instance.drawItemCount++;
        ShowSelectItem();
    }

    void ShowSelectItem()
    {
        List<ItemData> items = ItemManager.Instance.GetDrawTileItems(SELECT_ITEM_COUNT);
        if (items.Count == 0)
        {
            CloseCanvas();
            return;
        }

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
            SlimeSpawner.Instance.tilePickCallback = OnTilePicked;
            return;
        }

        Install(null);
    }

    void OnTilePicked(Tile tile)
    {
        if (!tile.CanAddTileItem)
        {
            ToastCanvas.Toast("이미 아이템이 설치된 타일입니다.");
            return;
        }
        Install(tile);
    }

    void Install(Tile tile)
    {
        Character.Instance.AddItem(selectedItem.key, tile).Forget();
        CloseCanvas();
    }
}

public enum SelectItemStep
{
    Draw,
    SelectItem,
    SelectTile
}

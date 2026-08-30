using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MiniMeSlotPanel : MonoBehaviour
{
    public int idx;
    public MiniMePanel miniMePanel;

    public GameObject sellPanel;
    public TMP_Text priceText;
    MiniMe miniMe;
    public static MiniMeSlotPanel startDragPanel;
    public bool selling;
    public void SetMiniMe(MiniMe me)
    {
        miniMe = me;
        miniMePanel.SetMiniMe(me);
    }

    public void UpdatePanel()
    {
        selling = false;
        sellPanel.SetActive(false);
        miniMePanel.gameObject.SetActive(false);
        if (idx < Character.Instance.miniMeInventory.curMiniMes.Count)
        {
            miniMePanel.gameObject.SetActive(true);
            MiniMe miniMe = Character.Instance.miniMeInventory.curMiniMes[idx];
            SetMiniMe(miniMe);
        }
        else if (idx + 1 == MiniMeSpawner.Instance.miniMeSlotCount)
        {
            selling = true;
            gameObject.SetActive(true);
            priceText.text = MiniMeSpawner.Instance.GetSlotPrice().ToString();
            sellPanel.SetActive(true);
        }

    }

    public void OnClickedPurchaseSlot()
    {
        if (!selling)
            return;

        if (Character.Instance.coin < MiniMeSpawner.Instance.GetSlotPrice())
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }
        Character.Instance.AddCoin(-MiniMeSpawner.Instance.GetSlotPrice());
        MiniMeSpawner.Instance.PurchaseSlot(idx);
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnBeginDrag(BaseEventData data)
    {
        Debug.Log($"MiniMeSlotPanel OnBeginDrag {gameObject.name}");
        if (miniMe == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        startDragPanel = this;
        miniMePanel.thumImage.enabled = false;
        MiniMeMergeDragUI.Instance.SetMiniMe(miniMe);
        MiniMeMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnDrag(BaseEventData data)
    {
        // Debug.Log($"MiniMeSlotPanel OnDrag {gameObject.name}");
        if (miniMe == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        MiniMeMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnEndDrag(BaseEventData data)
    {
        Debug.Log($"MiniMeSlotPanel OnEndDrag {gameObject.name}");
        MiniMeMergeDragUI.Instance.Hide();
        miniMePanel.thumImage.enabled = true;
        startDragPanel = null;
    }

    public void OnDrop(BaseEventData data)
    {
        Debug.Log($"MiniMeSlotPanel OnDrop {gameObject.name}");
        PointerEventData eventData = (PointerEventData)data;
        MiniMeSlotPanel sourceSlot = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<MiniMeSlotPanel>() : null;
        if (sourceSlot == null || sourceSlot == this || sourceSlot.miniMe == null || miniMe == null)
        {
            Debug.Log("MiniMeSlotPanel OnDrop if (sourceSlot == null || sourceSlot == this || sourceSlot.miniMe == null || miniMe == null)");
            return;
        }
        if (sourceSlot != null)
        {
            Debug.Log($"MiniMeSlotPanel OnDrop if(sourceSlot != null) {sourceSlot.name}");
        }


        Debug.Log($"MiniMeSlotPanel OnDrop Try To Merged sourceSlot {sourceSlot.name}");
        Merged(sourceSlot).Forget();

    }

    async UniTask Merged(MiniMeSlotPanel sourceSlot)
    {
        var (result, mergeResultKey, lv) = await MiniMeSpawner.Instance.Merge(sourceSlot.miniMe, this.miniMe);
        if (result)
        {
            Debug.Log($"Merged mergeResultKey {mergeResultKey}");
            Character.Instance.RemoveMiniMe(miniMe);
            Character.Instance.RemoveMiniMe(sourceSlot.miniMe);
            Character.Instance.AddMiniMe(mergeResultKey, lv);
        }
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnPointerDown(BaseEventData data)
    {
        if (miniMe == null)
            return;
        Debug.Log($"MiniMeSlotPanel OnPointerDown {gameObject.name}");
        MiniMeInfoPanel.Instance.SetMiniMe(miniMe);
    }

    public void OnPointerExit(BaseEventData data)
    {
        // Debug.Log($"MiniMeSlotPanel OnPointerDown {gameObject.name}");

        if (miniMe == null)
            return;

        if (MiniMeInfoPanel.Instance.gameObject.activeSelf)
            MiniMeInfoPanel.Instance.gameObject.SetActive(false);
    }

    public void OnPointerUp(BaseEventData data)
    {
        if (miniMe == null)
            return;
        Debug.Log($"MiniMeSlotPanel OnPointerUp {gameObject.name}");
        MiniMeInfoPanel.Instance.gameObject.SetActive(false);
    }
}
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

    // public bool sell;
    // public int price;
    public GameObject sellPanel;
    public TMP_Text priceText;
    MiniMe miniMe;
    public static MiniMeSlotPanel startDragPanel;
    public void SetMiniMe(MiniMe me)
    {
        miniMe = me;
        miniMePanel.SetMiniMe(me);
    }

    public void UpdatePanel()
    { 
        sellPanel.SetActive(false);
        miniMePanel.gameObject.SetActive(false);
        if(idx < Character.Instance.miniMeInventory.curMiniMes.Count)
        {
            miniMePanel.gameObject.SetActive(true);
            MiniMe miniMe = Character.Instance.miniMeInventory.curMiniMes[idx];
            SetMiniMe(miniMe);
        }
        else if(idx +1 == MiniMeSpawner.Instance.miniMeSlotCount)
        {
            gameObject.SetActive(true);
            priceText.text = MiniMeSpawner.Instance.GetSlotPrice().ToString();
            sellPanel.SetActive(true);
        }
        
    }

    public void OnClickedPurchaseSlot()
    {
        if(Character.Instance.coin < MiniMeSpawner.Instance.GetSlotPrice())
        {
            return;
        }
        Character.Instance.AddCoin(-MiniMeSpawner.Instance.GetSlotPrice());
        MiniMeSpawner.Instance.PurchaseSlot(idx);
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnBeginDrag(BaseEventData data)
    {
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
        if (miniMe == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        MiniMeMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnEndDrag(BaseEventData data)
    {
        MiniMeMergeDragUI.Instance.Hide();
        miniMePanel.thumImage.enabled = true;
    }

    public void OnDrop(BaseEventData data)
    {
        PointerEventData eventData = (PointerEventData)data;
        MiniMeSlotPanel sourceSlot = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<MiniMeSlotPanel>() : null;
        if (sourceSlot == null || sourceSlot == this || sourceSlot.miniMe == null || miniMe == null)
            return;

        if(sourceSlot == startDragPanel)
        {
            return;            
        }

        Merged(sourceSlot).Forget();
    }

    async UniTask Merged(MiniMeSlotPanel sourceSlot)
    {
         var (result, mergeResultKey,lv) = await MiniMeSpawner.Instance.Merge(startDragPanel.miniMe, sourceSlot.miniMe);
        if (result)
        {
            Character.Instance.RemoveMiniMe(startDragPanel.miniMe);
            Character.Instance.RemoveMiniMe(sourceSlot.miniMe);
            Character.Instance.AddMiniMe(mergeResultKey,lv);
        }
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnPointerDown(BaseEventData data)
    {
        MiniMeInfoPanel.Instance.SetMiniMe(miniMe);
    }
    
    public void OnPointerUp(BaseEventData data)
    {
        MiniMeInfoPanel.Instance.gameObject.SetActive(false);
    }
}
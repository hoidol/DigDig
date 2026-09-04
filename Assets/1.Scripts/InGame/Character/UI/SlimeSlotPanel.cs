using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class SlimeSlotPanel : MonoBehaviour
{
    public int idx;
    public SlimePanel slimePanel;

    public GameObject sellPanel;
    public TMP_Text priceText;
    Slime slime;
    public static SlimeSlotPanel startDragPanel;
    public bool selling;
    public void SetSlime(Slime me)
    {
        slime = me;
        slimePanel.SetSlime(me);
    }

    public virtual void UpdatePanel()
    {
        selling = false;
        sellPanel.SetActive(false);
        slimePanel.gameObject.SetActive(false);
        if (idx < Character.Instance.slimeInventory.curSlimes.Count)
        {
            slimePanel.gameObject.SetActive(true);
            Slime slime = Character.Instance.slimeInventory.curSlimes[idx];
            SetSlime(slime);
        }
        else if (idx + 1 == SlimeSpawner.Instance.slimeSlotCount)
        {
            selling = true;
            gameObject.SetActive(true);
            priceText.text = SlimeSpawner.Instance.GetSlotPrice().ToString();
            sellPanel.SetActive(true);
        }

    }

    public void OnClickedPurchaseSlot()
    {
        if (!selling)
            return;

        if (Character.Instance.coin < SlimeSpawner.Instance.GetSlotPrice())
        {
            ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
            return;
        }
        Character.Instance.AddCoin(-SlimeSpawner.Instance.GetSlotPrice());
        SlimeSpawner.Instance.PurchaseSlot(idx);
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnBeginDrag(BaseEventData data)
    {
        Debug.Log($"SlimeSlotPanel OnBeginDrag {gameObject.name}");
        if (slime == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        startDragPanel = this;
        slimePanel.thumImage.enabled = false;
        SlimeMergeDragUI.Instance.SetSlime(slime);
        SlimeMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnDrag(BaseEventData data)
    {
        // Debug.Log($"SlimeSlotPanel OnDrag {gameObject.name}");
        if (slime == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        SlimeMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnEndDrag(BaseEventData data)
    {
        Debug.Log($"SlimeSlotPanel OnEndDrag {gameObject.name}");
        SlimeMergeDragUI.Instance.Hide();
        slimePanel.thumImage.enabled = true;
        startDragPanel = null;
    }

    public void OnDrop(BaseEventData data)
    {
        Debug.Log($"SlimeSlotPanel OnDrop {gameObject.name}");
        PointerEventData eventData = (PointerEventData)data;
        SlimeSlotPanel sourceSlot = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<SlimeSlotPanel>() : null;
        if (sourceSlot == null || sourceSlot == this || sourceSlot.slime == null || slime == null)
        {
            Debug.Log("SlimeSlotPanel OnDrop if (sourceSlot == null || sourceSlot == this || sourceSlot.slime == null || slime == null)");
            return;
        }
        if (sourceSlot != null)
        {
            Debug.Log($"SlimeSlotPanel OnDrop if(sourceSlot != null) {sourceSlot.name}");
        }


        Debug.Log($"SlimeSlotPanel OnDrop Try To Merged sourceSlot {sourceSlot.name}");
        Merged(sourceSlot).Forget();

    }

    async UniTask Merged(SlimeSlotPanel sourceSlot)
    {
        var (result, mergeResultKey, lv) = await SlimeSpawner.Instance.Merge(sourceSlot.slime, this.slime);
        if (result)
        {
            Debug.Log($"Merged mergeResultKey {mergeResultKey}");
            Character.Instance.RemoveSlime(slime);
            Character.Instance.RemoveSlime(sourceSlot.slime);
            Character.Instance.AddSlime(mergeResultKey, lv);
        }
        CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnPointerDown(BaseEventData data)
    {
        if (slime == null)
            return;
        Debug.Log($"SlimeSlotPanel OnPointerDown {gameObject.name}");
        SlimeInfoPanel.Instance.SetSlime(slime);
    }

    public void OnPointerExit(BaseEventData data)
    {
        // Debug.Log($"SlimeSlotPanel OnPointerDown {gameObject.name}");

        if (slime == null)
            return;

        if (SlimeInfoPanel.Instance.gameObject.activeSelf)
            SlimeInfoPanel.Instance.gameObject.SetActive(false);
    }

    public void OnPointerUp(BaseEventData data)
    {
        if (slime == null)
            return;
        Debug.Log($"SlimeSlotPanel OnPointerUp {gameObject.name}");
        SlimeInfoPanel.Instance.gameObject.SetActive(false);
    }
}
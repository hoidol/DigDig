using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 드래그/드롭 이벤트는 인스펙터의 EventTrigger 컴포넌트로 연결한다 (BeginDrag/Drag/EndDrag/Drop -> 아래 메서드)
public class MinionSlotPanel : MonoBehaviour
{
    public int idx;
    public MinionPanel minionPanel;

    public GameObject sellPanel;
    public TMP_Text priceText;
    Minion minion;
    public static MinionSlotPanel startDragPanel;
    public bool selling;
    public void SetMinion(Minion me)
    {
        minion = me;
        minionPanel.SetMinion(me);
    }

    public virtual void UpdatePanel()
    {
        selling = false;
        sellPanel.SetActive(false);
        minionPanel.gameObject.SetActive(false);
        if (idx < KingSlime.Instance.minionInventory.curMinions.Count)
        {
            minionPanel.gameObject.SetActive(true);
            Minion minion = KingSlime.Instance.minionInventory.curMinions[idx];
            SetMinion(minion);
        }
        // else if (idx + 1 == MinionFieldController.Instance.minionSlotCount)
        // {
        //     selling = true;
        //     gameObject.SetActive(true);
        //     priceText.text = MinionFieldController.Instance.GetSlotPrice().ToString();
        //     sellPanel.SetActive(true);
        // }

    }

    public void OnClickedPurchaseSlot()
    {
        if (!selling)
            return;

        // if (Character.Instance.coin < MinionFieldController.Instance.GetSlotPrice())
        // {
        //     ToastCanvas.Toast(TranslateManager.GetText("Not enough coin"));
        //     return;
        // }
        // Character.Instance.AddCoin(-MinionFieldController.Instance.GetSlotPrice());
        // MinionFieldController.Instance.PurchaseSlot(idx);
        // CharacterManageCanvas.Instance.UpdateCanvas();
    }

    public void OnBeginDrag(BaseEventData data)
    {
        Debug.Log($"MinionSlotPanel OnBeginDrag {gameObject.name}");
        if (minion == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        startDragPanel = this;
        minionPanel.thumImage.enabled = false;
        MinionMergeDragUI.Instance.SetMinion(minion);
        MinionMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnDrag(BaseEventData data)
    {
        // Debug.Log($"MinionSlotPanel OnDrag {gameObject.name}");
        if (minion == null)
            return;

        PointerEventData eventData = (PointerEventData)data;
        MinionMergeDragUI.Instance.SetPosition(eventData.position);
    }

    public void OnEndDrag(BaseEventData data)
    {
        Debug.Log($"MinionSlotPanel OnEndDrag {gameObject.name}");
        MinionMergeDragUI.Instance.Hide();
        minionPanel.thumImage.enabled = true;
        startDragPanel = null;
    }

    public void OnDrop(BaseEventData data)
    {
        Debug.Log($"MinionSlotPanel OnDrop {gameObject.name}");
        PointerEventData eventData = (PointerEventData)data;
        MinionSlotPanel sourceSlot = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<MinionSlotPanel>() : null;
        if (sourceSlot == null || sourceSlot == this || sourceSlot.minion == null || minion == null)
        {
            Debug.Log("MinionSlotPanel OnDrop if (sourceSlot == null || sourceSlot == this || sourceSlot.minion == null || minion == null)");
            return;
        }
        if (sourceSlot != null)
        {
            Debug.Log($"MinionSlotPanel OnDrop if(sourceSlot != null) {sourceSlot.name}");
        }


        Debug.Log($"MinionSlotPanel OnDrop Try To Merged sourceSlot {sourceSlot.name}");
        Merged(sourceSlot).Forget();

    }

    async UniTask Merged(MinionSlotPanel sourceSlot)
    {
        var (result, mergeResultKey, lv) = await MinionFieldController.Instance.Merge(sourceSlot.minion, this.minion);

        
        if (result)
        {
            Debug.Log($"Merged mergeResultKey {mergeResultKey}");
            KingSlime.Instance.RemoveMinion(minion);
            KingSlime.Instance.RemoveMinion(sourceSlot.minion);
            KingSlime.Instance.AddMinion(mergeResultKey, lv);
        }
        KingSlimeManageCanvas.Instance.UpdateCanvas();
    }

    public void OnPointerDown(BaseEventData data)
    {
        if (minion == null)
            return;
        Debug.Log($"MinionSlotPanel OnPointerDown {gameObject.name}");
        MinionInfoPanel.Instance.SetMinion(minion);
    }

    public void OnPointerExit(BaseEventData data)
    {
        // Debug.Log($"MinionSlotPanel OnPointerDown {gameObject.name}");

        if (minion == null)
            return;

        if (MinionInfoPanel.Instance.gameObject.activeSelf)
            MinionInfoPanel.Instance.gameObject.SetActive(false);
    }

    public void OnPointerUp(BaseEventData data)
    {
        if (minion == null)
            return;
        Debug.Log($"MinionSlotPanel OnPointerUp {gameObject.name}");
        MinionInfoPanel.Instance.gameObject.SetActive(false);
    }
}
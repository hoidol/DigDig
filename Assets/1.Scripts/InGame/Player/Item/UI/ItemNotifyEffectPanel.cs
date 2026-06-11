using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ItemNotifyEffectPanel : MonoBehaviour
{

    public Image thumImage;
    // public Image cooltimeImage;


    public void SetItem(Item item)
    {
        thumImage.sprite = item.itemData.thumbnail;
        // item.notifyListener = null;
        // item.notifyListener += Notify;
    }

    // void Notify()
    // {
        
    // }

    // bool wasActive;

    // public void Triggered()
    // {
    //     thumImage.transform.DOKill();
    //     thumImage.transform.DOScale(1.2f, 0.2f).OnComplete(() =>
    //     {
    //         thumImage.transform.DOScale(1f, 0.2f);
    //     });
    // }

    // void StartPulse()
    // {
    //     thumImage.transform.DOKill();
    //     thumImage.transform
    //         .DOScale(1.2f, 0.4f)
    //         .SetLoops(-1, LoopType.Yoyo)
    //         .SetEase(Ease.InOutSine);
    // }

    // void StopPulse()
    // {
    //     thumImage.transform.DOKill();
    //     thumImage.transform.localScale = Vector3.one;
    // }

    // void Update()
    // {
    //     if (triggerItem != null)
    //     {
    //         cooltimeImage.fillAmount = triggerItem.coolTime > 0
    //             ? triggerItem.CoolTimer / triggerItem.coolTime
    //             : 0f;
    //     }
    //     else if (triggerCycleItem != null)
    //     {
    //         bool isActive = triggerCycleItem.IsActive;

    //         if (isActive != wasActive)
    //         {
    //             wasActive = isActive;
    //             if (isActive) StartPulse();
    //             else StopPulse();
    //         }

    //         cooltimeImage.fillAmount = isActive ? 0f
    //             : triggerCycleItem.coolTime > 0
    //                 ? triggerCycleItem.CoolTimer / triggerCycleItem.coolTime
    //                 : 0f;
    //     }
    // }
}

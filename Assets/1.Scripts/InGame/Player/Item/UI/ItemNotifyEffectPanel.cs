using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ItemNotifyEffectPanel : ItemDisplayPanel
{
    public Image cooltimeImage;

    TriggerItem triggerItem;
    TriggerCycleItem triggerCycleItem;

    public override void SetItemData(ItemData itemData)
    {
        base.SetItemData(itemData);

        triggerItem = null;
        triggerCycleItem = null;

        Item i = Player.Instance.itemInventory.GetItem(itemData.key);

        if (i is TriggerItem ti)
        {
            triggerItem = ti;

            triggerItem.triggerListener -= Triggered;
            triggerItem.triggerListener += Triggered;
            cooltimeImage.gameObject.SetActive(true);
        }
        else if (i is TriggerCycleItem tci)
        {
            triggerCycleItem = tci;
            cooltimeImage.gameObject.SetActive(true);
        }
        else
        {
            cooltimeImage.gameObject.SetActive(false);
        }
    }

    bool wasActive;

    public void Triggered()
    {
        thumImage.transform.DOKill();
        thumImage.transform.DOScale(1.2f, 0.2f).OnComplete(() =>
        {
            thumImage.transform.DOScale(1f, 0.2f);
        });
    }

    void StartPulse()
    {
        thumImage.transform.DOKill();
        thumImage.transform
            .DOScale(1.2f, 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void StopPulse()
    {
        thumImage.transform.DOKill();
        thumImage.transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (triggerItem != null)
        {
            cooltimeImage.fillAmount = triggerItem.coolTime > 0
                ? triggerItem.CoolTimer / triggerItem.coolTime
                : 0f;
        }
        else if (triggerCycleItem != null)
        {
            bool isActive = triggerCycleItem.IsActive;

            if (isActive != wasActive)
            {
                wasActive = isActive;
                if (isActive) StartPulse();
                else StopPulse();
            }

            cooltimeImage.fillAmount = isActive ? 0f
                : triggerCycleItem.coolTime > 0
                    ? triggerCycleItem.CoolTimer / triggerCycleItem.coolTime
                    : 0f;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ItemActivePanel : ItemDisplayPanel
{
    public Image cooltimeImage;

    TriggerItem triggerItem;
    TriggerCycleItem triggerCycleItem;

    public override void SetItemData(ItemData itemData, int count)
    {
        base.SetItemData(itemData, count);

        triggerItem = null;
        triggerCycleItem = null;

        Item item = Player.Instance.itemInventory.GetItem(itemData.key);
        if (item is TriggerItem ti)
        {
            triggerItem = ti;
            cooltimeImage.gameObject.SetActive(true);
        }
        else if (item is TriggerCycleItem tc)
        {
            triggerCycleItem = tc;
            cooltimeImage.gameObject.SetActive(true);
        }
        else
        {
            cooltimeImage.gameObject.SetActive(false);
        }
    }

    public override void UpdateOverlapCount(ItemData itemData, int count)
    {
        // 보여줄 필요 없음
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
            if (triggerCycleItem.IsActive)
                cooltimeImage.fillAmount = 1f;
            else
                cooltimeImage.fillAmount = triggerCycleItem.coolTime > 0
                    ? triggerCycleItem.CoolTimer / triggerCycleItem.coolTime
                    : 0f;
        }
    }
}

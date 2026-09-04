using System.Collections.Generic;
using UnityEngine;

public class ChangeItemCanvas : CanvasUI<ChangeItemCanvas>
{
        public ChangeItemPanel[] ownItemPanels;

        string itemKey;
        public void OpenCanvas(string itemKey)
        {
            base.OpenCanvas(closeCallback);
            this.itemKey = itemKey;
            if(ownItemPanels.Length<=0)
                ownItemPanels = GetComponentsInChildren<ChangeItemPanel>();

            List<Item> curItems = Character.Instance.itemInventory.curItems;
            for(int i = 0; i < ownItemPanels.Length; i++)
            {
                ownItemPanels[i].SetItem(curItems[i],i);
            }
            OpenCanvas();
        }
        
        public void Selected(int idx)
        {
            Character.Instance.RemoveItem(Character.Instance.itemInventory.curItems[idx].key);
            Character.Instance.AddItem(itemKey);
            CloseCanvas();
        }
}

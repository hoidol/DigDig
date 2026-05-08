using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectItemPanel : ItemPanel
{

    public void OnClickedButton()
    {
        GetComponentInParent<SelectItemCanvas>().Selected(itemData);
    }
}

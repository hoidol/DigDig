using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectItemPanel : MonoBehaviour
{
    [SerializeField] protected ItemData itemData;

    public Image thumImage;
    public TMP_Text descText;

    public void SetItemData(ItemData itemData)
    {
        this.itemData = itemData;
        thumImage.sprite = itemData.thum;

        descText.text = itemData.GetDescription();
    }

    public void OnClickedButton()
    {
        GetComponentInParent<SelectItemCanvas>().Selected(itemData);


    }
}

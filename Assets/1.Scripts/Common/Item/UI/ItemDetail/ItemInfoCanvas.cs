using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoCanvas : CanvasUI<ItemInfoCanvas>
{
    //강화 시 능력치
    [SerializeField] ItemData itemData;
    [SerializeField] Image thumImage;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descText;
    [SerializeField] int lv;
    [SerializeField] GameObject nextLvButton;
    [SerializeField] GameObject preLvButton;

    
    public  void OpenCanvas(string key,Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        itemData = ItemData.GetItemData(key);
        
        titleText.text = itemData.Title;
        thumImage.sprite = itemData.thumbnail;
        lv =1;
        UpdateCanvas();
    }
    
    public void UpdateCanvas()
    {
        descText.text = itemData.GetDescription(lv);
        preLvButton.SetActive(false);
        nextLvButton.SetActive(false);
        if(lv > 1)
        {
            preLvButton.SetActive(true);
        }
        if(lv < ItemData.MAX_LEVEL)
        {
            nextLvButton.SetActive(true);
        }

    }

    public void OnClickedNextLevel()
    {
        lv++;
        UpdateCanvas();
    }

    public void OnClickedPreLevel()
    {
        lv--;
        UpdateCanvas();
    }

}

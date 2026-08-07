using TMPro;
using UnityEngine;

public class DrawItemButton : ButtonUI
{
    public TMP_Text titleText;
    public  void UpdateButton()
    {
        titleText.text= TranslateManager.GetText($"DrawItem_Title");

         if (Character.Instance.itemInventory.curItems.Count >= 5)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public override void OnClickedBtn()
    {

        Time.timeScale = 0;
        SelectItemCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;
        });

    }
}
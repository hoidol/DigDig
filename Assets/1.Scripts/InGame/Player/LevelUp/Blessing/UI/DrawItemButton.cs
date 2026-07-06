using UnityEngine;

public class DrawItemButton : BlessingButton
{
    public override void UpdateButton()
    {
        titleText.text= TranslateManager.GetText($"DrawItem_Title");

         if (Player.Instance.itemInventory.curItems.Count >= 5)
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
        BlessingCanvas.Instance.CloseCanvas();

        Time.timeScale = 0;
        SelectItemCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;
        });

    }
}
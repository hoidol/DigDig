using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatPanel : MonoBehaviour
{
    [SerializeField] StatData statData;
    [SerializeField] int lv;

    public Image thumImage;
    public Image lvBgImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public void SetStatData(StatData statData, int lv)
    {
        this.statData = statData;
        this.lv = lv;
        if (statData == null)
        {
            gameObject.SetActive(false);
            return;
        }
        lvBgImage.color = lv switch
        {
            1 => new Color(0.8f, 0.8f, 0.8f), //회색
            2 => new Color(0.3f, 0.8f, 0.3f), //초록
            3 => new Color(0.8f, 0.3f, 0.3f), //빨강
            _ => Color.white
        };
        gameObject.SetActive(true);
        titleText.text = $"{statData.Title} +{lv}";
        descriptionText.text = statData.GetDescription(lv);

        //UI 업데이트
    }


    public void OnClickedSelect()
    {
        // Player.Instance.statInventory.AddStat(this.statData, this.lv);
        StatCanvas.Instance.CloseCanvas();
    }
}

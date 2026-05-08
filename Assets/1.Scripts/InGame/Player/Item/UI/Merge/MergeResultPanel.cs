using TMPro;
using UnityEngine;

public class MergeResultPanel : MonoBehaviour
{

    [SerializeField] ItemDisplayPanel itemDisplayPanel;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descText;
    public void SetMergedItemEvent(MergedItemEvent e)
    {
        titleText.text = e.resultItemData.Title;
        descText.text = e.resourceItem1.GetDescription();
    }

    public void OnClickedClose()
    {
        MergeItemCanvas.Instance.CloseCanvas();
    }
}

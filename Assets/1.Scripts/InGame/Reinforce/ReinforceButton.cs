using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ReinforceButton : ButtonUI
{
    public TMP_Text expText;
    public Image expBar;
    void Start()
    {
        //   GameEventBus.Subscribe<MergeCoinEvent>(OnMergeCoinEvent);  
    }
    // void OnMergeCoinEvent(MergeCoinEvent e)
    // {
    //     UpdateButton();

    // }
    // public void UpdateButton()
    // {
    //     expText.text = $"{SlimeSpawner.Instance.mergeCoinGetCounter}/{GameSetting.MERGE_REINFORCE_MAX_EXP}";
    //     expBar.fillAmount = (float)SlimeSpawner.Instance.mergeCoinGetCounter/(float)GameSetting.MERGE_REINFORCE_MAX_EXP;
    // }

    public override void OnClickedBtn()
    {
        ReinforceCanvas.Instance.OpenCanvas();
    }
}

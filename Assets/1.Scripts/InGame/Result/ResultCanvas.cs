using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    public TMP_Text stageText;
    public TMP_Text destroyCountText;
    public TMP_Text memoryFragmentCountText;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }
    void UpdateCanvas()
    {
        stageText.text = GameManager.Instance.stageData.order.ToString();
        int destroy = GameManager.Instance.destroyStoneCount + GameManager.Instance.killEnemyCount;
        destroyCountText.text = $"처치 수 : {destroy}";
        int memoryFragmentCount = destroy / 10;
        memoryFragmentCountText.text = $"X {memoryFragmentCount}";

    }

    public void OnClickedHome()
    {
        CloseCanvas();
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}

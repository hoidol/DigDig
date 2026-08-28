using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultCanvas : CanvasUI<ResultCanvas>
{
    public TMP_Text stageText;
    public TMP_Text destroyCountText;
    public TMP_Text memorPieceCountText;

    bool clear;

    public void OpenCanvas(bool clear, Action closeCallback = null)
    {
        this.clear = clear;
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }

    void UpdateCanvas()
    {
        stageText.text = GameManager.Instance.stageData.order.ToString();
        int destroy = GameManager.Instance.destroyStoneCount + GameManager.Instance.killEnemyCount;
        destroyCountText.text = $"처치 수 : {destroy}";


        int memoryPieceCount = destroy / 10;
        memorPieceCountText.text = $"X {memoryPieceCount}";
        UserManager.Instance.AddMemoryPiece(memoryPieceCount);

        //최대 깊이에 따라서 보상 받게 하자
        float distance = Vector2.Distance(Character.Instance.transform.position,Vector2.zero); 

        UserManager.Instance.userStageManager.EndStage(GameManager.Instance.stageData.key, clear, clear? GameManager.Instance.phase-1 : GameManager.Instance.stageData.phaseDatas.Length-1);
    }

    public void OnClickedHome()
    {
        CloseCanvas();
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}

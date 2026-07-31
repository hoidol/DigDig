using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FailCanvas : CanvasUI<FailCanvas>
{
    string DAILY_RESURRECTION_COUNT_KEY = "DAILY_RESURRECTION_COUNT";
    int MAX_DAILY_RESURRECTION_COUNT = 5;
    public TMP_Text leftResurrectionText;
    public GameObject resumeButton;

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        UpdateCanvas();
    }
    void UpdateCanvas()
    {
        int count = PlayerPrefs.GetInt(DAILY_RESURRECTION_COUNT_KEY, MAX_DAILY_RESURRECTION_COUNT);
        leftResurrectionText.text = $"일일 부활 {MAX_DAILY_RESURRECTION_COUNT - count / MAX_DAILY_RESURRECTION_COUNT}";
        resumeButton.SetActive(count > 0); 
    }
    public void OnClickeResume()
    {
        GameManager.Instance.Resume();
    }
    public void OnClickedCancel()
    {
        CloseCanvas();
        FadeCanvs.Instance.FadeOutIn(null, () => { SceneManager.LoadScene("Lobby"); });
    }
}

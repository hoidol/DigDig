using UnityEngine;
using UnityEngine.SceneManagement;
public class StartStageButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        // UserManager.Instance.userStageManager.StartStage();
        SceneManager.LoadScene("InGame");
    }
}

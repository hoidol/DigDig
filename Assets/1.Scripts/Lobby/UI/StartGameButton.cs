using UnityEngine;
using UnityEngine.SceneManagement;
public class StartGameButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        SceneManager.LoadScene("InGame");
    }
}

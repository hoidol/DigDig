using UnityEngine;
using Lobby;
using UnityEngine.UI;
public class LobbyStateButton : ButtonUI
{
    public LobbyState state;
    public Image bgImage;
    public virtual void UpdateButton()
    {
        if(LobbyManager.Instance.lobbyState == state)
        {
            bgImage.color = ColorSetting.activeColor;
        }
        else
        {
            bgImage.color = Color.gray;
        }
    }
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(state);
    }
}
using UnityEngine;

public class LobbyStateButton : ButtonUI
{
    public LobbyState state;
    public virtual void UpdateButton()
    {
        
    }
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(state);
    }
}
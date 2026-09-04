using UnityEngine;

public class EquipmentButton : LobbyStateButton
{
    public override void OnClickedBtn()
    {
        LobbyManager.Instance.OpenCanvas(LobbyState.Equipment);
    }

}

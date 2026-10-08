using UnityEngine;

public class ItemBox : EventObject
{
    public override void StartEvent()
    {
        interacting = true;
        Time.timeScale = 0;
        SelectItemCanvas.Instance.OpenCanvas(() =>
        {
            Time.timeScale = 1;
            Destroy();
        });
    }
}
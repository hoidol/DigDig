using UnityEngine;

public class SlimeSettingPanel : MonoSingleton<SlimeSettingPanel>
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    Minion targetMinion;
    public void SetMinion(Minion minion)
    {
        targetMinion = minion;
        if (targetMinion == null)
        {
            obj.SetActive(false);
            return;
        }
        obj.SetActive(true);
        setFindTargetButton.SetMinion(targetMinion);
    }
}

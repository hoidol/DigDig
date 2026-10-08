using UnityEngine;

public class SlimeSettingPanel : MonoSingleton<SlimeSettingPanel>
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    Slime targetSlime;
    public void SetSlime(Slime slime)
    {
        if(slime == null)
        {
            obj.SetActive(false);
            return;
        }
        obj.SetActive(true);
        targetSlime = slime;
        setFindTargetButton.SetSlime(slime);
    }
}

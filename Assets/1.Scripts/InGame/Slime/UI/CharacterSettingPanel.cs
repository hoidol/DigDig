using UnityEngine;

public class CharacterSettingPanel : MonoSingleton<CharacterSettingPanel>
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    Slime targetSlime;
    public void SetSlime(Slime slime)
    {
        targetSlime = slime;
        if (targetSlime == null)
        {
            obj.SetActive(false);
            return;
        }
        obj.SetActive(true);
        setFindTargetButton.SetSlime(targetSlime);
    }
}

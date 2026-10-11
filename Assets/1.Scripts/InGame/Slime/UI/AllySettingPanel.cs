using UnityEngine;

public class AllySettingPanel : MonoSingleton<AllySettingPanel>
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    IAllyUnit targetSlime;
    public void SetSlime(IAllyUnit allyUnit)
    {
        targetSlime = allyUnit;
        if (allyUnit.AllyType == AllyType.Character)
        {
            //Character character = allyUnit.Transform.GetComponent<Character>();
        }
        else
        {
            Slime slime = allyUnit.Transform.GetComponent<Slime>();
        }
        if (allyUnit == null)
        {
            obj.SetActive(false);
            return;
        }
        obj.SetActive(true);
        setFindTargetButton.SetSlime(allyUnit);
    }
}

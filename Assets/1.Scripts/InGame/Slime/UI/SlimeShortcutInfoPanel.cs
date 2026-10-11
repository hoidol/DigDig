using UnityEngine;

public class SlimeShortcutInfoPanel : MonoSingleton<SlimeShortcutInfoPanel>
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    Slime targetSlime;
    public void SetSlime(IAllyUnit allyUnit)
    {
        if (allyUnit.AllyType == AllyType.Character)
        {
            Character character = allyUnit.Transform.GetComponent<Character>();
        }
        else
        {
            Slime slime = allyUnit.Transform.GetComponent<Slime>();
        }
    }
}

using UnityEngine;


[CreateAssetMenu(fileName ="Pierce",menuName = "SlimeData/C/Pierce")]
public class PierceSlimeData : EnhanceSlimeData
{
    public int[] pierceCount = {2,3,4,5,6};

    public override string GetDescription(int level =0)
    {
        return string.Format(TranslateManager.GetText("Slime.Pierce.Desc"), pierceCount[level]);
    }

}
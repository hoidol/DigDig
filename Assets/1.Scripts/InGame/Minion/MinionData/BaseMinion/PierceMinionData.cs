using UnityEngine;


[CreateAssetMenu(fileName ="Pierce",menuName = "MinionData/C/Pierce")]
public class PierceMinionData : MinionData
{
    public int[] pierceCount = {2,3,4,5,6};

    public override string GetDescription(int level =0)
    {
        return string.Format(TranslateManager.GetText("Minion.Pierce.Desc"), pierceCount[level]);
    }

}
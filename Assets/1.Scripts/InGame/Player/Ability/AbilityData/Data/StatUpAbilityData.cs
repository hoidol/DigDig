using UnityEngine;

[CreateAssetMenu(fileName = "StatUpAbilityData", menuName = "Ability/StatUpAbilityData", order = 0)]
public class StatUpAbilityData : AbilityData
{
#if UNITY_EDITOR
    public override void LoadData()
    {
        Debug.Log("StatUpAbilityData LoadData()");
        base.LoadData();
    }
#endif
}

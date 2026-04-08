using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData", order = 0)]
public class PlayerData : ScriptableObject
{
    public string key;

    public PlayerStat[] playerStats = new PlayerStat[(int)StatType.Count];
    public PlayerStat GetPlayerStat(StatType type)
    {
        for (int i = 0; i < playerStats.Length; i++)
        {
            if (playerStats[i].statType == type)
            {
                return playerStats[i];
            }
        }
        return null;
    }
#if UNITY_EDITOR
    public void Edit()
    {
        for (int i = 0; i < playerStats.Length; i++)
        {
            playerStats[i].statType = (StatType)i;
        }
    }
#endif
}
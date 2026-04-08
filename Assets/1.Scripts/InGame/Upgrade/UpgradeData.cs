using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "UpgradeData", order = 0)]
public class UpgradeData : ScriptableObject
{
    public UpgradeType upgradeType;

    public int maxLv = -1;
    public float initValue;
    public float increaseValue;
    public float initPrice;
    public float increasePrice;
    public string valueFormat = "{0}";

    public float GetValue(int lv = -1)
    {
        if (lv == -1)
        {
            lv = 0;
            //lv = Player.Instance.playerStatMgr.GetUpgradeLv(PlayerStatManager.UpgradeTypeToStatType(upgradeType));// 임시 
        }
        return initValue + increaseValue * lv;
    }

    public static UpgradeData GetUpgradeData(UpgradeType upgradeType)
    {
        return Resources.Load<UpgradeData>($"UpgradeData/{upgradeType}");
    }

    public string Title => $"{upgradeType.ToString()}";
    public string Value => string.Format(valueFormat, GetValue());
    public int GetPrice(int lv = -1)
    {
        if (lv == -1)
        {
            lv = 0;// 임시 
        }
        return (int)(initPrice + increasePrice * lv);
    }
}
public enum UpgradeType
{
    AttackPower,
    MaxHp,
    RecoveryHp,
    Exp
}
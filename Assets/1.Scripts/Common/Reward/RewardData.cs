

using UnityEngine;

[System.Serializable]
public class RewardData
{
    public RewardType rewardType;
    public string value;

    public string GetValueToString()
    {
        switch (rewardType)
        {
            case RewardType.Minion:
                return MinionManager.Instance.GetMinionData(value).Title;
        }
        return value;
    }
    public Sprite GetThum()
    {
        switch (rewardType)
        {
            case RewardType.Minion:
                return MinionManager.Instance.GetMinionData(value).thum;
            case RewardType.Energe:
                return Resources.Load<Sprite>("Icons/Currency/Energy");
            case RewardType.Exp:
                return Resources.Load<Sprite>("Icons/Exp");
        }
        return null;
    }
    public void Receive()
    {
        switch (rewardType)
        {
            case RewardType.Minion:
                RewardCanvas.Instance.OpenCanvas(this);
                UserDataManager.Instance.userMinionManager.AddUserMinion(value);
                break;
            case RewardType.Energe:
                UserDataManager.Instance.AddEnerge(int.Parse(value));
                break;
            case RewardType.Exp:
                UserDataManager.Instance.userMyInfoManager.AddExp(int.Parse(value));
                break;
        }

    }
}
public class UserReward
{

    public string id;
    public bool gotten;
}
// (2026-11-27) 골드(1)·다이아(2) 삭제. 번호로 저장될 수 있어 남은 값의 번호는 그대로
public enum RewardType
{
    Minion = 0,
    Energe = 3,
    Exp = 4,
}
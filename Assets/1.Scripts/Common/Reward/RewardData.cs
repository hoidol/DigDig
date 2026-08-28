

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
            case RewardType.MiniMe:
                return MiniMeManager.Instance.GetMiniMeData(value).Title;
        }
        return null;
    }
    public Sprite GetThum()
    {
        switch (rewardType)
        {
            case RewardType.MiniMe:
                return MiniMeManager.Instance.GetMiniMeData(value).thum;
        }
        return null;
    }
    public void Receive()
    {
        switch (rewardType)
        {
            case RewardType.MiniMe:
                RewardCanvas.Instance.OpenCanvas(this);
                UserManager.Instance.userMiniMeManager.AddUserMiniMe(value);
            break;
        }
        
    }
}
public class UserReward
{
    
    public string id;
    public bool gotten;
}
public enum RewardType
{
    MiniMe
}
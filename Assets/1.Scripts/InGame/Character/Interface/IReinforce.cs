public interface IReinforce
{
    string Key {get;}   
    ReinforceType ReinforceType {get;}
    int GetLevel();
    bool IsMaxLevel();
}

public enum ReinforceType
{
    Item,
    Bullet
}
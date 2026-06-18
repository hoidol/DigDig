public interface IReinforce
{   
    ReinforceType ReinforceType {get;}
    int GetLevel();
}

public enum ReinforceType
{
    Item,
    Bullet
}
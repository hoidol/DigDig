using UnityEngine;

[System.Serializable]
public class EnemySpecialAttackCondition
{
    public EnemySpecialAttackConditionType conditionType;
    public int value;
    public bool CheckCondition(IEnemySpecialAttackPattern enemy)
    {
        switch (conditionType)
        {
            case EnemySpecialAttackConditionType.NearPlayer:
                return Vector2.Distance(enemy.Transform.position, Player.Instance.transform.position) < value;
            case EnemySpecialAttackConditionType.None:
                return true;
            default:
                return false;
        }
    }
}

public enum EnemySpecialAttackConditionType
{
    NearPlayer,
    None,
}
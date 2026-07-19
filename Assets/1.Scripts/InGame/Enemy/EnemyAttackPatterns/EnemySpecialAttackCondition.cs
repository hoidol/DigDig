using UnityEngine;

[System.Serializable]
public class EnemyAttackCondition
{
    public EnemyAttackConditionType conditionType;
    public int value;
    public bool CheckCondition(IEnemySpecialAttackPattern enemy)
    {
        switch (conditionType)
        {
            case EnemyAttackConditionType.NearPlayer:
                return Vector2.Distance(enemy.Transform.position, Player.Instance.transform.position) < value;
            case EnemyAttackConditionType.None:
                return true;
            default:
                return false;
        }
    }
}

public enum EnemyAttackConditionType
{
    NearPlayer,
    None,
}
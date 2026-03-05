using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData", order = 0)]
public class EnemyData : ScriptableObject
{
    public EnemyType enemyType;
    public float initHp;
    public float increaseHp;
    public float initAttackPower;
    public float increaseAttackPower;
    public float attackSpeed;
    public float attackRange;
    public float moveSpeed;

    public float GetHp(int lv = -1)
    {
        if (lv == -1)
        {
            lv = 0;// 임시 
        }
        return initHp + increaseHp * lv;
    }
    public float GetAttackPower(int lv = -1)
    {
        if (lv == -1)
        {
            lv = 0;// 임시 
        }
        return initAttackPower + increaseAttackPower * lv;
    }
}

public enum EnemyType
{
    Melee,
    Ranged
}
using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public string key;
    public string title;
    public const int MAX_OWN_COUNT = 3;
    public string GetDescription(int count = 1)
    {
        return itemPrefab.GetDescription(count);
    }
    public Sprite thumbnail;
    [SerializeField] int price;
    public int GetPrice()
    {
        return price;
    }
    public Grade grade;      // 등급별 필터링용
    public Item itemPrefab;
    public int applyOrder; // 아이템 적용 순서
    // 실제 효과는 Item 컴포넌트(or Strategy)로 분리

    public static Color GetGradeColor(Grade grade)
    {
        return grade switch
        {
            Grade.Normal => new Color(0.78f, 0.78f, 0.78f), // 회색
            Grade.Rare => new Color(0.00f, 0.44f, 0.87f), // 파랑
            Grade.Unique => new Color(0.56f, 0.28f, 0.98f), // 보라
            Grade.Legend => new Color(1.00f, 0.64f, 0.00f), // 주황/금
            Grade.Myth => new Color(0.98f, 0.18f, 0.18f), // 빨강
            _ => Color.white
        };
    }
}
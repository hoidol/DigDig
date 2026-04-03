using UnityEngine;
using TMPro;
public class GoldPanel : MonoBehaviour
{
    public TMP_Text goldText;

    void Start()
    {
        GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
    }

    void OnGoldChanged(GoldChangedEvent e)
    {
        goldText.text = e.gold.ToString();  // 변경 시에만 실행
    }
}

public class GoldChangedEvent
{
    public int gold;
    public GoldChangedEvent(int gold)
    {
        this.gold = gold;
    }
}
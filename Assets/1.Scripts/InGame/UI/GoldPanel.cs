using UnityEngine;
using TMPro;
public class GoldPanel : MonoBehaviour
{
    public TMP_Text goldText;

    void Start()
    {
        GameEventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
        goldText.text = "0";
    }

    void OnGoldChanged(GoldChangedEvent e)
    {
        goldText.text = e.totalGold.ToString();  // 변경 시에만 실행
    }
}

public class GoldChangedEvent
{

    public int totalGold;
    public int addGold;
    public GoldChangedEvent(int gold, int aGold)
    {
        totalGold = gold;
        addGold = aGold;
    }
}
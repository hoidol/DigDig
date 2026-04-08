
using UnityEngine;
using UnityEngine.UI;
public class ExpBar : MonoBehaviour
{
    public Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameEventBus.Subscribe<ExpChangedEvent>(OnExpChanged);
        image.fillAmount = 0;
    }

    void OnExpChanged(ExpChangedEvent e)
    {
        image.fillAmount = (float)e.exp / (float)e.maxExp;
    }

}
public class ExpChangedEvent
{
    public int exp;
    public int maxExp;
    public ExpChangedEvent(int e, int mE)
    {
        this.exp = e;
        this.maxExp = mE;
    }
}
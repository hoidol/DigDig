using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SlimeHealthCanvas : MonoBehaviour
{
    public TMP_Text hpText;
    public Image hpBar;

    void Start()
    {
        GameEventBus.Subscribe<SlimeUpdateEvent>(OnPlayerUpdate);
        GameEventBus.Subscribe<SlimeHpChangedEvent>(OnHpChanged);
    }

    void OnPlayerUpdate(SlimeUpdateEvent e)
    {
        UpdateCanvas();
    }

    void OnHpChanged(SlimeHpChangedEvent e)
    {
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        hpBar.fillAmount = Slime.Instance.CurHp / Slime.Instance.statMgr.MaxHp;
        hpText.text = $"{(int)Slime.Instance.CurHp}";
    }

}

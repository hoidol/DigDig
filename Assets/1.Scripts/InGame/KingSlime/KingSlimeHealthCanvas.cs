using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class KingSlimeHealthCanvas : MonoBehaviour
{
    public TMP_Text hpText;
    public Image hpBar;

    void Start()
    {
        GameEventBus.Subscribe<KingSlimeUpdateEvent>(OnPlayerUpdate);
        GameEventBus.Subscribe<KingSlimeHpChangedEvent>(OnHpChanged);
    }

    void OnPlayerUpdate(KingSlimeUpdateEvent e)
    {
        UpdateCanvas();
    }

    void OnHpChanged(KingSlimeHpChangedEvent e)
    {
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        hpBar.fillAmount = KingSlime.Instance.CurHp / KingSlime.Instance.statMgr.MaxHp;
        hpText.text = $"{(int)KingSlime.Instance.CurHp}";
    }

}

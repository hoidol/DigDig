using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerHpStateCanvas : MonoBehaviour
{
    public TMP_Text hpText;
    public Image hpBar;

    void Start()
    {
        GameEventBus.Subscribe<PlayerUpdateEvent>(OnPlayerUpdate);
        GameEventBus.Subscribe<PlayerHpChangedEvent>(OnHpChanged);
    }



    void OnPlayerUpdate(PlayerUpdateEvent e)
    {
        UpdateCanvas();
    }

    void OnHpChanged(PlayerHpChangedEvent e)
    {
        UpdateCanvas();
    }
    void UpdateCanvas()
    {

        hpBar.fillAmount = Player.Instance.curHp / Player.Instance.statMgr.MaxHp;
        hpText.text = $"{(int)Player.Instance.curHp}/{(int)Player.Instance.statMgr.MaxHp}";
    }

}

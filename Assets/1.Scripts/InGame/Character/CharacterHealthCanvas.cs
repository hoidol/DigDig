using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CharacterHealthCanvas : MonoBehaviour
{
    public TMP_Text hpText;
    public Image hpBar;

    void Start()
    {
        GameEventBus.Subscribe<CharacterUpdateEvent>(OnPlayerUpdate);
        GameEventBus.Subscribe<CharacterHpChangedEvent>(OnHpChanged);
    }

    void OnPlayerUpdate(CharacterUpdateEvent e)
    {
        UpdateCanvas();
    }

    void OnHpChanged(CharacterHpChangedEvent e)
    {
        UpdateCanvas();
    }
    public void UpdateCanvas()
    {
        hpBar.fillAmount = Character.Instance.curHp / Character.Instance.statMgr.MaxHp;
        hpText.text = $"{(int)Character.Instance.curHp}";
    }

}

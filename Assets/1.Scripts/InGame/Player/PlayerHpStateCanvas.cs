using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerHpStateCanvas : MonoBehaviour
{
    public TMP_Text hpText;
    public Image hpBar;

    void Update()
    {
        float hpAmount = Player.Instance.curHp / Player.Instance.playerStatMgr.MaxHp;
        hpBar.fillAmount = hpAmount;
        hpText.text = $"{(int)Player.Instance.curHp}/{(int)Player.Instance.playerStatMgr.MaxHp}";

    }

}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpBonusPanel : MonoBehaviour
{

    // public Image thumImage;

    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public LevelUpBonusType levelUpBonusType;

    public virtual void SetBonusPanel()
    {
        // titleText.text = TranslateManager.GetText($"{levelUpBonusType}_title");
        // descriptionText.text = TranslateManager.GetText($"{levelUpBonusType}_description");
    }

    public void OnClickedSelect()
    {
        LevelUpCanvas.Instance.CloseCanvas();
        switch (levelUpBonusType)
        {
            case LevelUpBonusType.MaxHp:
                Player.Instance.AddBuff(new Buff(StatType.MaxHp, 5, StatOpType.Add));
                break;
            case LevelUpBonusType.FullHeal:
                Player.Instance.AddHp(Player.Instance.health.MaxHp);
                break;

            case LevelUpBonusType.AttackPower:
                Player.Instance.AddBuff(new Buff(StatType.AttackPower, 1f, StatOpType.Add));
                break;
            case LevelUpBonusType.RecoveryHp:
                Player.Instance.AddBuff(new Buff(StatType.RecoveryHp, 0.1f, StatOpType.Add));
                break;

            case LevelUpBonusType.Bounce:
                Player.Instance.AddBounce(1);
                break;
        }


    }
}
public enum LevelUpBonusType
{
    MaxHp,
    FullHeal, //체력 완전 회복
    Bounce,
    AttackPower,
    RecoveryHp
    // AddSpecialBullet,
    // MergeBullet
}

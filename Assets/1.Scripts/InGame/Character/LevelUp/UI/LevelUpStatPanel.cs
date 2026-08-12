using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpStatPanel : MonoBehaviour
{

    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public LevelUpStatType levelUpStatType;
    LevelUpStatData levelUpStatData;

    public virtual void SetLevelUpStatPanel()
    {
        if (levelUpStatData == null)
            levelUpStatData = LevelUpStatManager.Instance.GetLevelUpStatData(levelUpStatType);

        titleText.text = levelUpStatData.Title;
        descriptionText.text = levelUpStatData.GetDescription();
    }

    public void OnClickedSelect()
    {
        LevelUpCanvas.Instance.CloseCanvas();
        // Character.Instance.AddLevelUpState(levelUpStatType, 1);

    }
}
public enum LevelUpStatType : int
{
    MaxHp,
    FullHeal, //체력 완전 회복
    Bounce,
    AttackPower,
    Count
    // RecoveryHp
    // AddSpecialBullet,
    // MergeBullet
}

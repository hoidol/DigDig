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
        InGame.LevelUpCanvas.Instance.CloseCanvas();
        // Character.Instance.AddLevelUpState(levelUpStatType, 1);

    }
}
public enum LevelUpStatType : int
{
    MaxHp,//최대 체력 증가
    HalfHeal,
    //FullHeal, //체력 완전 회복    
    AttackPower, //공격력 증가
    AttackSpeed, //공격속도 증가
    Count, 
    // RecoveryHp
    // AddSpecialBullet,
    // MergeBullet
}

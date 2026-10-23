using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class AttackTypePanel : MonoBehaviour 
{
    public TMP_Text titleText;

    public Image thumImage;
    public void SetAttackType(AttackType attackType)
    {
        
        titleText.text = AttackInfo.Title(attackType) ;
        // descText.text = AttackInfo.Desc(attackType);
        thumImage.sprite = AttackInfo.GetSprite(attackType);
    }
}
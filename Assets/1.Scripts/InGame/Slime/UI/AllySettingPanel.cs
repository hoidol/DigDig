using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllySettingPanel : MonoBehaviour
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    // public TMP_Text nameText;
    // public Image thumImage;
    public CharacterShortcutInfoPanel characterShortcutInfoPanel;
    public SlimeShortcutInfoPanel slimeShortcutInfoPanel;
    public AttackTypePanel attackTypePanel;

    IAllyUnit targetSlime;
    void Start()
    {
      obj.SetActive(false);  
    }
    public void SetSlime(IAllyUnit allyUnit)
    {
        targetSlime = allyUnit;
        if (allyUnit == null)
        {
            obj.SetActive(false);
            return;
        }

        characterShortcutInfoPanel.gameObject.SetActive(false);
        slimeShortcutInfoPanel.gameObject.SetActive(false);
        if (allyUnit.AllyType == AllyType.Character)
        {
            CharacterData characterData = CharacterManager.Instance.GetCharacterData(Character.Instance.characterName );
            // thumImage.sprite = characterData.thum;
            // nameText.text = characterData.Title;
            
            attackTypePanel.SetAttackType(GetAttackType(Character.Instance.weapon.attackType, Character.Instance.shotModifier));
            characterShortcutInfoPanel.SetCharacter(Character.Instance);
        }
        else
        {
            Slime slime = allyUnit.Transform.GetComponent<Slime>();
            // thumImage.sprite = slime.slimeData.thum;
            // nameText.text = slime.slimeData.Title;
            attackTypePanel.SetAttackType(GetAttackType(slime.slimeData.attackType, slime.shotModifier));
            slimeShortcutInfoPanel.SetSlime(slime);
        }
        obj.SetActive(true);
        setFindTargetButton.SetSlime(targetSlime);
    }

    // 공격 타입 변경 타일 위에 있으면 바뀐 타입을 표시
    AttackType GetAttackType(AttackType baseType, ShotModifier shotModifier)
    {
        return shotModifier.tileItem is AttackTypeTileItem attackTypeTileItem ? attackTypeTileItem.AttackType : baseType;
    }
}

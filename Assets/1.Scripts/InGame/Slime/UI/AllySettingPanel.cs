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
            
            attackTypePanel.SetAttackType(Character.Instance.weapon.attackType);
            characterShortcutInfoPanel.SetCharacter(Character.Instance);
        }
        else
        {
            Slime slime = allyUnit.Transform.GetComponent<Slime>();
            // thumImage.sprite = slime.slimeData.thum;
            // nameText.text = slime.slimeData.Title;
            attackTypePanel.SetAttackType(slime.slimeData.attackType);
            slimeShortcutInfoPanel.SetSlime(slime);
        }
        obj.SetActive(true);
        setFindTargetButton.SetSlime(targetSlime);
    }
}

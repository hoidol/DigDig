using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllySettingPanel : MonoBehaviour
{
    public GameObject obj;
    public SetFindTargetButton setFindTargetButton;
    public SetBuffRangeButton setBuffRangeButton;                 // 십자 버프 슬라임: 십자 / 대각
    public SetRotationDirectionButton setRotationDirectionButton; // 회전체·경로 순환 슬라임: 시계 / 반시계
    // public TMP_Text nameText;
    // public Image thumImage;
    public KingSlimeShortcutInfoPanel characterShortcutInfoPanel;
    public MinionShortcutInfoPanel minionShortcutInfoPanel;
    public AttackTypePanel attackTypePanel;

    IAllyUnit targetMinion;
    void Start()
    {
      obj.SetActive(false);  
    }
    public void SetMinion(IAllyUnit allyUnit)
    {
        targetMinion = allyUnit;
        if (allyUnit == null)
        {
            obj.SetActive(false);
            return;
        }

        characterShortcutInfoPanel.gameObject.SetActive(false);
        minionShortcutInfoPanel.gameObject.SetActive(false);
        if (allyUnit.AllyType == AllyType.Character)
        {
            KingSlimeData characterData = KingSlimeManager.Instance.GetKingSlimeData(KingSlime.Instance.kingslimeName );
            // thumImage.sprite = characterData.thum;
            // nameText.text = characterData.Title;
            
            attackTypePanel.SetAttackType(GetAttackType(KingSlime.Instance.weapon.attackType, KingSlime.Instance.shotModifier));
            characterShortcutInfoPanel.SetCharacter(KingSlime.Instance);
        }
        else
        {
            Minion minion = allyUnit.Transform.GetComponent<Minion>();
            // thumImage.sprite = minion.minionData.thum;
            // nameText.text = minion.minionData.Title;
            attackTypePanel.SetAttackType(GetAttackType(minion.minionData.attackType, minion.shotModifier));
            minionShortcutInfoPanel.SetMinion(minion);
        }
        obj.SetActive(true);
        // 유닛이 쓸 수 있는 설정 버튼만 노출 (각 버튼이 IsAvailable로 판단)
        setFindTargetButton.SetMinion(targetMinion);
        if (setBuffRangeButton != null)
            setBuffRangeButton.SetMinion(targetMinion);
        if (setRotationDirectionButton != null)
            setRotationDirectionButton.SetMinion(targetMinion);
    }

    // 공격 타입 변경 타일 위에 있으면 바뀐 타입을 표시
    AttackType GetAttackType(AttackType baseType, ShotModifier shotModifier)
    {
        return shotModifier.tileItem is AttackTypeTileItem attackTypeTileItem ? attackTypeTileItem.AttackType : baseType;
    }
}

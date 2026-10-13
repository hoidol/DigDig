using UnityEngine;

public class CharacterShortcutInfoPanel : MonoBehaviour
{
    public SlimeStatPanel[] statPanels;
    // public OwnItemPanel[] ownItemPanels;
    public void SetCharacter(Character character)
    {
        gameObject.SetActive(true);
        // for(int i = 0; i < ownItemPanels.Length; i++)
        // {
        //     ownItemPanels[i].gameObject.SetActive(false);
        // }

        // for(int i = 0; i < Character.Instance.itemInventory.ownItemKeys.Count; i++)
        // {
        //     ownItemPanels[i].SetItemData(ItemData.GetItemData(Character.Instance.itemInventory.ownItemKeys[i]));
        // }
        //공격력
        //공격 속도
        //공격 범위
        StatType[] displayStatTypes =
        {
            StatType.AttackPower,
            StatType.AttackSpeed,
            StatType.MaxHp,
            StatType.RecoveryHp,
            StatType.Dodge,
            StatType.CritChance,
            StatType.CritPower,
        };
        
        for(int i = 0; i < statPanels.Length; i++)
        {
            if (i < displayStatTypes.Length)
            {
                statPanels[i].gameObject.SetActive(true);
                statPanels[i].titleText.text = character.statMgr.statDic[displayStatTypes[i]].Title;
                statPanels[i].valueText.text = character.statMgr.statDic[displayStatTypes[i]].value.ToString();
                statPanels[i].thumImage.sprite = character.statMgr.statDic[displayStatTypes[i]].Thum;
            }
            else
            {
                statPanels[i].gameObject.SetActive(false);
            }
        }
    }
}

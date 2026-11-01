
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class SlimeStatPanel : MonoBehaviour
    {
        // public SlimeStatType statType;
        public TMP_Text titleText;
        public TMP_Text valueText;
        public Image thumImage;
        public void SetSlimeStat(Slime slime, SlimeStatType type)
        {
            if (thumImage != null)
                thumImage.sprite = slime.slimeData.thum;

            float randomRatio = slime.slimeData.GetRandomRatio(type);
            int mergeLv = slime.mergeLevel;
            SlimeStat slimeStat = slime.slimeData.GetSlimeStat(type, slime.userSlime.enhanceLevel);
            titleText.text = slimeStat.Title;
            // 버프 등이 반영된 현재 값 표시. statDic에 없는 고유 스탯은 데이터 값 표시
            valueText.text = TryGetStatType(type, out StatType statType)
                ? SlimeStat.FormatValue(slime.statDic[statType], randomRatio)
                : slimeStat.GetValueToString(mergeLv, randomRatio);
        }

        // SlimeStatType과 StatType은 enum 번호가 달라서 형변환하면 안 됨 (AttackSpeed=1 → MaxHp=1)
        static bool TryGetStatType(SlimeStatType type, out StatType statType)
        {
            switch (type)
            {
                case SlimeStatType.AttackPower: statType = StatType.AttackPower; return true;
                case SlimeStatType.AttackSpeed: statType = StatType.AttackSpeed; return true;
                case SlimeStatType.AttackRange: statType = StatType.AttackRange; return true;
                default: statType = default; return false;
            }
        }
    }

}
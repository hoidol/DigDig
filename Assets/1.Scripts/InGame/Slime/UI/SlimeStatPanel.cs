
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class SlimeStatPanel : MonoBehaviour
    {
        // public StatType statType;
        public TMP_Text titleText;
        public TMP_Text valueText;
        public Image thumImage;
        public void SetSlimeStat(Slime slime, StatType type)
        {
            if (thumImage != null)
                thumImage.sprite = StatData.GetThum(type);

            float randomRatio = slime.slimeData.GetRandomRatio(type);
            SlimeStat slimeStat = slime.slimeData.GetSlimeStat(type, slime.userSlime.enhanceLevel);
            if (slimeStat == null)
            {
                // 데이터에 없는 스탯 (displayStatTypes 설정 실수 등) → 패널 숨김
                Debug.LogWarning($"[SlimeStatPanel] {slime.slimeData.key} 에 {type} 스탯 데이터 없음");
                gameObject.SetActive(false);
                return;
            }

            titleText.text = slimeStat.Title;
            // 버프 등이 반영된 현재 값 표시. statDic에 없는 고유 스탯은 데이터 값 표시
            valueText.text = slime.statDic.TryGetValue(type, out float value)
                ? SlimeStat.FormatValue(value, randomRatio)
                : slimeStat.GetValueToString(slime.mergeLevel, randomRatio);
        }
    }

}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class MinionStatPanel : MonoBehaviour
    {
        // public StatType statType;
        public TMP_Text titleText;
        public TMP_Text valueText;
        public Image thumImage;
        public void SetMinionStat(Minion minion, StatType type)
        {
            if (thumImage != null)
                thumImage.sprite = StatData.GetThum(type);

            float randomRatio = minion.minionData.GetRandomRatio(type);
            MinionStat minionStat = minion.minionData.GetMinionStat(type, minion.userMinion.enhanceLevel);
            if (minionStat == null)
            {
                // 데이터에 없는 스탯 (displayStatTypes 설정 실수 등) → 패널 숨김
                Debug.LogWarning($"[MinionStatPanel] {minion.minionData.key} 에 {type} 스탯 데이터 없음");
                gameObject.SetActive(false);
                return;
            }

            titleText.text = minionStat.Title;
            // 버프 등이 반영된 현재 값 표시. statDic에 없는 고유 스탯은 데이터 값 표시
            valueText.text = minion.statDic.TryGetValue(type, out float value)
                ? MinionStat.FormatValue(value, randomRatio)
                : minionStat.GetValueToString(minion.mergeLevel, randomRatio);
        }
    }

}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 피해 집계 한 줄: 미니언 썸네일 + 바(1위 대비) + 피해량 + 비율(전체 대비)
public class DamageMeterSystemPanel : MonoBehaviour
{
    public Image thumImage;
    public Slider slider;        // 로비 미니언 경험치 바와 같은 Slider_02_Orange (조작 불가)
    public TMP_Text damageText;
    public TMP_Text percentText;

    MinionData minionData;

    public void SetData(MinionData data, double damage, double maxDamage, double totalDamage)
    {
        if (minionData != data)
        {
            minionData = data;
            if (thumImage != null)
                thumImage.SetThum(data.thum);
        }

        if (slider != null)
            slider.value = maxDamage > 0 ? (float)(damage / maxDamage) : 0f;
        if (damageText != null)
            damageText.text = FormatDamage(damage);
        if (percentText != null)
            percentText.text = totalDamage > 0 ? $"{damage / totalDamage * 100:0}%" : "0%";
    }

    // 1234 → 1234, 12345 → 12.3K, 1234567 → 1.2M
    public static string FormatDamage(double damage)
    {
        if (damage >= 1_000_000_000) return $"{damage / 1_000_000_000:0.#}B";
        if (damage >= 1_000_000) return $"{damage / 1_000_000:0.#}M";
        if (damage >= 10_000) return $"{damage / 1_000:0.#}K";
        return $"{damage:0}";
    }
}

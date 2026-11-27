using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 피해 집계 한 줄: 썸네일 + 바(1위 대비) + 피해량 + 비율(전체 대비). 미니언·슬라임 공용 (2026-11-27 슬라임 줄 추가)
// nameText·detailText는 선택 (결과 화면 줄에만 있음, 비우면 건너뜀)
public class DamageMeterSystemPanel : MonoBehaviour
{
    public Image thumImage;
    public Slider slider;        // 로비 미니언 경험치 바와 같은 Slider_02_Orange (조작 불가)
    public TMP_Text damageText;
    public TMP_Text percentText;
    public TMP_Text nameText;    // 이름 (용사 이름·미니언 이름)
    public TMP_Text detailText;  // 슬라임 = "스킬 N%"

    Sprite thum;

    public void SetData(DamageMeterSystem.Entry entry, double maxDamage, double totalDamage)
    {
        if (thum != entry.thum)
        {
            thum = entry.thum;
            if (thumImage != null)
                thumImage.SetThum(entry.thum);
        }
        if (nameText != null)
            nameText.text = entry.name;
        if (detailText != null)
        {
            detailText.gameObject.SetActive(!string.IsNullOrEmpty(entry.detail));
            detailText.text = entry.detail;
        }

        double damage = entry.damage;
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

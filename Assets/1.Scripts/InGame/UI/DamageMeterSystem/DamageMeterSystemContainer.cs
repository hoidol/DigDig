using UnityEngine;

// 피해 집계 창: 장착 미니언 + 킹(슬라임)을 패널에 하나씩, 피해가 큰 순서로 정렬해 보여준다 (2026-11-27 킹 줄 추가)
// 줄 수 = 장착 수 + 1 → 씬에 패널이 모자라면 경고만 (Tools/InGame/Add King Row To DamageMeter 로 패널 추가)
// 켜져 있는 동안 REFRESH_INTERVAL(실제 시간)마다 갱신 → 일시정지·튜토리얼 슬로우 중에도 갱신
public class DamageMeterSystemContainer : MonoBehaviour
{
    public const float REFRESH_INTERVAL = 0.5f;

    public DamageMeterSystemPanel[] damageMeterSystemPanels; // 비워 두면 하위 전부

    float refreshTimer;
    bool warned;

    void OnEnable()
    {
        Refresh();
    }

    void Update()
    {
        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer >= REFRESH_INTERVAL)
            Refresh();
    }

    public void Refresh()
    {
        refreshTimer = 0;
        if (damageMeterSystemPanels == null || damageMeterSystemPanels.Length == 0)
            damageMeterSystemPanels = GetComponentsInChildren<DamageMeterSystemPanel>(true);
        if (MinionManager.Instance == null || MinionManager.Instance.minionDataDic.Count == 0)
            return; // 미니언 데이터 로드 전

        var entries = DamageMeterSystem.GetEntries();
        if (entries.Count > damageMeterSystemPanels.Length && !warned)
        {
            warned = true;
            Debug.LogWarning($"[DamageMeterSystemContainer] 줄 {entries.Count}개에 패널 {damageMeterSystemPanels.Length}개 — 메뉴 Tools/InGame/Add King Row To DamageMeter");
        }
        double maxDamage = entries.Count > 0 ? entries[0].damage : 0;
        double totalDamage = DamageMeterSystem.AllDamage;

        for (int i = 0; i < damageMeterSystemPanels.Length; i++)
        {
            DamageMeterSystemPanel panel = damageMeterSystemPanels[i];
            if (panel == null)
                continue;

            bool active = i < entries.Count;
            panel.gameObject.SetActive(active);
            if (!active)
                continue;

            panel.transform.SetSiblingIndex(i);
            panel.SetData(entries[i], maxDamage, totalDamage);
        }
    }
}

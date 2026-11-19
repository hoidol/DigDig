using System.Linq;
using UnityEngine;

// 피해 집계 창: 장착 5종을 패널에 하나씩, 피해가 큰 순서로 정렬해 보여준다
// 켜져 있는 동안 REFRESH_INTERVAL(실제 시간)마다 갱신 → 일시정지·튜토리얼 슬로우 중에도 갱신
public class DamageMeterSystemContainer : MonoBehaviour
{
    public const float REFRESH_INTERVAL = 0.5f;

    public DamageMeterSystemPanel[] damageMeterSystemPanels; // 비워 두면 하위 전부

    float refreshTimer;

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

        UserMinion[] equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
        // 장착 순서를 기본으로 두고 피해가 큰 순서로 (같으면 장착 순서)
        MinionData[] datas = (equipped ?? new UserMinion[0])
            .Where(e => e != null && e.MinionData != null)
            .Select(e => e.MinionData)
            .Distinct()
            .Select((data, idx) => (data, idx))
            .OrderByDescending(e => DamageMeterSystem.GetDamage(e.data.key))
            .ThenBy(e => e.idx)
            .Select(e => e.data)
            .ToArray();

        double maxDamage = datas.Length > 0 ? DamageMeterSystem.GetDamage(datas[0].key) : 0;
        double totalDamage = DamageMeterSystem.TotalDamage;

        for (int i = 0; i < damageMeterSystemPanels.Length; i++)
        {
            DamageMeterSystemPanel panel = damageMeterSystemPanels[i];
            if (panel == null)
                continue;

            bool active = i < datas.Length;
            panel.gameObject.SetActive(active);
            if (!active)
                continue;

            panel.transform.SetSiblingIndex(i);
            panel.SetData(datas[i], DamageMeterSystem.GetDamage(datas[i].key), maxDamage, totalDamage);
        }
    }
}

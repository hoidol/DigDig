using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SynergyData", menuName = "Ability/SynergyData", order = 0)]
public class SynergyData : ScriptableObject
{
    public SynergyType synergyType;
    public SynergyResource[] abilitieResources;
    public string synergyAbilityKey;
    public string Name => synergyType.ToString();
    [System.Serializable]
    public class SynergyResource
    {
        public string key;
        public int count;
    }
    public bool CanPickSynergyAbility()
    {
        Ability sAbility = Player.Instance.abilityInventory.GetAbility(synergyAbilityKey);
        if (sAbility != null && sAbility.count > 0)
            return false;

        bool notEnough = abilitieResources.Any(e =>
            {
                Ability a = Player.Instance.abilityInventory.GetAbility(e.key);
                return a == null || a.count < e.count;
            });
        return !notEnough;
    }


#if UNITY_EDITOR
    public void LoadData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Json/SynergyData.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[SynergyData] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Split(',');
        for (int i = 0; i < headers.Length; i++)
            headers[i] = headers[i].Trim();

        int iType = System.Array.IndexOf(headers, "synergyType");
        int iSynergyAbility = System.Array.IndexOf(headers, "synergyAbility");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Split(',');

            string rowType = iType >= 0 && iType < cols.Length ? cols[iType].Trim() : "";
            if (rowType != synergyType.ToString()) continue;

            var resources = new System.Collections.Generic.List<SynergyResource>();
            for (int n = 1; n <= 5; n++)
            {
                // CSV 헤더가 ability3처럼 Key 없이 올 수도 있어서 둘 다 탐색
                int keyCol = -1;
                int countCol = -1;
                for (int h = 0; h < headers.Length; h++)
                {
                    string lower = headers[h].ToLower();
                    if (lower == $"ability{n}key" || lower == $"ability{n}")
                        keyCol = h;
                    else if (lower == $"ability{n}count")
                        countCol = h;
                }

                if (keyCol < 0 || countCol < 0 || keyCol >= cols.Length || countCol >= cols.Length) continue;

                string abilityKey = cols[keyCol].Trim();
                if (string.IsNullOrEmpty(abilityKey)) continue;
                if (!int.TryParse(cols[countCol].Trim(), out int abilityCount)) continue;

                resources.Add(new SynergyResource { key = abilityKey, count = abilityCount });
            }

            abilitieResources = resources.ToArray();
            if (iSynergyAbility >= 0 && iSynergyAbility < cols.Length)
                synergyAbilityKey = cols[iSynergyAbility].Trim();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[SynergyData] {synergyType} LoadData 완료 ({abilitieResources.Length}개 조건)");
            return;
        }

        Debug.LogWarning($"[SynergyData] CSV에서 '{synergyType}' 를 찾지 못함");
    }
#endif
}

public enum SynergyType
{
    Tracker,
    Gunner,
    Sniper,
    Berserker,
    Predator,
    Survivor,
    Counter,
    MinerHunter,
    Rampage,
    LuckyMan,
    Count
}
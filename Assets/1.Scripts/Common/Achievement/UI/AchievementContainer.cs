using UnityEngine;

// 업적 목록: 씬에는 패널을 1개만 두고(템플릿) 업적 수만큼 코드가 복사한다 → CSV 행이 늘어도 씬 수정 없음
public abstract class AchievementContainer : MonoBehaviour
{
    public AchievementPanel[] achievementPanels;

    // 이 목록에 보여줄 업적 (CSV 순서)
    protected abstract AchievementData[] GetAchievementDatas();

    public virtual void OpenContainer()
    {
        AchievementData[] datas = GetAchievementDatas();
        EnsurePanels(datas.Length);
        for (int i = 0; i < achievementPanels.Length; i++)
        {
            achievementPanels[i].SetAchievementData(i < datas.Length ? datas[i] : null);
        }
    }

    // 패널이 모자라면 첫 패널을 같은 부모 아래에 복사 (꺼진 패널도 포함해서 셈)
    void EnsurePanels(int count)
    {
        achievementPanels = GetComponentsInChildren<AchievementPanel>(true);
        if (achievementPanels.Length == 0)
        {
            if (count > 0)
                Debug.LogError($"[{name}] 업적 패널(템플릿)이 없음");
            return;
        }
        AchievementPanel template = achievementPanels[0];
        for (int i = achievementPanels.Length; i < count; i++)
        {
            Instantiate(template, template.transform.parent);
        }
        achievementPanels = GetComponentsInChildren<AchievementPanel>(true);
    }

    public virtual void UpdateContainer()
    {
        for (int i = 0; i < achievementPanels.Length; i++)
        {
            achievementPanels[i].UpdatePanel();
        }
    }
}

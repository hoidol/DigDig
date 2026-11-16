using System.Collections.Generic;
using UnityEngine;

// 다크니스 변신 목록: 쿨타임이 찬 다크니스(DarknessMinion.IsSkillReady)를 선택하면 그 다크니스 머리 위에 장착 미니언(다크니스 제외) 목록을 띄움
// 목록에서 하나를 누르면 그 종으로 변신 (머지 레벨 유지). 게임은 멈추지 않음, 선택을 해제하거나 드래그하면 숨김
// 씬: GameWorldCanvas 하위에 이 오브젝트(항상 켜 둠) → panel(보이는 부분) → optionTemplate(DarknessTransformOption, 복사해서 씀)
public class DarknessTransformPopup : MonoBehaviour
{
    public GameObject panel;                     // 켜고 끄는 부분 (이 오브젝트 자신은 이벤트를 받으려고 항상 켜 둠)
    public DarknessTransformOption optionTemplate;  // 선택지 하나 (panel 하위, 키 수만큼 복사)
    public Vector3 offset = new Vector3(0f, 0.6f, 0f); // 다크니스 StatePointPosition 기준 위치 (월드 단위)

    readonly List<DarknessTransformOption> options = new();
    DarknessMinion darkness;      // 선택된 다크니스 (쿨타임이 안 찼어도 기억 → 차는 순간 표시)
    DarknessMinion shownDarkness; // 지금 목록을 만든 다크니스

    void Awake()
    {
        if (optionTemplate != null)
        {
            optionTemplate.gameObject.SetActive(false);
            options.Add(optionTemplate);
        }
        Hide();
    }

    // GameManager.Awake의 GameEventBus.Clear 이후에 구독되도록 Start에서
    void Start()
    {
        GameEventBus.Subscribe<SetTargetEvent>(OnSetTarget);
        GameEventBus.Subscribe<ReleaseTargetEvent>(OnReleaseTarget);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<SetTargetEvent>(OnSetTarget);
        GameEventBus.Unsubscribe<ReleaseTargetEvent>(OnReleaseTarget);
    }

    void OnSetTarget(SetTargetEvent e) => darkness = e.unit as DarknessMinion;

    void OnReleaseTarget(ReleaseTargetEvent e)
    {
        if (ReferenceEquals(e.unit, darkness))
            darkness = null;
    }

    void LateUpdate()
    {
        bool show = darkness != null && darkness.gameObject.activeInHierarchy && darkness.IsSkillReady && !darkness.IsDragging;
        if (!show)
        {
            Hide();
            return;
        }
        if (shownDarkness != darkness)
            Build(darkness);
        transform.position = darkness.StatePointPosition + offset;
    }

    void Build(DarknessMinion target)
    {
        shownDarkness = target;
        List<string> keys = target.GetTransformKeys();
        while (options.Count < keys.Count && optionTemplate != null)
            options.Add(Instantiate(optionTemplate, optionTemplate.transform.parent, false));
        for (int i = 0; i < options.Count; i++)
        {
            bool active = i < keys.Count;
            options[i].gameObject.SetActive(active);
            if (active)
                options[i].Set(this, MinionManager.Instance.GetMinionData(keys[i]));
        }
        if (panel != null)
            panel.SetActive(true);
    }

    void Hide()
    {
        shownDarkness = null;
        if (panel != null && panel.activeSelf)
            panel.SetActive(false);
    }

    public void Select(string key)
    {
        DarknessMinion target = darkness;
        Hide();
        if (target != null)
            target.TransformTo(key);
    }
}

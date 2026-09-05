using UnityEngine;

public class PhaseStepContainer : MonoBehaviour
{
    [SerializeField] PhaseStepPanel[] phaseStepPanels;

    void Awake()
    {
        phaseStepPanels = GetComponentsInChildren<PhaseStepPanel>();

    }
    void Start()
    {
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
        GameEventBus.Subscribe<BreakStartEvent>(OnDayStartEvent);
        GameEventBus.Subscribe<PhaseEndEvent>(OnPhaseEndEvent);
    }

    void OnStartGameEvent(StartGameEvent e)
    {
        for (int i = 0; i < phaseStepPanels.Length; i++)
        {
            phaseStepPanels[i].Init(i);
        }
    }
    BreakStartEvent dayStartEvent;
    void OnDayStartEvent(BreakStartEvent e)
    {
        dayStartEvent = e;
        UpdateContainer();
    }

    void UpdateContainer()
    {
        for (int i = 0; i < phaseStepPanels.Length; i++)
        {
            if (phaseStepPanels[i].idx == dayStartEvent.phaseIdx)
            {
                phaseStepPanels[i].UpdatePanel(dayStartEvent.phaseIdx);
            }

        }
    }
    void OnPhaseEndEvent(PhaseEndEvent e)
    {
        for (int i = 0; i < phaseStepPanels.Length; i++)
        {
            if (phaseStepPanels[i].idx == e.phaseIdx)
            {
                phaseStepPanels[i].Clear();
                break;
            }
        }
        UpdateContainer();
    }
}
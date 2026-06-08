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
        GameEventBus.Subscribe<PhaseStartEvent>(OnPhaseStartEvent);
        GameEventBus.Subscribe<PhaseEndEvent>(OnPhaseEndEvent);
    }

    void OnStartGameEvent(StartGameEvent e)
    {
        for (int i = 0; i < phaseStepPanels.Length; i++)
        {
            phaseStepPanels[i].Init(i);
        }
    }
    PhaseStartEvent phaseStartEvent;
    void OnPhaseStartEvent(PhaseStartEvent e)
    {
        phaseStartEvent = e;
        UpdateContainer();
    }

    void UpdateContainer()
    {
        for (int i = 0; i < phaseStepPanels.Length; i++)
        {
            if (phaseStepPanels[i].idx == phaseStartEvent.phaseIdx)
            {
                phaseStepPanels[i].UpdatePanel(phaseStartEvent.phaseIdx);
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
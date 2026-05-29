using UnityEngine;

public class OrdealStepContainer : MonoBehaviour
{
    [SerializeField] OrdealStepPanel[] ordealStepPanels;

    void Awake()
    {
        ordealStepPanels = GetComponentsInChildren<OrdealStepPanel>();

    }
    void Start()
    {
        GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
        GameEventBus.Subscribe<OrdealStartEvent>(OnOrdealStartEvent);
        GameEventBus.Subscribe<OrdealEndEvent>(OnOrdealEndEvent);
    }

    void OnStartGameEvent(StartGameEvent e)
    {
        for (int i = 0; i < ordealStepPanels.Length; i++)
        {
            ordealStepPanels[i].Init(i);
        }
    }
    OrdealStartEvent ordealStartEvent;
    void OnOrdealStartEvent(OrdealStartEvent e)
    {
        ordealStartEvent = e;
        UpdateContainer();
    }

    void UpdateContainer()
    {
        for (int i = 0; i < ordealStepPanels.Length; i++)
        {
            if (ordealStepPanels[i].idx == ordealStartEvent.ordealProgressData.clearCount)
            {
                ordealStepPanels[i].UpdatePanel(ordealStartEvent.ordealProgressData.clearCount);
            }

        }
    }
    void OnOrdealEndEvent(OrdealEndEvent e)
    {
        for (int i = 0; i < ordealStepPanels.Length; i++)
        {
            if (ordealStepPanels[i].idx == e.ordealClearCount - 1)
            {
                ordealStepPanels[i].Clear();
                break;
            }
        }
        UpdateContainer();
    }
}
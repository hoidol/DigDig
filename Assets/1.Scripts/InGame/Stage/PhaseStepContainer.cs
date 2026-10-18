// using UnityEngine;

// public class PhaseStepContainer : MonoBehaviour
// {
//     [SerializeField] PhaseStepPanel[] phaseStepPanels;

//     void Awake()
//     {
//         phaseStepPanels = GetComponentsInChildren<PhaseStepPanel>();

//     }
//     void Start()
//     {
//         GameEventBus.Subscribe<StartGameEvent>(OnStartGameEvent);
//         GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
//         GameEventBus.Subscribe<PhaseEndEvent>(OnPhaseEndEvent);
//     }

//     void OnStartGameEvent(StartGameEvent e)
//     {
//         for (int i = 0; i < phaseStepPanels.Length; i++)
//         {
//             phaseStepPanels[i].Init(i);
//         }
//     }
//     int curPhaseIdx = -1;
//     void OnWaveStartEvent(WaveStartEvent e)
//     {
//         curPhaseIdx = e.phaseIdx;
//         UpdateContainer();
//     }

//     void UpdateContainer()
//     {
//         for (int i = 0; i < phaseStepPanels.Length; i++)
//         {
//             if (phaseStepPanels[i].idx == curPhaseIdx)
//             {
//                 phaseStepPanels[i].UpdatePanel(curPhaseIdx);
//             }

//         }
//     }
//     void OnPhaseEndEvent(PhaseEndEvent e)
//     {
//         for (int i = 0; i < phaseStepPanels.Length; i++)
//         {
//             if (phaseStepPanels[i].idx == e.phaseIdx)
//             {
//                 phaseStepPanels[i].Clear();
//                 break;
//             }
//         }
//         UpdateContainer();
//     }
// }
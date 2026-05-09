using UnityEngine;
using UnityEngine.UI;

// 씬의 UI Canvas에 배치. IronNestAbility가 준비되면 활성화되는 소환 버튼
public class IronNestSpawnButton : MonoBehaviour
{
    [SerializeField] Button button;

    void Awake()
    {
        button.onClick.AddListener(OnClick);
        button.gameObject.SetActive(false);
    }
    void Start()
    {
        GameEventBus.Subscribe<IronNestReadyEvent>(OnReady);
        // GameEventBus.Subscribe<UndergroundStartEvent>(OnUndergroundStartEvent);
    }

    // void OnUndergroundStartEvent(UndergroundStartEvent e)
    // {
    //     button.gameObject.SetActive(false);
    // }

    void OnReady(IronNestReadyEvent e)
    {
        button.gameObject.SetActive(true);
    }

    void OnClick()
    {
        button.gameObject.SetActive(false);
        GameEventBus.Publish(new IronNestSpawnRequestEvent());
    }
}

using UnityEngine;
using UnityEngine.UI;

// (2026-11-23) 미션 버튼: InGame 씬 GameCanvas/Panel/MissionButton (피해 집계 버튼 바로 아래). 누르면 미션 창(MissionPanel, InGameMissionPanel) 열기/닫기
// 미션 창과 피해 집계 창은 자리가 겹쳐서 하나를 열면 다른 하나는 닫음
// 씬 구성은 메뉴 Tools/Mission/Add Mission UI To InGame Scene (InGameMissionUIBuilder)
public class InGameMissionButton : MonoBehaviour
{
    public GameObject panel;                     // MissionPanel
    public DamageMeterSystemButton damageButton; // 비우면 씬에서 찾음

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        if (damageButton == null)
            damageButton = FindFirstObjectByType<DamageMeterSystemButton>(FindObjectsInactive.Include);
        if (damageButton != null)
            damageButton.GetComponent<Button>().onClick.AddListener(Hide); // 피해 집계 창을 열면 미션 창은 닫음
        if (panel != null)
            panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (damageButton != null)
            damageButton.GetComponent<Button>().onClick.RemoveListener(Hide);
    }

    void OnClick()
    {
        if (panel == null)
            return;
        bool open = !panel.activeSelf;
        panel.SetActive(open);
        if (open && damageButton != null && damageButton.container != null)
            damageButton.container.gameObject.SetActive(false);
    }

    void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}

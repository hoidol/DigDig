using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-27) 로비 [이어하기] 버튼 (BattleCanvas, 메뉴 Tools/Lobby/Add Multi Game Buttons)
// 저장된 싱글 판(LocalGameSave, 마지막 브레이크타임)이 있으면 보임 → 누르면 그 판으로 (에너지 소모 없음). 멀티 대기 중이어도 가능
[RequireComponent(typeof(Button))]
public class LocalResumeButton : MonoBehaviour
{
    public GameObject root; // 보이고 숨길 대상 (비우면 이 오브젝트)

    void Awake() => GetComponent<Button>().onClick.AddListener(OnClicked);

    void OnEnable() => Refresh();

    public void Refresh()
    {
        GameObject target = root != null ? root : gameObject;
        bool show = LocalGameSave.HasSave;
        if (target == gameObject)
            GetComponent<Button>().interactable = show; // 자기 자신을 끄면 다시 켤 수 없으니 버튼만 잠금
        else
            target.SetActive(show);
    }

    void OnClicked()
    {
        if (!LocalGameSave.RequestResume())
        {
            Refresh();
            return;
        }
        FadeCanvs.Instance.FadeOutIn(null, () => SceneManager.LoadScene("InGame"));
    }
}

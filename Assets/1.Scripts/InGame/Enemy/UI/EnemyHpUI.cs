using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 적 머리 위 체력바 하나. EnemyWorldCanvas 하위에 템플릿으로 하나 두면 적마다 복사해서 씀 (풀링)
// thumImage = 체력 비율만큼 채우는 바 이미지 (씬: HpBarBg > HpBar). Image Type이 Filled가 아니면 가로 Filled로 바꿔서 씀
public class EnemyHpUI : MonoBehaviour
{
    public Image thumImage;
    public TMP_Text hpText; // 선택. 있으면 현재 체력 숫자
    public Vector3 offset;  // 적 hpPoint 기준 추가 위치 (월드 단위)

    public Enemy Target { get; private set; }
    RectTransform rect;
    Canvas canvas;

    void Awake()
    {
        rect = transform as RectTransform;
        canvas = GetComponentInParent<Canvas>();
        if (thumImage != null && thumImage.type != Image.Type.Filled)
        {
            thumImage.type = Image.Type.Filled;
            thumImage.fillMethod = Image.FillMethod.Horizontal;
            thumImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }

    public void Bind(Enemy enemy)
    {
        Target = enemy;
        gameObject.SetActive(true);
        Refresh();
        FollowTarget();
    }

    public void Unbind()
    {
        Target = null;
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        if (Target == null)
            return;
        float ratio = Target.MaxHp > 0 ? Mathf.Clamp01(Target.CurHp / Target.MaxHp) : 0f;
        if (thumImage != null)
            thumImage.fillAmount = ratio;
        if (hpText != null)
            hpText.text = Target.CurHp > 0 && Target.CurHp < 1 ? "1" : ((int)Target.CurHp).ToString();
    }

    // 적이 움직인 뒤 따라가도록 LateUpdate
    void LateUpdate()
    {
        if (Target == null) // Unbind 없이 적이 파괴됨 (Unity null)
        {
            if (!ReferenceEquals(Target, null))
                EnemyWorldCanvas.Instance.Release(Target);
            return;
        }
        if (!Target.gameObject.activeInHierarchy) // Deactivate를 거치지 않고 꺼짐
        {
            EnemyWorldCanvas.Instance.Release(Target);
            return;
        }
        FollowTarget();
    }

    void FollowTarget()
    {
        Vector3 worldPos = Target.HpPoint.position + offset;
        if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = worldPos;
            return;
        }
        // Screen Space 캔버스면 화면 좌표로 변환
        Camera cam = Camera.main;
        if (cam == null)
            return;
        Vector2 screen = cam.WorldToScreenPoint(worldPos);
        RectTransform parent = rect.parent as RectTransform;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (parent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCam, out Vector2 local))
            rect.anchoredPosition = local;
    }
}

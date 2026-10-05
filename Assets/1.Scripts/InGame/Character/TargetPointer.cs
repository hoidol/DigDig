using UnityEngine;
using UnityEngine.EventSystems;

// UI가 아닌 월드 영역 클릭 시 클릭 위치에 targetIcon 표시
// 누르고 있는 동안 계속 표시, 손을 떼면 time초 후 비활성화
public class TargetPointer : MonoSingleton<TargetPointer>
{
    public GameObject targetIcon;
    public float time = 1f;

    public bool isPressing;
    float hideTimer;


    void Start()
    {
        targetIcon.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            isPressing = true;
            targetIcon.SetActive(true);
            SetIconPosition();
        }

        if (isPressing)
        {
            SetIconPosition();
            if (Input.GetMouseButtonUp(0))
            {
                isPressing = false;
                hideTimer = time;
            }
            return;
        }

        if (targetIcon.activeSelf)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
                targetIcon.SetActive(false);
        }
    }

    void SetIconPosition()
    {

        Vector3 pos = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        pos.z = targetIcon.transform.position.z;
        targetIcon.transform.position = pos;
    }
    public Vector2 Direction()
    {
        Vector2 pos = CameraManager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return pos - (Vector2)Character.Instance.transform.position;
    }
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return EventSystem.current.IsPointerOverGameObject();
    }
}

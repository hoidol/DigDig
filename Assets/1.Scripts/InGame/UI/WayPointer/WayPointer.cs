using UnityEngine.UI;
using UnityEngine;

public class WayPointer : MonoBehaviour
{
    Camera mainCamera;
    public GameObject rootTr;
    //[SerializeField] Camera uiCamara;
    void Start()
    {
        mainCamera = Camera.main;
    }
    public IWayPointerTarget wayPointerTarget;
    public Transform directionTr;
    public Image thumImage;
    public Image timerImage;

    public void SetTarget(IWayPointerTarget target)
    {
        Debug.Log("WayPointer SetTarget()");
        wayPointerTarget = target;
    }

    // Update is called once per frame
    void Update()
    {
        timerImage.fillAmount = wayPointerTarget.CurTimer / wayPointerTarget.MaxTime;
        thumImage.sprite = wayPointerTarget.Thum;

        Vector3 toPos = wayPointerTarget.Transform.position;
        Vector3 fromPos = mainCamera.transform.position;
        fromPos.z = 0;
        Vector3 dir = (toPos - fromPos).normalized;
        float angle = Util.GetAngleFromVector(dir);
        directionTr.localEulerAngles = new Vector3(0, 0, angle);

        float borderSize = 200f;
        Vector3 targetPosScreenPoint = mainCamera.WorldToScreenPoint(wayPointerTarget.Transform.position);
        bool isOffScreen = targetPosScreenPoint.x <= borderSize ||
        targetPosScreenPoint.x >= Screen.width - borderSize ||
        targetPosScreenPoint.y <= borderSize ||
        targetPosScreenPoint.y >= Screen.height - borderSize;

        if (isOffScreen)
        {
            rootTr.SetActive(true);
            Vector3 cappedTargetScreenPos = targetPosScreenPoint;
            if (cappedTargetScreenPos.x <= borderSize) cappedTargetScreenPos.x = borderSize;
            if (cappedTargetScreenPos.x >= Screen.width - borderSize) cappedTargetScreenPos.x = Screen.width - borderSize;
            if (cappedTargetScreenPos.y <= borderSize) cappedTargetScreenPos.y = borderSize;
            if (cappedTargetScreenPos.y >= Screen.height - borderSize) cappedTargetScreenPos.y = Screen.height - borderSize;

            Vector3 pointerWorldPos = mainCamera.ScreenToWorldPoint(cappedTargetScreenPos);
            transform.position = pointerWorldPos;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        }
        else
        {
            rootTr.SetActive(false);

        }
    }
}

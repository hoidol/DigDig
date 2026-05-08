using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class WayPointerCanvas : CanvasUI<WayPointerCanvas>
{
    // [SerializeField] Camera uiCamara;
    // [SerializeField] Transform targetTr;
    // [SerializeField] RectTransform pointerRectTr;
    // Camera mainCamera;
    List<WayPointer> wayPointers = new List<WayPointer>(); //풀링으로 처리하기 
    public WayPointer wayPointerPrefab;
    public Transform parentTr;
    void Awake()
    {
        // mainCamera = Camera.main;
        //targetPos = new Vector3(200, 45);
        //pointerRectTr = transform.Find("Pointer").GetComponent<RectTransform>();
    }

    public WayPointer AddWayPoint(IWayPointerTarget wayPointerTarget)
    {
        Debug.Log("WayPointerCanvas AddWayPoint");
        WayPointer wayPointer = Instantiate(wayPointerPrefab, parentTr);
        wayPointer.SetTarget(wayPointerTarget);
        wayPointers.Add(wayPointer);
        return wayPointer;
    }
    public void Remove(IWayPointerTarget wayPointerTarget)
    {
        for (int i = 0; i < wayPointers.Count; i++)
        {
            if (wayPointers[i].wayPointerTarget == wayPointerTarget)
            {
                WayPointer wP = wayPointers[i];
                wayPointers.Remove(wP);
                Destroy(wP.gameObject);
                break;
            }
        }
    }

    // void Update()
    // {
    //     Vector3 toPos = targetTr.position;
    //     Vector3 fromPos = mainCamera.transform.position;
    //     fromPos.z = 0;
    //     Vector3 dir = (toPos - fromPos).normalized;
    //     float angle = Util.GetAngleFromVector(dir);
    //     pointerRectTr.localEulerAngles = new Vector3(0, 0, angle);

    //     float borderSize = 50f;
    //     Vector3 targetPosScreenPoint = mainCamera.WorldToScreenPoint(targetTr.position);
    //     bool isOffScreen = targetPosScreenPoint.x <= borderSize ||
    //     targetPosScreenPoint.x >= Screen.width - borderSize ||
    //     targetPosScreenPoint.y <= borderSize ||
    //     targetPosScreenPoint.y >= Screen.height - borderSize;

    //     if (isOffScreen)
    //     {
    //         pointerRectTr.gameObject.SetActive(true);
    //         Vector3 cappedTargetScreenPos = targetPosScreenPoint;
    //         if (cappedTargetScreenPos.x <= borderSize) cappedTargetScreenPos.x = borderSize;
    //         if (cappedTargetScreenPos.x >= Screen.width - borderSize) cappedTargetScreenPos.x = Screen.width - borderSize;
    //         if (cappedTargetScreenPos.y <= borderSize) cappedTargetScreenPos.y = borderSize;
    //         if (cappedTargetScreenPos.y >= Screen.height - borderSize) cappedTargetScreenPos.y = Screen.height - borderSize;

    //         Vector3 pointerWorldPos = uiCamara.ScreenToWorldPoint(cappedTargetScreenPos);
    //         pointerRectTr.position = pointerWorldPos;
    //         pointerRectTr.localPosition = new Vector3(pointerRectTr.localPosition.x, pointerRectTr.localPosition.y, 0);
    //     }
    //     else
    //     {
    //         pointerRectTr.gameObject.SetActive(false);
    //     }
    // }
}

//public class 
using UnityEngine.UI;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
public class WayPointer : MonoBehaviour
{
    Camera mainCamera;
    public GameObject rootTr;

    public IWayPointerTarget wayPointerTarget;
    public Transform directionTr;
    public Image thumImage;
    public Image timerImage;
    public TMP_Text distanceText;

    bool isPlayingEffect;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Show(IWayPointerTarget target, bool effect = false, float delay = 0)
    {
        wayPointerTarget = target;
        if (effect)
            PlayEffect(delay).Forget();
    }

    async UniTaskVoid PlayEffect(float delay)
    {
        isPlayingEffect = true;
        rootTr.SetActive(true);

        Vector3 centerScreen = new(Screen.width * 0.5f, Screen.height * 0.5f, 1f);
        Vector3 centerWorld = mainCamera.ScreenToWorldPoint(centerScreen);
        centerWorld.z = 0f;
        transform.position = centerWorld;

        var token = this.GetCancellationTokenOnDestroy();
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

        float duration = 0.4f;
        float elapsed = 0f;
        Vector3 fromPos = centerWorld;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 toPos = CalcPointerWorldPos();
            transform.position = Vector3.Lerp(fromPos, toPos, elapsed / duration);
            await UniTask.Yield(cancellationToken: token);
        }

        isPlayingEffect = false;
    }

    Vector3 CalcPointerWorldPos()
    {
        float borderSize = 100f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(wayPointerTarget.Transform.position);
        if (screenPos.x <= borderSize) screenPos.x = borderSize;
        if (screenPos.x >= Screen.width - borderSize) screenPos.x = Screen.width - borderSize;
        if (screenPos.y <= borderSize) screenPos.y = borderSize;
        if (screenPos.y >= Screen.height - borderSize) screenPos.y = Screen.height - borderSize;
        Vector3 world = mainCamera.ScreenToWorldPoint(screenPos);
        world.z = 0f;
        return world;
    }

    void Update()
    {
        if (wayPointerTarget == null) return;

        timerImage.fillAmount = wayPointerTarget.CurTimer / wayPointerTarget.MaxTime;
        thumImage.sprite = wayPointerTarget.Thum;
        distanceText.text = $"{Vector2.Distance(Player.Instance.bodyCenterTr.position, wayPointerTarget.Transform.position):F1}M";

        Vector3 toPos = wayPointerTarget.Transform.position;
        Vector3 fromPos = mainCamera.transform.position;
        fromPos.z = 0;
        Vector3 dir = (toPos - fromPos).normalized;
        float angle = Util.GetAngleFromVector(dir);
        directionTr.localEulerAngles = new Vector3(0, 0, angle);

        if (isPlayingEffect) return;

        float borderSize = 150f;
        Vector3 targetPosScreenPoint = mainCamera.WorldToScreenPoint(wayPointerTarget.Transform.position);
        bool isOffScreen = targetPosScreenPoint.x <= borderSize ||
            targetPosScreenPoint.x >= Screen.width - borderSize ||
            targetPosScreenPoint.y <= borderSize ||
            targetPosScreenPoint.y >= Screen.height - borderSize;

        if (isOffScreen)
        {
            rootTr.SetActive(true);
            transform.position = CalcPointerWorldPos();
        }
        else
        {
            rootTr.SetActive(false);
        }
    }
}

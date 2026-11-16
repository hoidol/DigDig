using UnityEngine;

// 탄 그림을 계속 돌림 (던진 부적처럼 빙글빙글). 탄 루트는 진행 방향을 바라보므로 그림 자식에 붙인다
public class BulletSpin : MonoBehaviour
{
    [Tooltip("초당 회전 각도 (음수 = 시계 방향)")]
    public float degreesPerSecond = -720f;

    void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    void Update()
    {
        transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}

using UnityEngine;

public class WaveWarningPointer : MonoBehaviour, IWayPointerTarget
{
    public Transform Transform => transform;

    Vector2 moveDirection;
    float moveSpeed;
    public Sprite sprite;
    public Rigidbody2D rd2d;
    public void Spawn(Vector2 pos, Vector2 moveDir, float moveSpeed)
    {
        transform.position = pos;
        moveDirection = moveDir;
        this.moveSpeed = moveSpeed;
        rd2d.linearVelocity = moveDir * moveSpeed;
        Appear(pos);
    }
    void Update()
    {
        // Vector2.zero 지점을 지나치면 멈춤
        if (rd2d.linearVelocity != Vector2.zero && Vector2.Dot((Vector2)transform.position, moveDirection) >= 0f)
        {
            rd2d.linearVelocity = Vector2.zero;
            transform.position = Vector2.zero;
        }

        if(Vector2.Distance(transform.position,Vector2.zero) < CameraManager.Instance.mainCamera.orthographicSize - 3f)
        {
            Destroy();
        }
    }

    public void Appear(Vector2 spawnPos)
    {
        WayPointerCanvas.Instance.AddWayPoint(this);
    }

    public void Destroy()
    {
        gameObject.SetActive(false);
        WayPointerCanvas.Instance.Remove(this);
    }

    public Sprite GetThum()
    {
        return sprite;
    }
    
    public static WaveWarningPointerPoolingSystem poolingSystem = new();
    public static WaveWarningPointer Instantiate(Vector2 pos)
    {
        return poolingSystem.Get(pos);
    }

}

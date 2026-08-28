using UnityEngine;

public class SteelSphereBulletObject : CharacterBulletObject
{
    static readonly float[] DAMAGE_PER_REDUCED_BOUNCE = { 0.5f, 0.75f, 1f };

    int remaining;

    // public override void Shoot(Vector2 dir)
    // {
    //     base.Shoot(dir);

    //     // int bounce = Character.Instance.statMgr.Bounce;
    //     // remaining = Mathf.FloorToInt(bounce * 0.5f);

    //     // int lv = Player.Instance.statMgr.bulletStatDic[key].lv;
    //     // AddBulletForce(new DamageBoostForce(DAMAGE_PER_REDUCED_BOUNCE[lv - 1]));
    // }

    public override void CheckHit()
    {
        if (CheckReachedEndScreen()) return;

        RaycastHit2D enemyHit = GetRaycastHit2D();
        if (enemyHit) Hit(enemyHit);
    }

    bool CheckReachedEndScreen()
    {
        Vector3 vp = CameraManager.Instance.mainCamera.WorldToViewportPoint(transform.position);

        bool hitEdge = vp.x <= 0f || vp.x >= 1f || vp.y <= 0f || vp.y >= 1f;
        if (!hitEdge) return false;

        if (remaining-- <= 0) { Release(); return true; }

        if (vp.x <= 0f || vp.x >= 1f) direction = new Vector3(-direction.x, direction.y, 0).normalized;
        if (vp.y <= 0f || vp.y >= 1f) direction = new Vector3(direction.x, -direction.y, 0).normalized;
        transform.right = direction;

        // 화면 안으로 위치 보정
        vp.x = Mathf.Clamp(vp.x, 0.001f, 0.999f);
        vp.y = Mathf.Clamp(vp.y, 0.001f, 0.999f);
        transform.position = CameraManager.Instance.mainCamera.ViewportToWorldPoint(vp);

        return true;
    }
}

// [폭격] 폭격 슬라임 폭탄
// 발사 위치에서 targetPos(깃발)까지 포물선으로 날아가 떨어진 자리에서 폭발 → boomRange 안 적 최대 maxHitCount기 피해
// 날아가는 동안은 적과 부딪히지 않음
using UnityEngine;

public class MortarBulletSpec : AllyBulletSpec
{
    public float boomRange = 2.5f;
    public int maxHitCount = 5;
    public Vector2 targetPos;       // 떨어지는 지점 (깃발)
    public float flightTime = 0.6f; // 발사 → 착탄 시간
    public float arcHeight = 1.5f;  // 포물선 최고 높이
    public LayerMask hitLayerMask;

    public MortarBulletSpec()
    {
        key = "Mortar";
    }
}

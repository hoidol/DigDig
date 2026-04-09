using UnityEngine;

// 발사 전 준비 신호 — spread 요청, 조건 세팅 등
// Player.Attack()에서 Shoot() 호출 전에 처리됨
public interface IPreAttack
{
    void OnPreAttack(Player player, Vector2 dir);
}

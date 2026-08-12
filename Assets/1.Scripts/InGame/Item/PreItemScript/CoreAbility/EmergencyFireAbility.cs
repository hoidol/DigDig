// using UnityEngine;
// using Cysharp.Threading.Tasks;

// // 긴급 호출 - 마지막 탄 발사 시 360도 방향으로 방사형 탄 발사
// public class EmergencyFireAbility : Ability, IFired, IPreFire
// {
//     const int BULLET_COUNT = 5;

//     public override string GetDescription(bool detail = false)
//     {
//         return $"모든 총알 소비 후 방사형으로 {BULLET_COUNT}탄 난사";
//     }

//     public void OnPreFire(ref Bullet bullet,  Vector2 dir)
//     {

//     }
//     public void OnFired(ref Bullet bullet, ref CharacterBulletObject playerBulletObject, Vector2 dir)
//     {
//         // if (Player.Instance.weapon.IsReloading) return;
//         //if (player.curBulletCount != 1) return; //마지막 탄인지 확인

//         FireRadial(dir).Forget();
//     }

//     async UniTaskVoid FireRadial(Vector2 dir)
//     {
//         float angleStep = 360f / BULLET_COUNT;
//         float baseAngle = Vector2.SignedAngle(Vector2.right, dir);
//         for (int i = 0; i < BULLET_COUNT; i++)
//         {
//             float rad = (baseAngle + angleStep * i) * Mathf.Deg2Rad;
//             Vector2 shootDir = new(Mathf.Cos(rad), Mathf.Sin(rad));
//             Character.Instance.weapon.Shoot(new NormalBullet(), shootDir);
//             await UniTask.Delay(30);
//         }
//     }

//     public override void OnUnequip(Character player) { }

// }

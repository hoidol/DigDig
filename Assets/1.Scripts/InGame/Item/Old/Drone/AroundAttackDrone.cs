// using System.Collections;
// using UnityEngine;

// // 360도 공격 드론: 주변 적을 자동으로 공격
// public class AroundAttackDrone : Drone
// {

//     public float attackInterval = 0.5f;
//     public float damage;
//     public Transform dirTr;

//     public override void Spawn(Vector2 pos)
//     {
//         base.Spawn(pos);
//         StopAllCoroutines();
//         StartCoroutine(CoRotate());
//         StartCoroutine(AttackLoop());
//     }

//     public float rotateSpeed = 90f; // 초당 회전 각도

//     IEnumerator CoRotate()
//     {
//         while (true)
//         {
//             dirTr.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
//             yield return null;
//         }
//     }

//     IEnumerator AttackLoop()
//     {
//         while (true)
//         {
//             yield return new WaitForSeconds(attackInterval);
//             Fire();
//         }
//     }

//     void Fire()
//     {
//         // float baseAngle = Vector2.SignedAngle(Vector2.right, Random.insideUnitCircle.normalized);
//         // float angleStep = 360f / (3 + level);
//         // for (int i = 0; i < 3 + level; i++)
//         // {
//         //     float rad = (baseAngle + angleStep * i) * Mathf.Deg2Rad;
//         //     Vector2 dir = new(Mathf.Cos(rad), Mathf.Sin(rad));
//         //     AllyBullet b = AllyBullet.Instantiate();
//         //     b.transform.position = transform.position;
//         //     b.Shoot(dir, damage);
//         // }
//         damage = Character.Instance.statMgr.AttackPower;

//         Vector2 dir = dirTr.up;
//         // AllyBullet b = AllyBullet.Instantiate();
//         // b.transform.position = transform.position;
//         // b.Shoot(dir);
//         // b.damage = damage;
//     }
// }

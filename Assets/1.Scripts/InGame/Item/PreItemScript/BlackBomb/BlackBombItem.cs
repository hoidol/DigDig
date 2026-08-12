// using UnityEngine;

// // [검은 폭탄]
// // 쿨타임마다 랜덤 방향으로 BlackBomb을 투척. 폭발 데미지는 마력의 50%.
// public class BlackBombItem : TriggerItem
// {
//     const float DAMAGE_RATE = 0.5f;

//     public BlackBomb bombPrefab;

//     public override void OnTrigger()
//     {
//         base.OnTrigger();
//         float damage = Character.Instance.statMgr.AttackPower * DAMAGE_RATE;
//         Vector2 randomDir = Random.insideUnitCircle.normalized;

//         var bomb = Instantiate(bombPrefab);
//         bomb.transform.position = Character.Instance.transform.position;
//         bomb.Shoot(randomDir, damage);
//     }

//     public override string GetDescription(int lv = 1,bool detail = false)
//     {
//         return $"랜덤 방향으로 폭탄 투척, 멈추면 폭발 (마력의 {DAMAGE_RATE * 100:0}% 데미지)";
//     }
// }

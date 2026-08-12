// using UnityEngine;

// public class CrystalExplosionItem : Item
// {
//     public float explosionRadius = 3f;
//     public float explosionDamage = 20f;
//     public LayerMask enemyLayer;

//     public override void OnEquip()
//     {
//         GameEventBus.Subscribe<DestroyedStoneEvent>(OnDestroyedStone);
//     }

//     public override void OnUnequip()
//     {
//         GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnDestroyedStone);
//     }

//     void OnDestroyedStone(DestroyedStoneEvent e)
//     {
//         InGameUtil.DamageEnemies(
//             e.stone.transform.position,
//             explosionRadius * count,
//             explosionDamage * count,
//             enemyLayer
//         );
//     }
// }

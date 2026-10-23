// public class RedScarfItem : Item
// {
//     Buff critBuff;
//     bool pendingCrit;

//     public override void OnEquip()
//     {
//         critBuff = new Buff(StatType.CritChance, 1f, StatOpType.Add); // CritChance 100% 추가
//         GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
//         GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFired);
//     }

//     public override void OnUnequip()
//     {
//         GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);
//         GameEventBus.Unsubscribe<BulletFiredEvent>(OnBulletFired);
//         RemoveCritBuff();
//     }

//     void OnEnemyDead(EnemyDeadEvent e)
//     {
//         pendingCrit = true;
//         Character.Instance.AddBuff(critBuff);
//     }

//     void OnBulletFired(BulletFiredEvent e)
//     {
//         if (!pendingCrit) return;
//         pendingCrit = false;
//         RemoveCritBuff();
//     }

//     void RemoveCritBuff()
//     {
//         if (critBuff == null) return;
//         Character.Instance.RemoveBuff(critBuff);
//     }
// }

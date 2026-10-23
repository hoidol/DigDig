// public class PierceBerserkerItem : Item, IToggle, IBullet
// {
//     const float HP_THRESHOLD = 0.3f;
//     const float ATTACK_BONUS = 0.2f;

//     Buff buff;
//     bool isOn;

//     public override void OnEquip()
//     {
//         buff = new Buff(StatType.AttackPower, 1f + ATTACK_BONUS, StatOpType.Multiply);
//     }

//     public override void OnUnequip()
//     {
//         if (isOn) OnTurnOff();
//     }

//     public override void UpdateItem()
//     {
//         // count 변화 시 버프 세기 갱신
//         bool wasOn = isOn;
//         if (wasOn) OnTurnOff();
//         buff = new Buff(StatType.AttackPower, 1f + ATTACK_BONUS, StatOpType.Multiply);
//         if (wasOn) OnTurnOn();
//     }

//     void Update()
//     {
//         var player = Character.Instance;
//         bool condition = CheckCondition();
//         if (condition && !isOn) OnTurnOn();
//         else if (!condition && isOn) OnTurnOff();
//     }

//     public bool CheckCondition()
//     {
//         return Character.Instance.curHp / Character.Instance.statMgr.MaxHp < HP_THRESHOLD;
//     }

//     public void OnTurnOn()
//     {
//         isOn = true;
//         Character.Instance.AddBuff(buff);
//     }

//     public void OnTurnOff()
//     {
//         isOn = false;
//         Character.Instance.RemoveBuff(buff);
//     }
//     public int pierceCount = 2;

//     public void OnBulletFired(CharacterBulletObject bullet)
//     {
//         bullet.AddBehavior(new PierceBehavior(pierceCount));
//     }
// }

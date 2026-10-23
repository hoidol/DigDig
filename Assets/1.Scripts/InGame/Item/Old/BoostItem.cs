// using System.Collections.Generic;
// using UnityEngine;

// //미니미 count만큼 소환 및 처치 시 미니미 추가탄 발사 \n쿨타임 : {coolTimes[lv-1]}초, 발사 당 체력 -{itemData.consumeHp}
// public class BoostItem : Item
// {
//     public BoostMiniMe boostMiniMePrefab;
//     public List<BoostMiniMe> boostMiniMes = new();

//     public float consumeTime = 5;

//     float attackPower = 4f;
//     float attackSpeed =  2.5f;

//     float coolTime=2f;

//     public override void OnUnequip()
//     {
//         base.OnUnequip();
//         foreach (BoostMiniMe boostMiniMe in boostMiniMes)
//         {
//             Destroy(boostMiniMe.gameObject);
//         }
//         boostMiniMes.Clear();
//     }

//     public override void UpdateItem()
//     {
//         base.UpdateItem();

//         while (boostMiniMes.Count < count)
//         {
//             BoostMiniMe boostMiniMe = Instantiate(boostMiniMePrefab);
//             Vector2 pos = (Vector2)Character.Instance.transform.position + Random.insideUnitCircle.normalized;
//             // boostMiniMe.Spawn(pos);
//             boostMiniMes.Add(boostMiniMe);
//         }
//         while (boostMiniMes.Count > count)
//         {
//             BoostMiniMe boostMiniMe = boostMiniMes[^1];
//             boostMiniMes.RemoveAt(boostMiniMes.Count - 1);
//             Destroy(boostMiniMe.gameObject);
//         }

//         foreach (BoostMiniMe boostMiniMe in boostMiniMes)
//         {
//             // boostMiniMe.SetLevel(count);
//             // boostMiniMe.attackPower = attackPower;
//             // boostMiniMe.attackSpeed = attackSpeed;
//             boostMiniMe.coolTime = coolTime;
//         }
//     }


//     float timer;
//     void Update()
//     {
//         timer += Time.deltaTime;
//         if (timer >= consumeTime)
//         {
//             Character.Instance.AddHp(-itemData.consumeHp);

//             timer = 0;
//         }
//     }



//     public override string GetDescription()
//     {
//         return $"처지 시 {count*2}개 추가 탄 발사하는 미니미 소환, \n쿨타임 : {coolTime}초, {consumeTime}초 당 체력 -{itemData.consumeHp}";
//         //return string.Format(TranslateManager.GetText($"{key}_Desc"),count*2,coolTime,consumeTime,itemData.consumeHp);
//     }




// }
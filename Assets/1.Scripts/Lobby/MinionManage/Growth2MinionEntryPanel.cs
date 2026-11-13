// using UnityEngine;
// namespace Lobby
// {
//     public class Growth2MinionEntryPanel : MinionEntryPanel
//     {
//         //현재 Growth1 조합을 통해서 뽑을 수 있는 것만 - 구매 유도하자
//         MinionMergeData minionMergeData;
//         public GameObject unownPanel;
//         public GameObject canMergePanel;
//         public bool canSpawn;
//         public bool selling;
//         public override void SetData(MinionData minionData)
//         {
//             base.SetData(minionData);

//             if (minionData == null)
//             {
//                 gameObject.SetActive(false);
//                 return;
//             }
//             gameObject.SetActive(true);
//             minionMergeData = MinionManager.Instance.GetMinionMergeData(minionData.key);
//         }
//         public override void UpdatePanel()
//         {
//             if (minionData == null)
//             {
//                 gameObject.SetActive(false);
//                 return;
//             }
//             gameObject.SetActive(true);

//             unownPanel.SetActive(false);
//             canMergePanel.SetActive(false);

//             canSpawn = minionMergeData.CanSpawn();
//             canMergePanel.SetActive(canSpawn);
//             if (minionMergeData.sell) //구매해야됌
//             {
//                 //보유중
//                 if (!userMinion.own)
//                 {
//                     selling = true;
//                     unownPanel.SetActive(true);
//                 }
//             }
//         }

//     }
// }

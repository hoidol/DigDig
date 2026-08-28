using UnityEngine;
namespace Lobby
{
    public class Growth2MiniMeEntryPanel : MiniMeEntryPanel
    {
        //현재 Growth1 조합을 통해서 뽑을 수 있는 것만 - 구매 유도하자
        MiniMeMergeData miniMeMergeData;
        public GameObject sellPanel;
        public GameObject canSpawnPanel;
        public bool canSpawn;
        public bool selling;
        public override void SetData(MiniMeData miniData)
        {
            base.SetData(miniData);
            miniMeMergeData = MiniMeManager.Instance.GetMiniMeMergeData(miniData.key); 
        }
        public override void UpdatePanel()
        {
            sellPanel.SetActive(false);
            canSpawnPanel.SetActive(false);

            canSpawn = miniMeMergeData.CanSpawn();
            canSpawnPanel.SetActive(canSpawn);
            if (miniMeMergeData.sell) //구매해야됌
            {
                //보유중
                if (!userMiniMe.own)
                {
                    selling = true;
                    sellPanel.SetActive(true);
                    canSpawnPanel.SetActive(false);
                }
            }
        }

    }    
}

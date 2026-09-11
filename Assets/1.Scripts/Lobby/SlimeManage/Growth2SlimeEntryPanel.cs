using UnityEngine;
namespace Lobby
{
    public class Growth2SlimeEntryPanel : SlimeEntryPanel
    {
        //현재 Growth1 조합을 통해서 뽑을 수 있는 것만 - 구매 유도하자
        SlimeMergeData slimeMergeData;
        public GameObject unownPanel;
        public GameObject canMergePanel;
        public bool canSpawn;
        public bool selling;
        public override void SetData(SlimeData slimeData)
        {
            base.SetData(slimeData);
            slimeMergeData = SlimeManager.Instance.GetSlimeMergeData(slimeData.key); 
        }
        public override void UpdatePanel()
        {
            unownPanel.SetActive(false);
            canMergePanel.SetActive(false);

            canSpawn = slimeMergeData.CanSpawn();
            canMergePanel.SetActive(canSpawn);
            if (slimeMergeData.sell) //구매해야됌
            {
                //보유중
                if (!userSlime.own)
                {
                    selling = true;
                    unownPanel.SetActive(true);
                }
            }
        }

    }    
}

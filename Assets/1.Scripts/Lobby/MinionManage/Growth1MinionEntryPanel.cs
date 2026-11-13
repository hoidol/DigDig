using UnityEngine;
namespace Lobby
{
    public class Growth1MinionEntryPanel : MinionEntryPanel
    {
        public GameObject unownPanel;

        public override void UpdatePanel()
        {
            if(minionData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            unownPanel.SetActive(false);

            if(!userMinion.own)
            {                
                unownPanel.SetActive(true);
            }

        }
    }    
}
